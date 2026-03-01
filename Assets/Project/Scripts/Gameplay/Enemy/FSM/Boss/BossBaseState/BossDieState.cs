using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDieState : EnemyStateBase
{
    public BossDieState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
    }
}
