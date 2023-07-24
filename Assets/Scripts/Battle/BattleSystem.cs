using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Initializing, ActionSelection, MoveSelection, PartyScreen, RunningTurn, AfterTurn, SwitchingPokemon, BattleOver }
public enum BattleAction { Move, SwitchPokemon, UseItem, Run }
public class Team
{
    List<Screen> _screenList = new List<Screen>();
    public List<Screen> TeamScreens { get { return _screenList; } set { _screenList = value; } }
}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] UIBattleManager _uiBattleManager;

    public event Action<bool> OnBattleOver;
    public event Action OnBattleStart;

    BattleState _state;
    BattleState? _preState;
    int _currentAction;
    int _currentMove;
    int _currentMember;
    WeatherManager _weatherManager = new WeatherManager();
    BattleCalculator _battleCalculator = new BattleCalculator();

    PokemonParty _playerParty;
    Pokemon _wildPokemon;
    Team _playerTeam = new Team();
    Team _enemyTeam = new Team();

    Dictionary<BattleState, BattleStateBase> _stateDictionary;
    SwitchingPokemonState _busyState;
    BattleOverState _battleOverState;
    RunningTurnState _runningTurnState;
    AfterTurnState _afterTurnState;
    List<BattleUnit> _turnOrder;
    List<BattleUnit> _activePokemon;
    public UIBattleManager UIBattleManager => _uiBattleManager;
    public WeatherManager WeatherManager => _weatherManager;
    public BattleCalculator BattleCalculator => _battleCalculator;
    public BattleStateBase CurrentState { get; private set; }
    public int CurrentAction { get { return _currentAction; } set { _currentAction = value; } }
    public int CurrentMove { get { return _currentMove; } set { _currentMove = value; } }
    public int CurrentMember { get { return _currentMember; } set { _currentMember = value; } }
    public PokemonParty PlayerParty { get { return _playerParty; } private set { _playerParty = value; } }
    public BattleState State { get { return _state; } private set { _state = value; } }
    public BattleState? PreState { get { return _preState; } set { _preState = value; } }
    public Team PlayerTeam { get { return _playerTeam; } private set { _playerTeam = value; } }
    public Team EnemyTeam { get { return _enemyTeam; } private set { _enemyTeam = value; } }
    public BattleUnit FaintedUnit { get; private set; }
    public List<BattleUnit> TurnOrder { get { return _turnOrder; } set { _turnOrder = value; } }
    public List<BattleUnit> ActivePokemon { get { return _activePokemon; } set { _activePokemon = value; } }
    void Start()
    {
        _runningTurnState = new RunningTurnState(this);
        _afterTurnState = new AfterTurnState(this);
        _busyState = new SwitchingPokemonState(this);
        _battleOverState = new BattleOverState(this);
        _turnOrder = new List<BattleUnit>();
        _activePokemon = new List<BattleUnit>();
        AbilityManager.Instance.SetWeatherManager(_weatherManager);
        _stateDictionary = new Dictionary<BattleState, BattleStateBase>
        {
            { BattleState.ActionSelection, new ActionSelectionState(this) },
            { BattleState.AfterTurn, new AfterTurnState(this) },
            { BattleState.BattleOver, new BattleOverState(this) },
            { BattleState.MoveSelection, new MoveSelectionState(this) },
            { BattleState.PartyScreen, new PartyScreenState(this) },
            { BattleState.RunningTurn, new RunningTurnState(this) },
            { BattleState.SwitchingPokemon, new SwitchingPokemonState(this) },
    };
        var moveSelectionState = (MoveSelectionState)_stateDictionary[BattleState.MoveSelection];
        var runningTurnState = (RunningTurnState)_stateDictionary[BattleState.RunningTurn];
        var partyScreenSt = (PartyScreenState)_stateDictionary[BattleState.PartyScreen];
        var busyState = (SwitchingPokemonState)_stateDictionary[BattleState.SwitchingPokemon];
        var afterTurnState = (AfterTurnState)_stateDictionary[BattleState.AfterTurn];

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
        CurrentState = _stateDictionary[nextState];
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
        _uiBattleManager.UpdateWeatherImage(environmentWeather);
        StartCoroutine(SetupBattle(environmentWeather));
        OnBattleStart?.Invoke();
    }

    public IEnumerator SetupBattle(Weather environmentWeather)
    {
        _uiBattleManager.SetupPlayerParty();
        _uiBattleManager.SetupEnemyPokemon(_wildPokemon);

        yield return _uiBattleManager.DialogueBox.TypeDialogue($"A wild { _uiBattleManager.ActiveEnemyUnit.Pokemon.Base.Name } appeared.");
        yield return _uiBattleManager.FaintDelay;
        yield return _weatherManager.SetInitialWeather(environmentWeather);
        yield return InitialSkillsSetup(_uiBattleManager.ActivePlayerUnit, _uiBattleManager.ActiveEnemyUnit);
        TransitionToState(BattleState.ActionSelection);
    }

    IEnumerator InitialSkillsSetup(BattleUnit playerUnit, BattleUnit enemyUnit)
    {
        _activePokemon.Add(playerUnit);
        _activePokemon.Add(enemyUnit);
        _activePokemon = _afterTurnState.SortedPokemonBySpeed(_activePokemon);

        foreach (var unit in _activePokemon)
        {
            if (unit.IsPlayerTeam)
                yield return _busyState.PokemonSwitchAbility(playerUnit.Pokemon, enemyUnit.Pokemon);
            else
                yield return _busyState.PokemonSwitchAbility(enemyUnit.Pokemon, playerUnit.Pokemon);
        }

        _activePokemon.Clear();
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
            FaintedUnit = unit;
            if (ActivePokemon.Contains(unit))
                ActivePokemon.Remove(unit);
            unit.PlayFaintAnimation();
            yield return _uiBattleManager.FaintDelay;
            yield return _uiBattleManager.DialogueBox.TypeDialogue($"{ unit.Pokemon.Base.Name } has fainted.");
            yield return _uiBattleManager.PressAnyKeyToContinue;
            TransitionToState(BattleState.BattleOver);
        }
    }
    public IEnumerator ScreensAfterTurn()
    {
        if (_playerTeam.TeamScreens.Count > 0)
        {
            foreach (var screen in _playerTeam.TeamScreens)
            {
                screen.Duration--;
                if (screen.Duration == 0)
                {
                    yield return UIBattleManager.DialogueBox.TypeDialogue(screen.PlayerEndMessage);
                    _playerTeam.TeamScreens.Remove(screen);
                }
            }
        }

        //check if the enemy has any screen and run its dialogue
        if (_enemyTeam.TeamScreens.Count > 0)
        {
            foreach (var screen in _enemyTeam.TeamScreens)
            {
                screen.Duration--;
                if (screen.Duration == 0)
                {
                    yield return UIBattleManager.DialogueBox.TypeDialogue(screen.EnemyEndMessage);
                    _enemyTeam.TeamScreens.Remove(screen);
                }
            }
        }
    }
    public void RaiseBattleOverEvent(bool won)
    {
        //unsubscribe to all events from the weather when the battle is over, and invoke the onbattleover
        _uiBattleManager.BattleOver(won);
        OnBattleOver?.Invoke(won);
    }
}
