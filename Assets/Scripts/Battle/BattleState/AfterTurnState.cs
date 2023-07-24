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
        battleSystem.UIBattleManager.StartCoroutine(RunAfterTurn());
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
        battleSystem.ActivePokemon.Add(battleSystem.UIBattleManager.ActivePlayerUnit);
        battleSystem.ActivePokemon.Add(battleSystem.UIBattleManager.ActiveEnemyUnit);
        battleSystem.ActivePokemon = SortedPokemonBySpeed(battleSystem.ActivePokemon);
        yield return RunEffectsAfterTurn(battleSystem.ActivePokemon);
        foreach (var unit in battleSystem.ActivePokemon)
        {
            yield return RunStatusEffectsAfterTurn(unit);
        }

        // If this did not result in the end of battle, we reset the turn, allowing the player to choose a new action
        if (battleSystem.State != BattleState.BattleOver)
        {
            if (battleSystem.UIBattleManager.ActivePlayerUnit.Pokemon.TwoTurnMove || battleSystem.UIBattleManager.ActivePlayerUnit.Pokemon.MustRecharge)
            {
                yield return battleSystem.UIBattleManager.AttackDelay;
                battleSystem.TransitionToState(BattleState.RunningTurn, () => battleSystem.UIBattleManager.StartCoroutine(runningTurnState.SwitchPokemonTurn()));
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
        yield return battleSystem.UIBattleManager.ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateLoseHP();
        yield return battleSystem.CheckForFaint(sourceUnit);
        sourceUnit.Pokemon.Flinched = false;
    }
    public IEnumerator RunEffectsAfterTurn(List<BattleUnit> units)
    {
        //Run the after turn for both Weather and Screens
        yield return battleSystem.WeatherManager.WeatherAfterTurn(units);        
        yield return battleSystem.ScreensAfterTurn();
    }
    
    public List<BattleUnit> SortedPokemonBySpeed(List<BattleUnit> unitList)
    {
        // Sort the Pokémon based on priority and speed
        unitList.Sort((a, b) =>
        {
            // Compare their speeds returning the comparison
            int speedComparison = b.Pokemon.Speed.CompareTo(a.Pokemon.Speed);
            if (speedComparison != 0)
                return speedComparison;

            // If there is a speed tie, randomly decide the order
            return Random.Range(0, 2) == 0 ? -1 : 1;
        });

        return unitList;
    }
}
