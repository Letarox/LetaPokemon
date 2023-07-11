using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSelectionState : BattleStateBase
{
    int currentAction;
    public ActionSelectionState(BattleSystem battleSystem) : base(battleSystem)
    {
    }

    public override void EnterState()
    {
        // Set up the initial state when entering the ActionState
        battleSystem.UIBattleManager.DialogueBox.SetDialogue("Choose an action.");
        battleSystem.UIBattleManager.DialogueBox.EnableActionSelector(true);
        battleSystem.UIBattleManager.DialogueBox.EnableDialogueText(true);
        currentAction = battleSystem.CurrentAction;
    }

    public override void UpdateState()
    {
        // Handle the action selection logic in the ActionState
        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentAction++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentAction--;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentAction -= 2;

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        battleSystem.UIBattleManager.DialogueBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.X))
        {
            switch (currentAction)
            {
                case 0: // FIGHT
                    battleSystem.TransitionToState(BattleState.MoveSelection);
                    break;
                case 1: // BAG
                    // Handle the Bag action
                    break;
                case 2: // POKEMON
                    battleSystem.PreState = battleSystem.State;
                    battleSystem.TransitionToState(BattleState.PartyScreen);
                    break;
                case 3: // RUN
                    // Handle the Run action
                    break;
                default:
                    break;
            }
        }
    }

    public override void ExitState()
    {
        battleSystem.UIBattleManager.DialogueBox.EnableActionSelector(false);
        battleSystem.CurrentAction = currentAction;        
    }
}
