using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BattleStateBase
{
    protected BattleSystem battleSystem;
    protected BattleState State => battleSystem.State;
    protected BattleState? PreState => battleSystem.PreState;

    public BattleStateBase(BattleSystem battleSystem)
    {
        this.battleSystem = battleSystem;
    }
    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
}
