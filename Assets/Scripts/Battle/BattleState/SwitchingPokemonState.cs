using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchingPokemonState : BattleStateBase
{
    private RunningTurnState runningTurnState;
    public SwitchingPokemonState(BattleSystem battleSystem) : base(battleSystem)
    {
    }
    public void SetRunningTurnState(RunningTurnState runningTurnState)
    {
        this.runningTurnState = runningTurnState;
    }

    public override void EnterState()
    {
        battleSystem.StartCoroutine(SwitchPokemon(battleSystem.PlayerParty.Pokemons[battleSystem.CurrentMember]));
    }

    public override void UpdateState()
    {

    }

    public override void ExitState()
    {
        
    }
    public IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        battleSystem.UIBattleManager.ActivePlayerUnit.ReAppearUnit();
        battleSystem.UIBattleManager.ActiveEnemyUnit.ReAppearUnit();

        if (battleSystem.UIBattleManager.ActivePlayerUnit.Pokemon.HP > 0)
        {
            yield return battleSystem.UIBattleManager.DialogueBox.TypeDialogue($"Come back { battleSystem.UIBattleManager.ActivePlayerUnit.Pokemon.Base.Name }!");
            battleSystem.UIBattleManager.ActivePlayerUnit.PlayFaintAnimation();
            yield return battleSystem.UIBattleManager.FaintDelay;
        }

        yield return battleSystem.UIBattleManager.FaintDelay;

        battleSystem.UIBattleManager.ActivePlayerUnit.Setup(newPokemon);
        battleSystem.UIBattleManager.DialogueBox.SetMoveNames(newPokemon.Moves);
        yield return battleSystem.UIBattleManager.DialogueBox.TypeDialogue($"Go {newPokemon.Base.Name}!");
        yield return battleSystem.UIBattleManager.FaintDelay;

        yield return PokemonSwitchAbility(battleSystem.UIBattleManager.ActivePlayerUnit.Pokemon, battleSystem.UIBattleManager.ActiveEnemyUnit.Pokemon);

        if (battleSystem.PreState == BattleState.ActionSelection)
        {
            battleSystem.PreState = null;
            battleSystem.TransitionToState(BattleState.RunningTurn, () => battleSystem.StartCoroutine(runningTurnState.SwitchPokemonTurn()));
        }
        else
        {
            battleSystem.TransitionToState(BattleState.AfterTurn);
        }
    }
    public IEnumerator PokemonSwitchAbility(Pokemon source, Pokemon target)
    {
        if (AbilityManager.Instance.OnPokemonEnterBattle(source, target))
        {
            yield return battleSystem.UIBattleManager.DisplayAbilityBoxMessage(source.Base.Ability.Name, source, target);
        }
    }
}
