using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSelectionState : BattleStateBase
{
    private RunningTurnState runningTurnState;
    int currentMove;

    public void SetRunningTurnState(RunningTurnState runningTurnState)
    {
        this.runningTurnState = runningTurnState;
    }

    public MoveSelectionState(BattleSystem battleSystem) : base(battleSystem)
    {
    }

    public override void EnterState()
    {
        // Set up the initial state when entering the MoveSelectionState
        battleSystem.DialogueBox.EnableActionSelector(false);
        battleSystem.DialogueBox.EnableDialogueText(false);
        battleSystem.DialogueBox.EnableMoveSelector(true);
        currentMove = battleSystem.CurrentMove;
    }

    public override void UpdateState()
    {
        // Handle the move selection logic in the MoveSelectionState
        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentMove++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentMove--;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMove -= 2;

        currentMove = Mathf.Clamp(currentMove, 0, battleSystem.ActivePlayerUnit.Pokemon.Moves.Count - 1);

        battleSystem.DialogueBox.UpdateMoveSelection(currentMove, battleSystem.ActivePlayerUnit.Pokemon.Moves[currentMove], battleSystem.ActiveEnemyUnit.Pokemon);

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (battleSystem.ActivePlayerUnit.Pokemon.Moves[currentMove].PP == 0)
                return;

            battleSystem.CurrentMove = currentMove;
            battleSystem.DialogueBox.EnableMoveSelector(false);
            battleSystem.DialogueBox.EnableDialogueText(true);
            battleSystem.TransitionToState(BattleState.RunningTurn, () => battleSystem.StartCoroutine(runningTurnState.RunTurn()));
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            battleSystem.TransitionToState(BattleState.ActionSelection);
        }
    }


    public override void ExitState()
    {
        battleSystem.DialogueBox.EnableMoveSelector(false);
        battleSystem.CurrentMove = currentMove;
    }
}
