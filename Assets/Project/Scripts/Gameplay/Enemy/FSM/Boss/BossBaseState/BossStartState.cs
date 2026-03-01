using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStartState : EnemyStateBase
{
    private BossBase Boss;
    public BossStartState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
        Boss = enemy as BossBase;
    }
    public override void Enter()
    {
        base.Enter();
        enemy.Flip(false);
    }
    public override void FrameUpdate()
    {
        base.FrameUpdate();
        if (Boss.CheckPlayer())
        {
            Boss.SetPlayer(Boss.CheckPlayer().transform);
            state.ChangeState(Boss.BossIdleState);
        }
    }
}
