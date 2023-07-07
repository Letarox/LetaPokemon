using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Initializing, ActionSelection, MoveSelection, PartyScreen, RunningTurn, AfterTurn, SwitchingPokemon, BattleOver }
public enum BattleAction { Move, SwitchPokemon, UseItem, Run }
public class Team
{
    List<Screen> _screenList = new List<Screen>();
    public List<Screen> TeamScreens { get { return _screenList; } set { } }
}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit _playerUnit, _enemyUnit;
    [SerializeField] BattleDialogueBox _dialogueBox;
    [SerializeField] BattleAbilityBox _abilityBox;
    [SerializeField] PartyScreen _partyScreen;
    [SerializeField] SpriteRenderer _weatherImage;
    [SerializeField] List<Sprite> _weathers;

    public event Action<bool> OnBattleOver;

    BattleState _state;
    BattleState? _preState;
    int _currentAction;
    int _currentMove;
    int _currentMember;
    Weather _currentWeather;

    WaitUntil _pressAnyKeyToContinue, _waitUntilRunningTurn;
    WaitForSeconds _attackDelay, _faintDelay, _weatherDelay;

    PokemonParty _playerParty;
    Pokemon _wildPokemon;
    Team _playerTeam = new Team();
    Team _enemyTeam = new Team();

    private Dictionary<BattleState, BattleStateBase> stateDictionary;
    private PartyScreenState partyScreenState;
    private SwitchingPokemonState _busyState;
    private BattleOverState battleOverState;
    private RunningTurnState _runningTurnState;
    private List<BattleUnit> _turnOrder;

    public BattleUnit ActivePlayerUnit => _playerUnit;
    public BattleUnit ActiveEnemyUnit => _enemyUnit;
    public BattleDialogueBox DialogueBox => _dialogueBox;
    public BattleAbilityBox AbilityBox => _abilityBox;
    public PartyScreen PartyScreen => _partyScreen;
    public SpriteRenderer WeatherImage => _weatherImage;
    public List<Sprite> Weathers => _weathers;
    public WaitForSeconds AttackDelay => _attackDelay;
    public WaitForSeconds FaintDelay => _faintDelay;
    public WaitForSeconds WeatherDelay => _weatherDelay;
    public WaitUntil WaintUntilRunningTurn => _waitUntilRunningTurn;
    public BattleStateBase CurrentState { get; private set; }
    public int CurrentAction { get { return _currentAction; } set { _currentAction = value; } }
    public int CurrentMove { get { return _currentMove; } set { _currentMove = value; } }
    public int CurrentMember { get { return _currentMember; } set { _currentMember = value; } }
    public Move ChosenMove { get; set; }
    public PokemonParty PlayerParty { get { return _playerParty; } private set { _playerParty = value; } }
    public Weather CurrentWeather { get { return _currentWeather; } set { _currentWeather = value; } }
    public BattleState State { get { return _state; } private set { _state = value; } }
    public BattleState? PreState { get { return _preState; } set { _preState = value; } }
    public Team PlayerTeam { get { return _playerTeam; } private set { _playerTeam = value; } }
    public Team EnemyTeam { get { return _enemyTeam; } private set { _enemyTeam = value; } }
    public BattleUnit FaintedUnit { get; private set; }
    public List<BattleUnit> TurnOrder { get { return _turnOrder; } set { _turnOrder = value; } }
    void Start()
    {
        _pressAnyKeyToContinue = new WaitUntil(() => Input.GetKeyDown(KeyCode.X));
        _waitUntilRunningTurn = new WaitUntil(() => _state == BattleState.RunningTurn);
        _attackDelay = new WaitForSeconds(1.5f);
        _faintDelay = new WaitForSeconds(1f);
        _weatherDelay = new WaitForSeconds(0.25f);
        partyScreenState = new PartyScreenState(this);
        _runningTurnState = new RunningTurnState(this);
        _busyState = new SwitchingPokemonState(this);
        battleOverState = new BattleOverState(this);
        TurnOrder = new List<BattleUnit>();
        stateDictionary = new Dictionary<BattleState, BattleStateBase>
        {
            { BattleState.ActionSelection, new ActionSelectionState(this) },
            { BattleState.BattleOver, new BattleOverState(this) },
            { BattleState.SwitchingPokemon, new SwitchingPokemonState(this) },
            { BattleState.MoveSelection, new MoveSelectionState(this) },
            { BattleState.PartyScreen, new PartyScreenState(this) },
            { BattleState.RunningTurn, new RunningTurnState(this) },
            { BattleState.AfterTurn, new AfterTurnState(this) },
    };
        var moveSelectionState = (MoveSelectionState)stateDictionary[BattleState.MoveSelection];
        var runningTurnState = (RunningTurnState)stateDictionary[BattleState.RunningTurn];
        var partyScreenSt = (PartyScreenState)stateDictionary[BattleState.PartyScreen];
        var busyState = (SwitchingPokemonState)stateDictionary[BattleState.SwitchingPokemon];
        var afterTurnState = (AfterTurnState)stateDictionary[BattleState.AfterTurn];

        // Set the runningTurnState in the MoveSelectionState
        moveSelectionState.SetRunningTurnState(runningTurnState);
        busyState.SetRunningTurnState(runningTurnState);
        partyScreenSt.SetBusyState(busyState);
        afterTurnState.SetRunningTurnState(runningTurnState);
    }
    void Update()
    {
        if (CurrentState != null)
        {
            CurrentState.UpdateState();
        }
    }

    public void TransitionToState(BattleState nextState, Action callback = null)
    {        
        CurrentState?.ExitState();        
        CurrentState = stateDictionary[nextState];
        CurrentState.EnterState();
        _state = nextState;        
        callback?.Invoke();
    }

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon, Weather environmentWeather)
    {
        _playerParty = playerParty;
        _wildPokemon = wildPokemon;
        _currentAction = 0;
        _currentMove = 0;
        _currentWeather = environmentWeather;
        StartCoroutine(SetupBattle());
        if (_currentWeather.Id != WeatherID.None)
        {
            _currentWeather.Duration = 1;
            _currentWeather.EnvironmentWeather = true;
            UpdateWeatherImage(_currentWeather.Id);
        }
    }

    public IEnumerator SetupBattle()
    {
        SetupPlayerParty();
        SetupEnemyPokemon();

        yield return _dialogueBox.TypeDialogue($"A wild { _enemyUnit.Pokemon.Base.Name } appeared.");
        if (_currentWeather.Id != WeatherID.None)
            yield return _dialogueBox.TypeDialogue($"{ _currentWeather.StartMessage }");
        yield return _faintDelay;

        yield return _busyState.PokemonSwitchAbility(_playerUnit.Pokemon, _enemyUnit.Pokemon);
        yield return _busyState.PokemonSwitchAbility(_enemyUnit.Pokemon, _playerUnit.Pokemon);

        TransitionToState(BattleState.ActionSelection);
    }

    private void SetupPlayerParty()
    {
        _playerUnit.Setup(_playerParty.GetHealthyPokemon());
        _dialogueBox.SetMoveNames(_playerUnit.Pokemon.Moves);
    }

    private void SetupEnemyPokemon()
    {
        _enemyUnit.Setup(_wildPokemon);
    }
    public void HandleUpdate()
    {
        //Based on which state the battle is currently on, we handle which selection the player can make
        switch (_state)
        {
            case BattleState.ActionSelection:
                TransitionToState(BattleState.ActionSelection);
                break;
            case BattleState.MoveSelection:
                TransitionToState(BattleState.MoveSelection);
                break;
            case BattleState.PartyScreen:
                TransitionToState(BattleState.PartyScreen);
                break;
        }
    }
    public IEnumerator CheckForFaint(BattleUnit unit)
    {
        //check if the unit has fainted, if it has, apply its animation and dialogue and check if the battle can continue
        if (unit.Pokemon.HP <= 0)
        {
            TurnOrder.Remove(unit);
            FaintedUnit = unit;
            unit.PlayFaintAnimation();
            yield return _faintDelay;
            yield return _dialogueBox.TypeDialogue($"{ unit.Pokemon.Base.Name } has fainted.");
            yield return _pressAnyKeyToContinue;
            TransitionToState(BattleState.BattleOver);
        }
    }

    public void UpdateWeatherImage(WeatherID weatherID)
    {
        _weatherImage.sprite = _weathers[(int)weatherID];
    }
    
    public IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        //Dequeues the list of messages of status changes for the pokemon
        while(pokemon.StatusChanges.Count > 0)
        {
            string message = pokemon.StatusChanges.Dequeue();
            yield return _dialogueBox.TypeDialogue(message);
        }
    }

    public void RaiseBattleOverEvent(bool won)
    {
        OnBattleOver?.Invoke(won);
    }
}
