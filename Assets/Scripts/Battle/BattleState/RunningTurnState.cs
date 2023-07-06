using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate IEnumerator RunningTurnAction();
public class RunningTurnState : BattleStateBase
{
    private PartyScreenState partyScreenState;
    private RunningTurnAction runningTurnAction;
    private List<Pokemon> turnOrder;

    public RunningTurnState(BattleSystem battleSystem) : base(battleSystem)
    {
        turnOrder = new List<Pokemon>();
    }
    public void SetPartyScreenState(PartyScreenState partyScreenState)
    {
        this.partyScreenState = partyScreenState;
    }
    public void SetRunningTurnAction(RunningTurnAction action)
    {
        runningTurnAction = action;
    }
    public override void EnterState()
    {

    }
    public override void UpdateState()
    {

    }
    public override void ExitState()
    {

    }
    List<Pokemon> SortedPokemonByTurnOrder(List<Pokemon> pokemonList)
    {
        // Sort the Pokémon based on priority and speed
        pokemonList.Sort((a, b) =>
        {
            bool hasPriorityA = a.CurrentMove.Base.Priority > 0;
            bool hasPriorityB = b.CurrentMove.Base.Priority > 0;

            if (hasPriorityA && !hasPriorityB)
            {
                return -1; //Pokémon A has priority, so it goes first
            }
            else if (!hasPriorityA && hasPriorityB)
            {
                return 1; //Pokémon B has priority, so it goes first
            }
            else if (hasPriorityA && hasPriorityB)
            {
                //Both Pokémon have priority moves, so randomize the order
                return UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
            }
            else
            {
                //Neither Pokémon has priority moves, compare their speeds
                int speedComparison = b.Speed.CompareTo(a.Speed);
                if (speedComparison != 0)
                    return speedComparison;

                //If there is a speed tie, randomly decide the order
                return UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
            }
        });

        return pokemonList;
    }
    public bool CheckFirstTurn(Pokemon playerPokemon, Move playerMove, Pokemon enemyPokemon, Move enemyMove)
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

    public IEnumerator RunTurn()
    {
        // We grab a handle of each move chosen by both the player and the enemy
        if (!battleSystem.ActivePlayerUnit.Pokemon.TwoTurnMove || !battleSystem.ActivePlayerUnit.Pokemon.MustRecharge)
            battleSystem.ActivePlayerUnit.Pokemon.CurrentMove = battleSystem.ActivePlayerUnit.Pokemon.Moves[battleSystem.CurrentMove];
        if (!battleSystem.ActiveEnemyUnit.Pokemon.TwoTurnMove || !battleSystem.ActiveEnemyUnit.Pokemon.MustRecharge)
            battleSystem.ActiveEnemyUnit.Pokemon.CurrentMove = battleSystem.ActiveEnemyUnit.Pokemon.GetRandomMove();

        // We check who goes first based on speed. In case of a speed-tie, we randomize who goes first. Then a handle of each unit is also set
        bool playerFirst = CheckFirstTurn(battleSystem.ActivePlayerUnit.Pokemon, battleSystem.ActivePlayerUnit.Pokemon.CurrentMove, battleSystem.ActiveEnemyUnit.Pokemon, battleSystem.ActiveEnemyUnit.Pokemon.CurrentMove);
        BattleUnit firstUnit = (playerFirst) ? battleSystem.ActivePlayerUnit : battleSystem.ActiveEnemyUnit;
        BattleUnit secondUnit = (!playerFirst) ? battleSystem.ActivePlayerUnit : battleSystem.ActiveEnemyUnit;
        Pokemon secondPokemon = secondUnit.Pokemon;

        // We run the move for the First Unit
        yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
        if (battleSystem.State == BattleState.BattleOver)
            yield break;

        // If the second unit did not die to the move, we proceed with its own selection. In case of a switch in pokemon, this will be skipped 
        if (secondPokemon.HP > 0)
        {
            yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
            if (battleSystem.State == BattleState.BattleOver)
                yield break;
        }

        // After both moves are executed, we run the AfterTurn in order of speed
        yield return RunAfterTurn(firstUnit);
        yield return RunAfterTurn(secondUnit);
        if (battleSystem.CurrentWeather != WeatherDB.Weathers[WeatherID.None])
            yield return RunEffectsAfterTurn(firstUnit, secondUnit);

        // If this did not result in the end of battle, we reset the turn, allowing the player to choose a new action
        if (battleSystem.State != BattleState.BattleOver)
        {
            if (battleSystem.ActivePlayerUnit.Pokemon.TwoTurnMove || battleSystem.ActivePlayerUnit.Pokemon.MustRecharge)
            {
                yield return battleSystem.AttackDelay;
                battleSystem.StartCoroutine(RunTurn());
            }
            else
            {
                battleSystem.TransitionToState(BattleState.ActionSelection);
            }
        }
    }
    public IEnumerator SwitchPokemonTurn()
    {
        // We execute the enemy move after the player has changed to its new pokemon
        Move enemyMove = battleSystem.ActiveEnemyUnit.Pokemon.GetRandomMove();
        yield return RunMove(battleSystem.ActiveEnemyUnit, battleSystem.ActivePlayerUnit, enemyMove);

        // Check who goes first in the AfterTurn
        bool enemyFirst = (battleSystem.ActiveEnemyUnit.Pokemon.Speed > battleSystem.ActivePlayerUnit.Pokemon.Speed) ? true :
            (battleSystem.ActivePlayerUnit.Pokemon.Speed == battleSystem.ActiveEnemyUnit.Pokemon.Speed) ? (UnityEngine.Random.Range(1, 3) == 1) ? true : false :
            false;

        // Apply after turn for both pokemon
        if (enemyFirst)
        {
            yield return RunAfterTurn(battleSystem.ActiveEnemyUnit);
            yield return RunAfterTurn(battleSystem.ActivePlayerUnit);
            if (battleSystem.CurrentWeather != WeatherDB.Weathers[WeatherID.None])
                yield return RunEffectsAfterTurn(battleSystem.ActiveEnemyUnit, battleSystem.ActivePlayerUnit);
        }
        else
        {
            yield return RunAfterTurn(battleSystem.ActivePlayerUnit);
            yield return RunAfterTurn(battleSystem.ActiveEnemyUnit);
            if (battleSystem.CurrentWeather != WeatherDB.Weathers[WeatherID.None])
                yield return RunEffectsAfterTurn(battleSystem.ActivePlayerUnit, battleSystem.ActiveEnemyUnit);
        }

        // If this did not result in the end of battle, we reset the turn, allowing the player to choose a new action
        if (battleSystem.State != BattleState.BattleOver)
        {
            battleSystem.TransitionToState(BattleState.ActionSelection);
        }
    }
    bool AccuracyCheck(Move move, Pokemon source, Pokemon target)
    {
        //check if the move is being used on self
        if (move.Base.Category == MoveCategory.Status && move.Base.Target == MoveTarget.Self)
            return true;
        //check if the target is currently using Dig/Fly
        if (target.TwoTurnMove)
            return false;
        //check if the move should always hit
        if (move.Base.BypassAccuracy)
            return true;
        //Generates a random float number between 1 and 100, multiplying by the accuracy of the move and accuracy of the pokemon, applying the stat changes.
        //Clamps the accuracy to at least 33% of the move accuracy, and maximum of 3x its accuracy
        return (UnityEngine.Random.Range(1.00f, 100.00f) <= Math.Clamp(move.Base.Accuracy * source.Accuracy * target.Evasion * source.OnAccuracyCheck() * target.OnAccuracyCheck(), move.Base.Accuracy * 0.33f, move.Base.Accuracy * 3f)) ? true : false;
    }
    bool TargetIsImmune(Pokemon pokemon, Move move)
    {
        //check if the target isn't a grass pokemon being afflicted by a spore move
        if (move.Base.Variation == MoveVariation.Spore && (pokemon.Base.PrimaryType == PokemonType.Grass || pokemon.Base.SecondaryType == PokemonType.Grass))
            return true;

        return (TypeChart.GetEffectiveness(move.Base.Type, pokemon.Base.PrimaryType) * TypeChart.GetEffectiveness(move.Base.Type, pokemon.Base.SecondaryType) == 0f) ? true : false;
    }
    public IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        //check if the pokemon can act that turn before the move is called. In case it can't, it shows its dialogue updates and if it died due to confusion
        if (!sourceUnit.Pokemon.OnBeforeMove())
        {
            yield return battleSystem.ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateLoseHP();
            yield return battleSystem.CheckForFaint(sourceUnit);
            yield break;
        }

        //check if the target has its MustRecharge set to true, therefore recharging and cancelling his turn after the message is displayed
        if (sourceUnit.Pokemon.MustRecharge)
        {
            sourceUnit.Pokemon.MustRecharge = false;
            yield return battleSystem.DialogueBox.TypeDialogue($"{ sourceUnit.Pokemon.Base.Name } { move.Base.OnCastMessage }");
            yield break;
        }

        //check if the target is currently using Dig/Fly, so we don't consume an additional PP and display the proper message
        if (!sourceUnit.Pokemon.TwoTurnMove)
            move.PP--;

        if (sourceUnit.Pokemon.TwoTurnMove || !move.Base.TwoTurnMove)
        {
            if (move.Base.Name == "Struggle")
                yield return battleSystem.DialogueBox.TypeDialogue($"{ sourceUnit.Pokemon.Base.Name } { move.Base.OnCastMessage} ");
            yield return battleSystem.DialogueBox.TypeDialogue($"{ sourceUnit.Pokemon.Base.Name } used { move.Base.Name }.");
        }
        else
            yield return battleSystem.DialogueBox.TypeDialogue($"{ sourceUnit.Pokemon.Base.Name } { move.Base.OnCastMessage }");

        //check if the pokemon has hit its target
        if (AccuracyCheck(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            //if the move is Dig/Fly we play its animation and cancel the turn. If second turn, we check if the target can be damaged, and if it can, deals normal damage
            if (move.Base.TwoTurnMove && !sourceUnit.Pokemon.TwoTurnMove)
            {
                sourceUnit.Pokemon.TwoTurnMove = true;
                sourceUnit.PlayTwoTurnAnimation(true);
                yield return battleSystem.AttackDelay;
                yield break;
            }

            //if the target is immune to the move type, returns the dialogue and ends the move
            if (TargetIsImmune(targetUnit.Pokemon, move))
            {
                yield return battleSystem.DialogueBox.TypeDialogue($"It doesn't affect enemy { targetUnit.Pokemon.Base.Name }.");
                yield break;
            }

            //apply attack animation and delay
            if (move.Base.TwoTurnMove)
                sourceUnit.PlayTwoTurnAnimation(false);
            else
                sourceUnit.PlayAttackAnimation();
            yield return battleSystem.AttackDelay;

            //check which type of move is being used
            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, move.Base.Target, sourceUnit, targetUnit.Pokemon, false);
            }
            else
            {
                //apply damage values, animations, dialogues and check if contact was made and apply recoil if applicable
                yield return RunDamage(sourceUnit, targetUnit, move);

                //Set the MustRecharge to true in case of a rechargable move
                if (move.Base.MustRecharge)
                    sourceUnit.Pokemon.MustRecharge = true;

                //if its Dig/Fly sort of move, remove them from their invulnerability
                if (move.Base.TwoTurnMove)
                    sourceUnit.Pokemon.TwoTurnMove = false;
            }

            //checks if the move used has secondary effects and the target survived the damage dealt. Apply all secondary effects from the move
            if (move.Base.SecondaryEffects != null && move.Base.SecondaryEffects.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach (SecondaryEffects secondary in move.Base.SecondaryEffects)
                {
                    if (UnityEngine.Random.Range(1, 101) <= secondary.ProcChance)
                    {
                        yield return RunMoveEffects(secondary, secondary.Target, sourceUnit, targetUnit.Pokemon, true);
                    }
                }
            }
        }
        else
        {
            //in case of miss, display the message and return
            yield return battleSystem.DialogueBox.TypeDialogue($"{ sourceUnit.Pokemon.Base.Name }'s attack missed!");
            yield return battleSystem.AttackDelay;
        }
    }
    IEnumerator RunMoveEffects(MoveEffects effects, MoveTarget moveTarget, BattleUnit sourceUnit, Pokemon target, bool secondaryEffect)
    {
        //Apply the Stat Boosting as long its not null. Check if the target can receive the specific boost based on his abilities
        if (effects.Boosts != null)
        {
            yield return ApplyBoostEffects(effects, moveTarget, sourceUnit.Pokemon, target);
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
                yield return battleSystem.DialogueBox.TypeDialogue("But it failed!");
        }

        //Run the weather effects and apply any effects as applicabe
        if (effects.WeatherEffect != WeatherID.None)
        {
            yield return WeatherMove(effects);
        }

        //Run the screen's effect
        if (effects.ScreenType != ScreenType.None)
        {
            yield return ScreenMove(effects, sourceUnit);
        }

        //Display message for both Pokemon according to their Status
        yield return DisplayMessageBothPokemon(sourceUnit.Pokemon, target);
    }

    IEnumerator RunDamage(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        List<Screen> screen = targetUnit.IsPlayerTeam ? battleSystem.PlayerTeam.TeamScreens : battleSystem.EnemyTeam.TeamScreens;
        DamageDetails damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon, battleSystem.CurrentWeather.Id, screen);
        targetUnit.PlayHitAnimation();
        yield return targetUnit.Hud.UpdateLoseHP();
        yield return ShowDamageDetails(damageDetails, targetUnit.Pokemon);

        if (move.Base.HPDrainingMove)
            yield return HPDrainingMove(sourceUnit, damageDetails.HealthRestored);

        if (move.Base.MakesContact)
            yield return ApplyContact(sourceUnit.Pokemon, targetUnit.Pokemon);

        if (move.Base.Recoil > 0)
            yield return ApplyRecoil(sourceUnit);

        yield return battleSystem.CheckForFaint(targetUnit);
        yield return battleSystem.CheckForFaint(sourceUnit);

    }
    IEnumerator HPDrainingMove(BattleUnit sourceUnit, int healthRestored)
    {
        //update the HP and display the "energy drained" message on the ShowStatusChanges
        yield return battleSystem.FaintDelay;
        yield return sourceUnit.Hud.UpdateRegainHP(healthRestored);
        yield return battleSystem.ShowStatusChanges(sourceUnit.Pokemon);
    }
    IEnumerator ApplyRecoil(BattleUnit sourceUnit)
    {
        //Update the HP and display the "was damaged by recoil" message on the ShowStatusChanges
        yield return sourceUnit.Hud.UpdateLoseHP();
        yield return battleSystem.ShowStatusChanges(sourceUnit.Pokemon);
    }
    IEnumerator ApplyContact(Pokemon source, Pokemon target)
    {
        //check if any pokemon has contact effects and apply such
        target.OnContactCheck(source);
        if (target.StatusChanges.Count > 0)
            yield return battleSystem.ShowStatusChanges(target);
        if (source.StatusChanges.Count > 0)
            yield return battleSystem.ShowStatusChanges(source);
    }
    IEnumerator DisplayMessageBothPokemon(Pokemon source, Pokemon target)
    {
        //check if any pokemon has any status changes messages and display them
        if (source.StatusChanges.Count > 0)
            yield return battleSystem.ShowStatusChanges(source);
        if (target.StatusChanges.Count > 0)
            yield return battleSystem.ShowStatusChanges(target);
    }
    IEnumerator ApplyBoostEffects(MoveEffects effects, MoveTarget moveTarget, Pokemon source, Pokemon target)
    {
        if (moveTarget == MoveTarget.Foe)
        {
            foreach (StatBoost statBoost in effects.Boosts)
            {
                if (target.CanReceiveBoost(statBoost, target))
                {
                    target.ApplyBoost(statBoost);
                    yield return battleSystem.ShowStatusChanges(target);
                }
            }
        }
        else
        {
            foreach (StatBoost statBoost in effects.Boosts)
            {
                source.ApplyBoost(statBoost);
                yield return battleSystem.ShowStatusChanges(source);
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
                yield return battleSystem.DialogueBox.TypeDialogue($"It doesn't affect enemy { target.Base.Name }.");
        }
        else
                if (!secondaryEffect)
            yield return battleSystem.DialogueBox.TypeDialogue("But it failed!");
    }
    public IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        //after the turn ends, we apply damage effect from Status Conditions and Weather Effects
        if (battleSystem.State == BattleState.BattleOver)
            yield break;
        //yield return new WaitUntil(() => _state == BattleState.RunningTurn);

        sourceUnit.Pokemon.OnAfterTurn();
        yield return battleSystem.ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateLoseHP();
        yield return battleSystem.CheckForFaint(sourceUnit);
    }
    IEnumerator ScreenMove(MoveEffects effects, BattleUnit sourceUnit)
    {
        if (effects.ScreenType == ScreenType.AuroraVeil && battleSystem.CurrentWeather.Id != WeatherID.Hail)
        {
            yield return battleSystem.DialogueBox.TypeDialogue("But it failed!");
            yield break;
        }

        if (sourceUnit.IsPlayerTeam)
        {
            if (battleSystem.PlayerTeam.TeamScreens.Exists(obj => obj.Id == effects.ScreenType))
            {
                yield return battleSystem.DialogueBox.TypeDialogue("But it failed!");
                yield break;
            }

            battleSystem.PlayerTeam.TeamScreens.Add(ScreenDB.Screens[effects.ScreenType]);
            battleSystem.PlayerTeam.TeamScreens[battleSystem.PlayerTeam.TeamScreens.Count - 1].Duration = 5;
            yield return battleSystem.DialogueBox.TypeDialogue(battleSystem.PlayerTeam.TeamScreens[battleSystem.PlayerTeam.TeamScreens.Count - 1].PlayerStartMessage);
        }
        else
        {
            if (battleSystem.EnemyTeam.TeamScreens.Exists(obj => obj.Id == effects.ScreenType))
            {
                yield return battleSystem.DialogueBox.TypeDialogue("But it failed!");
                yield break;
            }

            battleSystem.EnemyTeam.TeamScreens.Add(ScreenDB.Screens[effects.ScreenType]);
            battleSystem.EnemyTeam.TeamScreens[battleSystem.EnemyTeam.TeamScreens.Count - 1].Duration = 5;
            yield return battleSystem.DialogueBox.TypeDialogue(battleSystem.EnemyTeam.TeamScreens[battleSystem.EnemyTeam.TeamScreens.Count - 1].EnemyStartMessage);
        }
    }
    IEnumerator WeatherMove(MoveEffects effects)
    {
        if (battleSystem.CurrentWeather.Id != effects.WeatherEffect)
        {
            battleSystem.CurrentWeather = WeatherDB.Weathers[effects.WeatherEffect];
            battleSystem.CurrentWeather.Duration = 5;
            battleSystem.CurrentWeather.EnvironmentWeather = false;
            yield return battleSystem.DialogueBox.TypeDialogue(battleSystem.CurrentWeather.CastMessage);
            battleSystem.UpdateWeatherImage(battleSystem.CurrentWeather.Id);
        }
        else
            yield return battleSystem.DialogueBox.TypeDialogue("But it failed!");
    }
    public IEnumerator RunEffectsAfterTurn(BattleUnit firstUnit, BattleUnit secondUnit)
    {
        //make sure that Environmental Weather lasts until replaced
        if (!battleSystem.CurrentWeather.EnvironmentWeather)
            battleSystem.CurrentWeather.Duration--;

        //remove weather or run its damage
        if (battleSystem.CurrentWeather.Duration == 0)
        {
            yield return battleSystem.DialogueBox.TypeDialogue(battleSystem.CurrentWeather.EndMessage);
            battleSystem.CurrentWeather = WeatherDB.Weathers[WeatherID.None];
            battleSystem.UpdateWeatherImage(WeatherID.None);
        }
        else
        {
            if (battleSystem.CurrentWeather?.OnAfterTurn != null)
            {
                yield return RunWeatherDamage(firstUnit);
                yield return RunWeatherDamage(secondUnit);
            }
        }

        //check if the player has any screen and run its dialogue
        if (battleSystem.PlayerTeam.TeamScreens.Count > 0)
        {
            foreach (Screen screen in battleSystem.PlayerTeam.TeamScreens)
            {
                screen.Duration--;
                if (screen.Duration == 0)
                {
                    yield return battleSystem.DialogueBox.TypeDialogue(screen.PlayerEndMessage);
                    battleSystem.PlayerTeam.TeamScreens.Remove(screen);
                }
            }
        }

        //check if the enemy has any screen and run its dialogue
        if (battleSystem.EnemyTeam.TeamScreens.Count > 0)
        {
            foreach (Screen screen in battleSystem.EnemyTeam.TeamScreens)
            {
                screen.Duration--;
                if (screen.Duration == 0)
                {
                    yield return battleSystem.DialogueBox.TypeDialogue(screen.EnemyEndMessage);
                    battleSystem.EnemyTeam.TeamScreens.Remove(screen);
                }
            }
        }
    }
    IEnumerator RunWeatherDamage(BattleUnit unit)
    {
        if (battleSystem.CurrentWeather.OnAfterTurn.Invoke(unit.Pokemon))
        {
            yield return battleSystem.ShowStatusChanges(unit.Pokemon);
            yield return battleSystem.WeatherDelay;
            unit.PlayHitAnimation();
            yield return battleSystem.WeatherDelay;
            yield return unit.Hud.UpdateLoseHP();
            yield return battleSystem.CheckForFaint(unit);
        }
    }
    IEnumerator ShowDamageDetails(DamageDetails damageDetails, Pokemon attackedPokemon)
    {
        //Display damage details according to Crit/Effectiveness
        if (damageDetails.Critical > 1f)
            yield return battleSystem.DialogueBox.TypeDialogue("A critical hit!");
        if (damageDetails.TypeEffectiveness == 1f)
            yield return null;
        else
        {
            if (damageDetails.TypeEffectiveness > 1f)
                yield return battleSystem.DialogueBox.TypeDialogue("Its super effective!");
            else
                yield return battleSystem.DialogueBox.TypeDialogue("Its not very effective.");
        }
    }
}
