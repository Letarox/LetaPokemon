using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleOverState : BattleStateBase
{
    public BattleOverState(BattleSystem battleSystem) : base(battleSystem)
    {
    }

    public override void EnterState()
    {
        CheckForBattleOver(battleSystem.FaintedUnit);
    }

    public override void UpdateState()
    {

    }

    public override void ExitState()
    {

    }
    void BattleOver(bool won)
    {
        //Update the state for battle over and apply the battle over on each pokemon
        battleSystem.PlayerParty.Pokemons.ForEach(p => p.OnBattleOver());
        battleSystem.RaiseBattleOverEvent(won);
    }
    public void CheckForBattleOver(BattleUnit faintedUnit)
    {
        //if the player still has any pokemon alive after a faint, opens the Party Screen for a pokemon change. Otherwise just ends the battle
        if (faintedUnit.IsPlayerTeam)
        {
            Pokemon nextPokemon = battleSystem.PlayerParty.GetHealthyPokemon();
            if (nextPokemon != null)
                battleSystem.TransitionToState(BattleState.PartyScreen);
            else
                BattleOver(false);
        }
        else
            BattleOver(true);
    }
}