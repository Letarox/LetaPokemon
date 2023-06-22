using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, ActionSelection, MoveSelection, PartyScreen, PerformMove, Busy, BattleOver }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit _playerUnit, _enemyUnit;
    [SerializeField] BattleDialogueBox _dialogueBox;
    [SerializeField] PartyScreen _partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState _state;
    int _currentAction;
    int _currentMove;
    int _currentMember;

    WaitUntil _pressAnyKeyToContinue;
    WaitForSeconds _attackDelay, _faintDelay;

    PokemonParty _playerParty;
    Pokemon _wildPokemon;

    void Start()
    {
        _pressAnyKeyToContinue = new WaitUntil(() => Input.GetKeyDown(KeyCode.X));
        _attackDelay = new WaitForSeconds(1.5f);
        _faintDelay = new WaitForSeconds(0.5f);        
    }

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        _playerParty = playerParty;
        _wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());
        _currentAction = 0;
        _currentMove = 0;
    }

    public void HandleUpdate()
    {
        switch (_state)
        {
            case BattleState.ActionSelection:
                HandleActionSelection();
                break;
            case BattleState.MoveSelection:
                HandleMoveSelection();
                break;
            case BattleState.PartyScreen:
                HandlePartyScreenSelection();
                break;
        }
    }    

    public IEnumerator SetupBattle()
    {
        _playerUnit.Setup(_playerParty.GetHealthyPokemon());
        _enemyUnit.Setup(_wildPokemon);

        _dialogueBox.SetMoveNames(_playerUnit.Pokemon.Moves);      

        yield return _dialogueBox.TypeDialogue(($"A wild { _enemyUnit.Pokemon.Base.Name } appeared."));
        yield return _faintDelay;

        ActionSelection();
    }

    void ChooseFirstTurn()
    {
        if (_playerUnit.Pokemon.Speed > _enemyUnit.Pokemon.Speed)
            ActionSelection();
        else if (_playerUnit.Pokemon.Speed < _enemyUnit.Pokemon.Speed)
            StartCoroutine(PerformEnemyMove());
        else
        {
            bool lucky = UnityEngine.Random.Range(1, 101) <= 50 ? true : false;
            if (lucky)
                ActionSelection();
            else
                StartCoroutine(PerformEnemyMove());
        }
    }

    void BattleOver(bool won)
    {
        _state = BattleState.BattleOver;
        _playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    void ActionSelection()
    {
        _state = BattleState.ActionSelection;
        _dialogueBox.SetDialogue("Choose an action.");
        _dialogueBox.EnableActionSelector(true);
    }

    void MoveSelection()
    {
        _state = BattleState.MoveSelection;
        _dialogueBox.EnableActionSelector(false);
        _dialogueBox.EnableDialogueText(false);
        _dialogueBox.EnableMoveSelector(true);
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            _currentAction++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            _currentAction--;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            _currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            _currentAction -= 2;

        _currentAction = Mathf.Clamp(_currentAction, 0, 3);

        _dialogueBox.UpdateActionSelection(_currentAction);

        if (Input.GetKeyDown(KeyCode.X))
        {
            switch (_currentAction)
            {
                case 0: //FIGHT
                    MoveSelection();
                    break;
                case 1: //BAG
                    break;
                case 2: //POKEMON
                    OpenPartyScreen();
                    break;
                case 3: //RUN
                    break;
                default:
                    break;
            }
        }
    }
    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            _currentMove++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            _currentMove--;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            _currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            _currentMove -= 2;

        _currentMove = Mathf.Clamp(_currentMove, 0, _playerUnit.Pokemon.Moves.Count - 1);

        _dialogueBox.UpdateMoveSelection(_currentMove, _playerUnit.Pokemon.Moves[_currentMove], _enemyUnit.Pokemon);

        if (Input.GetKeyDown(KeyCode.X))
        {
            _dialogueBox.EnableMoveSelector(false);
            _dialogueBox.EnableDialogueText(true);
            StartCoroutine(PlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            _dialogueBox.EnableMoveSelector(false);
            ActionSelection();
        }
    }
    void HandlePartyScreenSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            _currentMember++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            _currentMember--;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            _currentMember += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            _currentMember -= 2;

        _currentMember = Mathf.Clamp(_currentMember, 0, _playerParty.Pokemons.Count - 1);

        _partyScreen.UpdatePokemonSelection(_currentMember);

        if (Input.GetKeyDown(KeyCode.X))
        {
            Pokemon selectedMember = _playerParty.Pokemons[_currentMember];
            if (selectedMember.HP <= 0)
            {
                _partyScreen.SetMessageText("You can't send out a fainted Pokemon!");
                return;
            }
            if (selectedMember == _playerUnit.Pokemon)
            {
                _partyScreen.SetMessageText($"{ selectedMember.Base.Name } is already fighting!");
                return;
            }

            _partyScreen.gameObject.SetActive(false);
            _state = BattleState.Busy;
            StartCoroutine(SwitchPokemon(selectedMember));
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && _playerUnit.Pokemon.HP > 0)
        {
            _playerUnit.ReAppearUnit();
            _enemyUnit.ReAppearUnit();
            _partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }
    void OpenPartyScreen()
    {
        _state = BattleState.PartyScreen;
        _partyScreen.SetPartyData(_playerParty.Pokemons);
        _partyScreen.gameObject.SetActive(true);
        _playerUnit.HideUnit();
        _enemyUnit.HideUnit();
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        _playerUnit.ReAppearUnit();
        _enemyUnit.ReAppearUnit();
        bool fainted = (_playerUnit.Pokemon.HP > 0) ? false : true;
        if(!fainted)
        {
            yield return _dialogueBox.TypeDialogue($"Come back { _playerUnit.Pokemon.Base.Name }!");
            _playerUnit.PlayFaintAnimation();
        }

        yield return _dialogueBox.TypeDialogue($"Go { newPokemon.Base.Name }!");
        yield return _faintDelay;
        _playerUnit.Setup(newPokemon);
        _dialogueBox.SetMoveNames(newPokemon.Moves);
        yield return _attackDelay;

        if (!fainted)
            StartCoroutine(PerformEnemyMove());
        else
            ActionSelection();
    }    

    IEnumerator PlayerMove()
    {
        _state = BattleState.Busy;
        Move move = _playerUnit.Pokemon.Moves[_currentMove];

        yield return RunMove(_playerUnit, _enemyUnit, move);

        //We only proceed with the enemy move in case the battle is not over
        if(_state == BattleState.PerformMove)
            StartCoroutine(PerformEnemyMove());
    }

    IEnumerator PerformEnemyMove()
    {
        Move move = _enemyUnit.Pokemon.GetRandomMove();

        yield return RunMove(_enemyUnit, _playerUnit, move);

        //We only proceed with the player move in case the battle is not over
        if (_state == BattleState.PerformMove)
            ActionSelection();
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move) 
    {
        _state = BattleState.PerformMove;
        move.PP--;

        yield return _dialogueBox.TypeDialogue($"{ sourceUnit.Pokemon.Base.Name } used { move.Base.Name }. ");

        //Generates a random number between 1 and 10 thousand, multiplying by the accuracy of the move and accuracy of the pokemon, applying the stat changes. Both numbers are greatly increased to account for decimal values
        bool missed = (UnityEngine.Random.Range(1.00f, 100.00f) <= (move.Base.Accuracy * sourceUnit.Pokemon.Accuracy * targetUnit.Pokemon.Evasion)) ? false : true;
        if (!missed)
        {
            sourceUnit.PlayAttackAnimation();
            yield return _attackDelay;

            if(move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move, sourceUnit.Pokemon, targetUnit.Pokemon);
            }
            else
            {
                DamageDetails damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                targetUnit.PlayHitAnimation();
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails, targetUnit.Pokemon);
            }            
        }
        else
        {
            yield return _dialogueBox.TypeDialogue($"{ sourceUnit.Pokemon.Base.Name }'s attack missed!");
            yield return _attackDelay;
        }

        if (targetUnit.Pokemon.HP <= 0)
        {
            targetUnit.PlayFaintAnimation();
            yield return _faintDelay;
            yield return _dialogueBox.TypeDialogue($"{ targetUnit.Pokemon.Base.Name } has fainted.");
            yield return _pressAnyKeyToContinue;

            CheckForBattleOver(targetUnit);
        }
    }

    IEnumerator RunMoveEffects(Move move, Pokemon source, Pokemon target)
    {
        if (move.Base.Effects.Boosts != null)
        {
            if (move.Base.Target == MoveTarget.Foe)  
                target.ApplyBoost(move.Base.Effects.Boosts);
            else
                source.ApplyBoost(move.Base.Effects.Boosts);
        }

        if (source.StatusChanges.Count > 0)
            yield return ShowStatusChanges(source);
        if (target.StatusChanges.Count > 0)
            yield return ShowStatusChanges(target);
    }

    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while(pokemon.StatusChanges.Count > 0)
        {
            string message = pokemon.StatusChanges.Dequeue();
            yield return _dialogueBox.TypeDialogue(message);
        }
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerTeam)
        {
            Pokemon nextPokemon = _playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
                OpenPartyScreen();
            else
                BattleOver(false);
        }
        else
            BattleOver(true);
    }


    IEnumerator ShowDamageDetails(DamageDetails damageDetails, Pokemon attackedPokemon)
    {
        if (damageDetails.Critical > 1f)
            yield return _dialogueBox.TypeDialogue("A critical hit!");        
        if (damageDetails.TypeEffectiveness == 1f)
            yield return null;
        else
        {
            if (damageDetails.TypeEffectiveness == 0f)
                yield return _dialogueBox.TypeDialogue($"It doesn't affect enemy { attackedPokemon.Base.Name }.");
            else
                if (damageDetails.TypeEffectiveness > 1f)
                    yield return _dialogueBox.TypeDialogue("Its super effective!");
                else
                    yield return _dialogueBox.TypeDialogue("Its not very effective.");
        }
    }
}
