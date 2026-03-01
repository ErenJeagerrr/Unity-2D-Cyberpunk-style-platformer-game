using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossIdleState : EnemyStateBase
{
    private BossBase Boss;
    private float timer;

    public BossIdleState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
        Boss = enemy as BossBase;
    }

    public override void Enter()
    {
        base.Enter();
        timer = 0;
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        timer += Time.deltaTime;
        if (timer >= Boss.Data.IdleTime)
        {
            state.ChangeState(Boss.BossChaseState);
        }
    }
}
