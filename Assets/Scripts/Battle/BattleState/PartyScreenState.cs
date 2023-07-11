using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyScreenState : BattleStateBase
{
    private SwitchingPokemonState busyState;
    int currentMember;

    public PartyScreenState(BattleSystem battleSystem) : base(battleSystem)
    {
    }

    
    public void SetBusyState(SwitchingPokemonState busyState)
    {
        this.busyState = busyState;
    }

    public override void EnterState()
    {
        battleSystem.UIBattleManager.PartyScreen.SetPartyData(battleSystem.PlayerParty.Pokemons);
        battleSystem.UIBattleManager.PartyScreen.gameObject.SetActive(true);
        battleSystem.UIBattleManager.ActivePlayerUnit.HideUnit();
        battleSystem.UIBattleManager.ActiveEnemyUnit.HideUnit();
    }

    public override void UpdateState()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentMember++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentMember--;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMember += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMember -= 2;

        currentMember = Mathf.Clamp(currentMember, 0, battleSystem.PlayerParty.Pokemons.Count - 1);

        battleSystem.UIBattleManager.PartyScreen.UpdatePokemonSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.X))
        {
            Pokemon selectedMember = battleSystem.PlayerParty.Pokemons[currentMember];            

            if (selectedMember.HP <= 0)
            {
                battleSystem.UIBattleManager.PartyScreen.SetMessageText("You can't send out a fainted Pokemon!");
                return;
            }

            if (selectedMember == battleSystem.UIBattleManager.ActivePlayerUnit.Pokemon)
            {
                battleSystem.UIBattleManager.PartyScreen.SetMessageText($"{selectedMember.Base.Name} is already fighting!");
                return;
            }

            battleSystem.CurrentMember = currentMember;
            battleSystem.UIBattleManager.PartyScreen.gameObject.SetActive(false);
            battleSystem.TransitionToState(BattleState.SwitchingPokemon);            
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && battleSystem.UIBattleManager.ActivePlayerUnit.Pokemon.HP > 0)
        {
            battleSystem.TransitionToState(BattleState.ActionSelection);
        }
    }

    public override void ExitState()
    {
        battleSystem.UIBattleManager.ActivePlayerUnit.ReAppearUnit();
        battleSystem.UIBattleManager.ActiveEnemyUnit.ReAppearUnit();
        battleSystem.UIBattleManager.PartyScreen.gameObject.SetActive(false);
    }
}
