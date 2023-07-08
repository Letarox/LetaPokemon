using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterTurnState : BattleStateBase
{
    private RunningTurnState runningTurnState;
    public AfterTurnState(BattleSystem battleSystem) : base(battleSystem)
    {
    }
    public void SetRunningTurnState(RunningTurnState runningTurnState)
    {
        this.runningTurnState = runningTurnState;
    }
    public override void EnterState()
    {
        battleSystem.StartCoroutine(RunAfterTurn());
    }

    public override void UpdateState()
    {

    }

    public override void ExitState()
    {
        
    }
    IEnumerator RunAfterTurn()
    {
        // After both moves are executed, we run the AfterTurn in order of speed
        battleSystem.ActivePokemon.Clear();
        battleSystem.ActivePokemon.Add(battleSystem.ActivePlayerUnit);
        battleSystem.ActivePokemon.Add(battleSystem.ActiveEnemyUnit);
        battleSystem.ActivePokemon = SortedPokemonByEndOfTurnOrder(battleSystem.ActivePokemon);
        yield return RunEffectsAfterTurn(battleSystem.ActivePokemon);
        foreach (BattleUnit unit in battleSystem.ActivePokemon)
        {
            yield return RunStatusEffectsAfterTurn(unit);
        }

        // If this did not result in the end of battle, we reset the turn, allowing the player to choose a new action
        if (battleSystem.State != BattleState.BattleOver)
        {
            if (battleSystem.ActivePlayerUnit.Pokemon.TwoTurnMove || battleSystem.ActivePlayerUnit.Pokemon.MustRecharge)
            {
                yield return battleSystem.AttackDelay;
                battleSystem.TransitionToState(BattleState.RunningTurn, () => battleSystem.StartCoroutine(runningTurnState.SwitchPokemonTurn()));
            }
            else
            {
                battleSystem.TransitionToState(BattleState.ActionSelection);
            }
        }
    }
    public IEnumerator RunStatusEffectsAfterTurn(BattleUnit sourceUnit)
    {
        //after the turn ends, we apply damage effect from Status Conditions and Weather Effects
        if (battleSystem.State == BattleState.BattleOver)
            yield break;

        sourceUnit.Pokemon.OnAfterTurn();
        yield return battleSystem.ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateLoseHP();
        yield return battleSystem.CheckForFaint(sourceUnit);
    }
    public IEnumerator RunEffectsAfterTurn(List<BattleUnit> units)
    {
        yield return battleSystem.WeatherManager.WeatherAfterTurn(units);

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
    
    List<BattleUnit> SortedPokemonByEndOfTurnOrder(List<BattleUnit> unitList)
    {
        // Sort the Pokémon based on priority and speed
        unitList.Sort((a, b) =>
        {
            // Compare their speeds returning the comparison
            int speedComparison = b.Pokemon.Speed.CompareTo(a.Pokemon.Speed);
            if (speedComparison != 0)
                return speedComparison;

            // If there is a speed tie, randomly decide the order
            return UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
        });

        return unitList;
    }
}
