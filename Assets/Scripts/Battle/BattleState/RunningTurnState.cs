using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate IEnumerator RunningTurnAction();
public class RunningTurnState : BattleStateBase
{
    public RunningTurnState(BattleSystem battleSystem) : base(battleSystem)
    {
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
    List<BattleUnit> SortedPokemonByTurnOrder(List<BattleUnit> unitList)
    {
        // Sort the Pokémon based on priority and speed
        unitList.Sort((a, b) =>
        {
            bool hasPriorityA = a.Pokemon.CurrentMove != null && a.Pokemon.CurrentMove.Base.Priority > 0;
            bool hasPriorityB = b.Pokemon.CurrentMove != null && b.Pokemon.CurrentMove.Base.Priority > 0;

            if (hasPriorityA && !hasPriorityB)
            {
                return -1; // Pokémon A has priority, so it goes first
            }
            else if (!hasPriorityA && hasPriorityB)
            {
                return 1; // Pokémon B has priority, so it goes first
            }
            else if (hasPriorityA && hasPriorityB)
            {
                // Both Pokémon have priority moves, compare their priority levels
                int priorityComparison = b.Pokemon.CurrentMove.Base.Priority.CompareTo(a.Pokemon.CurrentMove.Base.Priority);
                if (priorityComparison != 0)
                    return priorityComparison;

                // If there is a tie in priority, sort based on speed
                int speedComparison = b.Pokemon.Speed.CompareTo(a.Pokemon.Speed);
                if (speedComparison != 0)
                    return speedComparison;

                // If there is a speed tie, randomly decide the order
                return UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
            }
            else
            {
                // Neither Pokémon has priority moves, compare their speeds
                if (a.Pokemon.CurrentMove == null && b.Pokemon.CurrentMove == null)
                {
                    // Both Pokémon don't have a current move, order them based on speed
                    return b.Pokemon.Speed.CompareTo(a.Pokemon.Speed);
                }
                else if (a.Pokemon.CurrentMove == null)
                {
                    return 1; // Pokémon A doesn't have a current move, so Pokémon B goes first
                }
                else if (b.Pokemon.CurrentMove == null)
                {
                    return -1; // Pokémon B doesn't have a current move, so Pokémon A goes first
                }
                else
                {
                    // Compare their speeds when both have a current move
                    int speedComparison = b.Pokemon.Speed.CompareTo(a.Pokemon.Speed);
                    if (speedComparison != 0)
                        return speedComparison;

                    // If there is a speed tie, randomly decide the order
                    return UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
                }
            }
        });

        return unitList;
    }

    public IEnumerator RunTurn()
    {
        // We grab a handle of each move chosen by both the player and the enemy
        if (!battleSystem.ActivePlayerUnit.Pokemon.TwoTurnMove || !battleSystem.ActivePlayerUnit.Pokemon.MustRecharge)
            battleSystem.ActivePlayerUnit.Pokemon.CurrentMove = battleSystem.ActivePlayerUnit.Pokemon.Moves[battleSystem.CurrentMove];
        if (!battleSystem.ActiveEnemyUnit.Pokemon.TwoTurnMove || !battleSystem.ActiveEnemyUnit.Pokemon.MustRecharge)
            battleSystem.ActiveEnemyUnit.Pokemon.CurrentMove = battleSystem.ActiveEnemyUnit.Pokemon.GetRandomMove();

        // We check who goes first based on speed. In case of a speed-tie, we randomize who goes first. Then a handle of each unit is also set
        battleSystem.TurnOrder.Clear();
        battleSystem.TurnOrder.Add(battleSystem.ActivePlayerUnit);
        battleSystem.TurnOrder.Add(battleSystem.ActiveEnemyUnit);
        battleSystem.TurnOrder = SortedPokemonByTurnOrder(battleSystem.TurnOrder);
        foreach(BattleUnit unit in battleSystem.TurnOrder)
        {
            if(unit.Pokemon.HP > 0)
            {
                if (unit.IsPlayerTeam)
                    yield return RunMove(battleSystem.ActivePlayerUnit, battleSystem.ActiveEnemyUnit, battleSystem.ActivePlayerUnit.Pokemon.CurrentMove);
                else
                    yield return RunMove(battleSystem.ActiveEnemyUnit, battleSystem.ActivePlayerUnit, battleSystem.ActiveEnemyUnit.Pokemon.CurrentMove);
                if (battleSystem.State == BattleState.BattleOver)
                    yield break;
            }
        }        
        battleSystem.TransitionToState(BattleState.AfterTurn);        
    }
    public IEnumerator SwitchPokemonTurn()
    {
        // We execute the enemy move after the player has changed to its new pokemon
        Move enemyMove = battleSystem.ActiveEnemyUnit.Pokemon.GetRandomMove();
        yield return RunMove(battleSystem.ActiveEnemyUnit, battleSystem.ActivePlayerUnit, enemyMove);

        battleSystem.TransitionToState(BattleState.AfterTurn);
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
        if (move == null)
            yield break;
        //check if the pokemon can act that turn before the move is called. In case it can't, it shows its dialogue updates and if it died due to confusion
        if (!sourceUnit.Pokemon.OnBeforeMove())
        {
            yield return battleSystem.ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateLoseHP();
            yield return battleSystem.CheckForFaint(sourceUnit);
            yield break;
        }
        //display message if the pokemon wakes up or is no longer confused
        yield return battleSystem.ShowStatusChanges(sourceUnit.Pokemon);
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
        if (battleSystem.BattleCalculator.AccuracyCheck(move, sourceUnit.Pokemon, targetUnit.Pokemon))
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
            yield return battleSystem.WeatherManager.WeatherMove(effects);
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
        List<Screen> screens = targetUnit.IsPlayerTeam ? battleSystem.PlayerTeam.TeamScreens : battleSystem.EnemyTeam.TeamScreens;
        DamageDetails damageDetails = battleSystem.BattleCalculator.ApplyDamage(move, sourceUnit.Pokemon, targetUnit.Pokemon, battleSystem.WeatherManager.CurrentWeather.Id, screens);
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
        AbilityManager.Instance.OnContactCheck(source, target);
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
                if (AbilityManager.Instance.CanReceiveBoost(statBoost, target))
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
    
    IEnumerator ScreenMove(MoveEffects effects, BattleUnit sourceUnit)
    {
        if (effects.ScreenType == ScreenType.AuroraVeil && battleSystem.WeatherManager.CurrentWeather.Id != WeatherID.Hail)
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
