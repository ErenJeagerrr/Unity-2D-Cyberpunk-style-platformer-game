using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossChaseState : EnemyStateBase
{
    private BossBase Boss;

    public BossChaseState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
        Boss = enemy as BossBase;
    }
    public override void Enter()
    {
        base.Enter();
    }
    public override void Exit()
    {
        base.Exit();
        Boss.SetVelocityX(0);
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
    }

}
