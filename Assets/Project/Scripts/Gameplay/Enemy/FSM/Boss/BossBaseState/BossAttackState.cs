using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackState : EnemyStateBase
{
    private BossBase Boss;
    public BossAttackState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
        Boss = enemy as BossBase;
    }
    public override void Enter()
    {
        base.Enter();
    }
    public override void FrameUpdate()
    {
        base.FrameUpdate();
        if (AniComplete)
        {
            state.ChangeState(Boss.BossIdleState);
        }
    }
    public override void AniEvent()
    {
    }
}
