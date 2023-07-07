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
        battleSystem.ActivePlayerUnit.ReAppearUnit();
        battleSystem.ActiveEnemyUnit.ReAppearUnit();

        if (battleSystem.ActivePlayerUnit.Pokemon.HP > 0)
        {
            yield return battleSystem.DialogueBox.TypeDialogue($"Come back {battleSystem.ActivePlayerUnit.Pokemon.Base.Name}!");
            battleSystem.ActivePlayerUnit.PlayFaintAnimation();
            yield return battleSystem.FaintDelay;
        }

        yield return battleSystem.FaintDelay;
        if (battleSystem.TurnOrder.Contains(battleSystem.ActivePlayerUnit))
            battleSystem.TurnOrder.Remove(battleSystem.ActivePlayerUnit);
        
        battleSystem.TurnOrder.Remove(battleSystem.ActivePlayerUnit);
        battleSystem.ActivePlayerUnit.Setup(newPokemon);
        battleSystem.TurnOrder.Add(battleSystem.ActivePlayerUnit);
        battleSystem.DialogueBox.SetMoveNames(newPokemon.Moves);
        yield return battleSystem.DialogueBox.TypeDialogue($"Go {newPokemon.Base.Name}!");
        yield return battleSystem.FaintDelay;

        yield return PokemonSwitchAbility(battleSystem.ActivePlayerUnit.Pokemon, battleSystem.ActiveEnemyUnit.Pokemon);

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
        if (source.OnPokemonSwitch(target))
        {
            battleSystem.AbilityBox.PlayAbilityEnterAnimation(source.Base.Ability.Name);
            yield return battleSystem.FaintDelay;
            yield return battleSystem.ShowStatusChanges(target);
            yield return battleSystem.AttackDelay;
            battleSystem.AbilityBox.PlayAbilityExitAnimation();
        }
        yield break;
    }
}
