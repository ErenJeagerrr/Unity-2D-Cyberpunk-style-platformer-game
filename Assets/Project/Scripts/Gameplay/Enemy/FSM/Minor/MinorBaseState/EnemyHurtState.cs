using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHurtState : EnemyStateBase
{
    public EnemyHurtState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        enemy.SetVelocityX(0); 
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        if (AniComplete)
        {
            if (enemy.CheckPlayer())
                state.ChangeState(enemy.ChaseState);
            else
                state.ChangeState(enemy.IdleState);
        }
    }
}