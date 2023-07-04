using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, ActionSelection, MoveSelection, PartyScreen, RunningTurn, Busy, BattleOver }
public enum BattleAction { Move, SwitchPokemon, UseItem, Run }
public enum Screen { None, LightScreen, Reflect, AuroraVeil }
public class Team
{
    BattleUnit _currentBattleUnit;
    List<Screen> _screenList;
    bool _isPlayerTeam;
    public BattleUnit CurrentBattleUnit { get; set; }
    public List<Screen> ScreenList { get; set; }
    public bool IsPlayerTeam { get; set; }

}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit _playerUnit, _enemyUnit;
    [SerializeField] BattleDialogueBox _dialogueBox;
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
    Team _playerTeam;
    Team _enemyTeam;

    void Start()
    {
        _pressAnyKeyToContinue = new WaitUntil(() => Input.GetKeyDown(KeyCode.X));
        _waitUntilRunningTurn = new WaitUntil(() => _state == BattleState.RunningTurn);
        _attackDelay = new WaitForSeconds(1.5f);
        _faintDelay = new WaitForSeconds(1f);
        _weatherDelay = new WaitForSeconds(0.25f);
    }

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon, Weather environmentWeather)
    {
        _playerParty = playerParty;
        _wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());
        _currentAction = 0;
        _currentMove = 0;
        _currentWeather = environmentWeather;
        if (_currentWeather.Id != WeatherID.None)
        {
            _currentWeather.Duration = 1;
            _currentWeather.EnvironmentWeather = true;
            UpdateWeatherImage(_currentWeather.Id);
        }
    }

    public void HandleUpdate()
    {
        //Based on which state the battle is currently on, we handle which selection the player can make
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
        //Set the values for both player and enemy pokemons
        _playerUnit.Setup(_playerParty.GetHealthyPokemon());
        _playerTeam.CurrentBattleUnit = _playerUnit;
        _playerTeam.IsPlayerTeam = true;
        _enemyUnit.Setup(_wildPokemon);
        _enemyTeam.CurrentBattleUnit = _enemyUnit;
        _enemyTeam.IsPlayerTeam = false;

        _dialogueBox.SetMoveNames(_playerUnit.Pokemon.Moves);      

        yield return _dialogueBox.TypeDialogue(($"A wild { _enemyUnit.Pokemon.Base.Name } appeared."));
        if (_currentWeather != WeatherDB.Weathers[WeatherID.None])
            yield return _dialogueBox.TypeDialogue($"{ _currentWeather.StartMessage }");
        yield return _faintDelay;

        ActionSelection();
    }

    void BattleOver(bool won)
    {
        //Update the state for battle over and apply the battle over on each pokemon
        _state = BattleState.BattleOver;
        _playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    void ActionSelection()
    {
        //When selecting which action to take, the player should be able to see the dialogue and the action selector
        _state = BattleState.ActionSelection;
        _dialogueBox.SetDialogue("Choose an action.");
        _dialogueBox.EnableActionSelector(true);
        _dialogueBox.EnableMoveSelector(false);
        _dialogueBox.EnableDialogueText(true);
    }
    void HandleActionSelection()
    {
        //Handle how the action selection of the player works, how to move the arrow keys to change the currentAction, and which method to call when the X button is pressed
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
                    _preState = _state;
                    OpenPartyScreen();
                    break;
                case 3: //RUN
                    break;
                default:
                    break;
            }
        }
    }
    void MoveSelection()
    {
        //When selection which move to use, the player should not be able to see his action selector or dialogue
        _state = BattleState.MoveSelection;
        _dialogueBox.EnableActionSelector(false);
        _dialogueBox.EnableDialogueText(false);
        _dialogueBox.EnableMoveSelector(true);
    }
    void HandleMoveSelection()
    {
        //Handle how the move selection of the player works, how to move the arrow keys to change the currentMove, and what happens when X is pressed
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
            if (_playerUnit.Pokemon.Moves[_currentMove].PP == 0) 
                return;
            _dialogueBox.EnableMoveSelector(false);
            _dialogueBox.EnableDialogueText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            ActionSelection();
        }
    }
    void HandlePartyScreenSelection()
    {
        //Handle how the party screen selection of the player works, how to move the arrow keys to change the currentMember, and which method to call when the X button is pressed
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

            //prevents the player from choosing a fainted pokemon
            if (selectedMember.HP <= 0)
            {
                _partyScreen.SetMessageText("You can't send out a fainted Pokemon!");
                return;
            }

            //prevents the player from choosing the current pokemon in-battle
            if (selectedMember == _playerUnit.Pokemon)
            {
                _partyScreen.SetMessageText($"{ selectedMember.Base.Name } is already fighting!");
                return;
            }

            _partyScreen.gameObject.SetActive(false);

            //if the player changed pokemon manually, allow the enemy to attack, otherwise just changes his pokemon
            if(_preState == BattleState.ActionSelection)
            {
                _preState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                _state = BattleState.Busy;
                StartCoroutine(SwitchPokemon(selectedMember));
            }
        }
        //Prevents the player from closing the PartyScreen in case his pokemon faints, or closes the screen otherwise
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
        //update the state and brings the PartyScreen to the UI
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

        //if it was a manual change, adds the "come back" text
        if(_playerUnit.Pokemon.HP > 0)
        {
            yield return _dialogueBox.TypeDialogue($"Come back { _playerUnit.Pokemon.Base.Name }!");
            _playerUnit.PlayFaintAnimation();
            yield return _faintDelay;
        }

        //updates the dialogue accordingly with the new pokemon and setup its information for the player
        yield return _faintDelay;
        _playerUnit.Setup(newPokemon);
        _dialogueBox.SetMoveNames(newPokemon.Moves);
        yield return _dialogueBox.TypeDialogue($"Go { newPokemon.Base.Name }!");
        yield return _faintDelay;

        _state = BattleState.RunningTurn;
    }
    bool CheckFirstTurn(Pokemon playerPokemon, Move playerMove, Pokemon enemyPokemon, Move enemyMove)
    {
        //check if any move has priority, otherwise just checks whoever is faster. In case of speed-tie a random pokemon will be selected
        if (playerMove.Base.Priority > enemyMove.Base.Priority)
            return true;
        else if (playerMove.Base.Priority < enemyMove.Base.Priority)
            return false;
        else
        {
            if (playerPokemon.Speed > enemyPokemon.Speed)
                return true;
            else if (playerPokemon.Speed < enemyPokemon.Speed)
                return false;
            else
                return UnityEngine.Random.Range(1, 3) == 1 ? true : false;
        }
    }
    IEnumerator RunTurns(BattleAction playerAction)
    {
        _state = BattleState.RunningTurn;

        if(playerAction == BattleAction.Move)
        {
            //We grab a handle of each move chosen by both the player and the enemy
            if (!_playerUnit.Pokemon.TwoTurnMove || !_playerUnit.Pokemon.MustRecharge)
                _playerUnit.Pokemon.CurrentMove = _playerUnit.Pokemon.Moves[_currentMove];
            if (!_enemyUnit.Pokemon.TwoTurnMove || !_enemyUnit.Pokemon.MustRecharge)
                _enemyUnit.Pokemon.CurrentMove = _enemyUnit.Pokemon.GetRandomMove();

            //We check who goes first based on speed. In case of a speed-tie, we randomize who goes first. Then a handle of each unit is also set
            bool playerFirst = CheckFirstTurn(_playerUnit.Pokemon, _playerUnit.Pokemon.CurrentMove, _enemyUnit.Pokemon, _enemyUnit.Pokemon.CurrentMove);
            BattleUnit firstUnit = (playerFirst) ? _playerUnit : _enemyUnit;
            BattleUnit secondUnit = (!playerFirst) ? _playerUnit : _enemyUnit;
            Pokemon secondPokemon = secondUnit.Pokemon;

            //We run the move for the First Unit
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            if (_state == BattleState.BattleOver)
                yield break;

            yield return _waitUntilRunningTurn;

            //If the second unit did not die to the move, we proceed with its own selection. In case of a switch in pokemon, this will be skiped 
            if (secondPokemon.HP > 0)
            {
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                if (_state == BattleState.BattleOver)
                    yield break;
            }

            //After both moves are executed, we run the AfterTurn in order of speed
            yield return RunAfterTurn(firstUnit);
            yield return RunAfterTurn(secondUnit);
            if (_currentWeather != WeatherDB.Weathers[WeatherID.None])
                yield return RunWeatherAfterTurn(firstUnit, secondUnit);

            //If this did not result in the end of battle, we reset the turn, allowing the player to chose a new action
            if (_state != BattleState.BattleOver)
                if (_playerUnit.Pokemon.TwoTurnMove || _playerUnit.Pokemon.MustRecharge)
                {
                    yield return _attackDelay;
                    StartCoroutine(RunTurns(BattleAction.Move));
                }
                else
                    ActionSelection();
        }
        else
        {
            if(playerAction == BattleAction.SwitchPokemon)
            {
                //We grab a handle of which pokemon was selected, change the state to prevent the player from acting any further, and apply the switch
                Pokemon selectedMember = _playerParty.Pokemons[_currentMember];
                _state = BattleState.Busy;
                yield return SwitchPokemon(selectedMember);

                //We execute the enemy move after the player has changed to its new pokemon
                Move enemyMove = _enemyUnit.Pokemon.GetRandomMove();
                yield return RunMove(_enemyUnit, _playerUnit, enemyMove);

                //Check who goes first in the AfterTurn
                bool enemyFirst = (_enemyUnit.Pokemon.Speed > _playerUnit.Pokemon.Speed) ? true :
                    (_playerUnit.Pokemon.Speed == _enemyUnit.Pokemon.Speed) ? (UnityEngine.Random.Range(1, 3) == 1) ? true : false :
                    false;

                //Apply after turn for both pokemon
                if (enemyFirst)
                {
                    yield return RunAfterTurn(_enemyUnit);
                    yield return RunAfterTurn(_playerUnit);
                    if (_currentWeather != WeatherDB.Weathers[WeatherID.None])
                        yield return RunWeatherAfterTurn(_enemyUnit, _playerUnit);
                }
                else
                {
                    yield return RunAfterTurn(_playerUnit);
                    yield return RunAfterTurn(_enemyUnit);
                    if (_currentWeather != WeatherDB.Weathers[WeatherID.None])
                        yield return RunWeatherAfterTurn(_playerUnit, _enemyUnit);
                }

                //If this did not result in the end of battle, we reset the turn, allowing the player to chose a new action
                if (_state != BattleState.BattleOver)
                    ActionSelection();
            }
        }
    }

    IEnumerator CheckForFaint(BattleUnit unit)
    {
        //check if the unit has fainted, if it has, apply its animation and dialogue and check if the battle can continue
        if (unit.Pokemon.HP <= 0)
        {
            unit.PlayFaintAnimation();
            yield return _faintDelay;
            yield return _dialogueBox.TypeDialogue($"{ unit.Pokemon.Base.Name } has fainted.");
            yield return _pressAnyKeyToContinue;
            CheckForBattleOver(unit);
        }
    }
    bool AccuracyCheck(Move move, Pokemon source, Pokemon target)
    {
        //check if the target is currently using Dig/Fly
        if (target.TwoTurnMove)
            return false;
        //check if the move is being used on self, or if the move bypasses accuracy to always hit
        if ((move.Base.Category == MoveCategory.Status && move.Base.Target == MoveTarget.Self) || move.Base.BypassAccuracy)
            return true;
        //Generates a random float number between 1 and 100, multiplying by the accuracy of the move and accuracy of the pokemon, applying the stat changes.
        //Clamps the accuracy to at least 33% of the move accuracy, and maximum of 3x its accuracy
        return (UnityEngine.Random.Range(1.00f, 100.00f) <= Math.Clamp(move.Base.Accuracy * source.Accuracy * target.Evasion * source.OnAccuracyCheck() * target.OnAccuracyCheck(), move.Base.Accuracy * 0.33f, move.Base.Accuracy * 3f)) ? true : false;
    }
    bool TargetIsImmune(Pokemon pokemon, Move move)
    {
        return (TypeChart.GetEffectiveness(move.Base.Type, pokemon.Base.PrimaryType) * TypeChart.GetEffectiveness(move.Base.Type, pokemon.Base.SecondaryType) == 0f) ? true : false;
    }
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move) 
    {
        _state = BattleState.RunningTurn;
        //check if the pokemon can act that turn before the move is called. In case it can't, it shows its dialogue updates and if it died due to confusion
        yield return OnBeforeMove(sourceUnit);

        //check if the target has its MustRecharge set to true, therefore recharging and cancelling his turn after the message is displayed
        if (sourceUnit.Pokemon.MustRecharge)
        {
            sourceUnit.Pokemon.MustRecharge = false;
            yield return _dialogueBox.TypeDialogue($"{ sourceUnit.Pokemon.Base.Name } { move.Base.OnCastMessage }");
            yield break;
        }

        //check if the target is currently using Dig/Fly, so we don't consume an additional PP and display the proper message
        if(!sourceUnit.Pokemon.TwoTurnMove)
            move.PP--;

        if (sourceUnit.Pokemon.TwoTurnMove || !move.Base.TwoTurnMove)
        {
            if (move.Base.Name == "Struggle")
                yield return _dialogueBox.TypeDialogue($"{ sourceUnit.Pokemon.Base.Name } { move.Base.OnCastMessage} ");
            yield return _dialogueBox.TypeDialogue($"{ sourceUnit.Pokemon.Base.Name } used { move.Base.Name }.");
        }            
        else
            yield return _dialogueBox.TypeDialogue($"{ sourceUnit.Pokemon.Base.Name } { move.Base.OnCastMessage }");

        //check if the pokemon has hit its target
        if (AccuracyCheck(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            //if the move is Dig/Fly we play its animation and cancel the turn. If second turn, we check if the target can be damaged, and if it can, deals normal damage
            if (move.Base.TwoTurnMove)
                yield return TwoTurnMoves(sourceUnit, targetUnit, move);            

            //if the target is immune to the move type, returns the dialogue and ends the move
            if (TargetIsImmune(targetUnit.Pokemon, move))
            {
                yield return _dialogueBox.TypeDialogue($"It doesn't affect enemy { targetUnit.Pokemon.Base.Name }.");
                yield break;
            }

            sourceUnit.PlayAttackAnimation();
            yield return _attackDelay;

            //check which type of move is being used
            if(move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, move.Base.Target, sourceUnit.Pokemon, targetUnit.Pokemon, false);
            }
            else
            {
                //apply damage values, animations, dialogues and check if contact was made and apply recoil if applicable
                yield return RunDamage(sourceUnit, targetUnit, move);               

                //Set the MustRecharge to true in case of a rechargable move
                if (move.Base.MustRecharge)
                    sourceUnit.Pokemon.MustRecharge = true;
            }

            //checks if the move used has secondary effects and the target survived the damage dealt. Apply all secondary effects from the move
            if(move.Base.SecondaryEffects != null && move.Base.SecondaryEffects.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach(SecondaryEffects secondary in move.Base.SecondaryEffects)
                {
                    if(UnityEngine.Random.Range(1,101) <= secondary.ProcChance)
                    {
                        yield return RunMoveEffects(secondary, secondary.Target, sourceUnit.Pokemon, targetUnit.Pokemon, true);
                    }
                }
            }
        }
        else
        {
            //in case of miss, display the message and return
            yield return _dialogueBox.TypeDialogue($"{ sourceUnit.Pokemon.Base.Name }'s attack missed!");
            yield return _attackDelay;
        }
    }
    IEnumerator RunMoveEffects(MoveEffects effects, MoveTarget moveTarget, Pokemon source, Pokemon target, bool secondaryEffect)
    {
        //Apply the Stat Boosting as long its not null. Check if the target can receive the specific boost based on his abilities
        if (effects.Boosts != null)
        {
            yield return ApplyBoostEffects(effects, moveTarget, source, target);
        }

        //Apply the Status Condition as long as its not NONE, check if the target can receive it before applying it and return its dialogue.
        //If the target already has a Status Condition, it returns the dialogue "But it failed!"
        if (effects.Status != ConditionID.None)
        {
            yield return ApplyStatusEffects(effects, target, secondaryEffect);
        }

        //Apply the Volatile Status Condition as long as its not NONE, check if the target can receive it before applying it and return its dialogue.
        //If the target already has a Volatile Status Condition, it returns the dialogue "But it failed!"
        if (effects.VolatileStatus != ConditionID.None)
        {
            if (target.VolatileStatus == null)
                target.SetVolatileStatus(effects.VolatileStatus);
            else if (!secondaryEffect)
                yield return _dialogueBox.TypeDialogue("But it failed!");
        }

        if (effects.WeatherEffect != WeatherID.None)
        {
            yield return WeatherMove(effects);
        }

        //Display message for both Pokemon according to their Status
        yield return DisplayMessageBothPokemon(source, target);
    }
    IEnumerator OnBeforeMove(BattleUnit sourceUnit)
    {
        if (!sourceUnit.Pokemon.OnBeforeMove())
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateLoseHP();
            yield return CheckForFaint(sourceUnit);
            yield break;
        }
    }
    IEnumerator TwoTurnMoves(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        if (!sourceUnit.Pokemon.TwoTurnMove)
        {
            sourceUnit.Pokemon.TwoTurnMove = true;
            sourceUnit.PlayTwoTurnAnimation(true);
            yield return _attackDelay;
            yield break;
        }
        else
        {
            //if the target is immune to the move type, returns the dialogue and ends the move
            if (TargetIsImmune(targetUnit.Pokemon, move))
            {
                yield return _dialogueBox.TypeDialogue($"It doesn't affect enemy { targetUnit.Pokemon.Base.Name }.");
                yield break;
            }
            else
            {
                sourceUnit.PlayTwoTurnAnimation(false);
                yield return _attackDelay;

                //apply damage values, animations, dialogues and check if contact was made, applying other effects as necessary
                yield return RunDamage(sourceUnit, targetUnit, move);
                sourceUnit.Pokemon.TwoTurnMove = false;
                yield break;
            }
        }
    }
    IEnumerator RunDamage(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        DamageDetails damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon, _currentWeather.Id);
        targetUnit.PlayHitAnimation();
        yield return targetUnit.Hud.UpdateLoseHP();
        yield return ShowDamageDetails(damageDetails, targetUnit.Pokemon);

        if (move.Base.HPDrainingMove)
            yield return HPDrainingMove(sourceUnit, damageDetails.HealthRestored);        

        if (move.Base.MakesContact)
            yield return ApplyContact(sourceUnit.Pokemon, targetUnit.Pokemon);

        if (move.Base.Recoil > 0)
            yield return ApplyRecoil(sourceUnit);

        yield return CheckForFaint(targetUnit);
        yield return CheckForFaint(sourceUnit);

    }
    IEnumerator HPDrainingMove(BattleUnit sourceUnit, int healthRestored)
    {
        //update the HP and display the "energy drained" message on the ShowStatusChanges
        yield return _faintDelay;
        yield return sourceUnit.Hud.UpdateRegainHP(healthRestored);
        yield return ShowStatusChanges(sourceUnit.Pokemon);
    }
    IEnumerator ApplyRecoil(BattleUnit sourceUnit)
    {
        //Update the HP and display the "was damaged by recoil" message on the ShowStatusChanges
        yield return sourceUnit.Hud.UpdateLoseHP();
        yield return ShowStatusChanges(sourceUnit.Pokemon);        
    }
    IEnumerator ApplyContact(Pokemon source, Pokemon target)
    {
        //check if any pokemon has contact effects and apply such
        target.OnContactCheck(source);
        if (target.StatusChanges.Count > 0)
            yield return ShowStatusChanges(target);
        if (source.StatusChanges.Count > 0)
            yield return ShowStatusChanges(source);
    }
    IEnumerator DisplayMessageBothPokemon(Pokemon source, Pokemon target)
    {
        //check if any pokemon has any status changes messages and display them
        if (source.StatusChanges.Count > 0)
            yield return ShowStatusChanges(source);
        if (target.StatusChanges.Count > 0)
            yield return ShowStatusChanges(target);
    }
    IEnumerator ApplyBoostEffects(MoveEffects effects, MoveTarget moveTarget, Pokemon source, Pokemon target)
    {
        if (moveTarget == MoveTarget.Foe)
        {
            foreach (StatBoost statBoost in effects.Boosts)
            {
                if (target.CanReceiveBoost(statBoost.stat, statBoost.boost, target))
                {
                    target.ApplyBoost(statBoost);
                    yield return ShowStatusChanges(target);
                }
                    
            }
        }
        else
        {
            foreach (StatBoost statBoost in effects.Boosts)
            {
                source.ApplyBoost(statBoost);
                yield return ShowStatusChanges(source);
            }
        }
    }
    IEnumerator ApplyStatusEffects(MoveEffects effects, Pokemon target, bool secondaryEffect)
    {
        if (target.Status == null)
        {
            if (target.CanReceiveStatus(effects.Status))
                target.SetStatus(effects.Status);
            else
                if (!secondaryEffect)
                yield return _dialogueBox.TypeDialogue($"It doesn't affect enemy { target.Base.Name }.");
        }
        else
                if (!secondaryEffect)
            yield return _dialogueBox.TypeDialogue("But it failed!");
    }
    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        //after the turn ends, we apply damage effect from Status Conditions and Weather Effects
        if (_state == BattleState.BattleOver)
            yield break;
        //yield return new WaitUntil(() => _state == BattleState.RunningTurn);

        sourceUnit.Pokemon.OnAfterTurn();        
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateLoseHP();
        yield return CheckForFaint(sourceUnit);
    }
    void UpdateWeatherImage(WeatherID weatherID)
    {
        _weatherImage.sprite = _weathers[(int)weatherID];
    }
    IEnumerator WeatherMove(MoveEffects effects)
    {
        if (_currentWeather.Id != effects.WeatherEffect)
        {
            _currentWeather = WeatherDB.Weathers[effects.WeatherEffect];
            _currentWeather.Duration = 5;
            _currentWeather.EnvironmentWeather = false;
            yield return _dialogueBox.TypeDialogue(_currentWeather.CastMessage);
            UpdateWeatherImage(_currentWeather.Id);
        }
        else
            yield return _dialogueBox.TypeDialogue("But it failed!");
    }
    IEnumerator RunWeatherAfterTurn(BattleUnit firstUnit, BattleUnit secondUnit)
    {
        //make sure that Environmental Weather lasts until replaced
        if (!_currentWeather.EnvironmentWeather)
            _currentWeather.Duration--;

        if (_currentWeather.Duration == 0)
        {
            yield return _dialogueBox.TypeDialogue(_currentWeather.EndMessage);
            _currentWeather = WeatherDB.Weathers[WeatherID.None];
            UpdateWeatherImage(WeatherID.None);
        }
        else
        {
            if (_currentWeather?.OnAfterTurn != null)
            {
                yield return RunWeatherDamage(firstUnit);
                yield return RunWeatherDamage(secondUnit);
            }                
        }
    }
    IEnumerator RunWeatherDamage(BattleUnit unit)
    {        
        if (_currentWeather.OnAfterTurn.Invoke(unit.Pokemon))
        {                
            yield return ShowStatusChanges(unit.Pokemon); 
            yield return _weatherDelay;
            unit.PlayHitAnimation();
            yield return _weatherDelay;
            yield return unit.Hud.UpdateLoseHP();
            yield return CheckForFaint(unit);       
        }
    }
    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        //Dequeues the list of messages of status changes for the pokemon
        while(pokemon.StatusChanges.Count > 0)
        {
            string message = pokemon.StatusChanges.Dequeue();
            yield return _dialogueBox.TypeDialogue(message);
        }
    }
    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        //if the player still has any pokemon alive after a faint, opens the Party Screen for a pokemon change. Otherwise just ends the battle
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
        //Display damage details according to Crit/Effectiveness
        if (damageDetails.Critical > 1f)
            yield return _dialogueBox.TypeDialogue("A critical hit!");        
        if (damageDetails.TypeEffectiveness == 1f)
            yield return null;
        else
        {
            if (damageDetails.TypeEffectiveness > 1f)
                yield return _dialogueBox.TypeDialogue("Its super effective!");
            else
                yield return _dialogueBox.TypeDialogue("Its not very effective.");
        }
    }
}
