using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDieState : EnemyStateBase
{
    public EnemyDieState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
    }
    public override void Enter()
    {
        base.Enter();
        enemy.Die();
        enemy.SetVelocityX(0);
        enemy.SetVelocityY(0);
    }
}
