using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurrentGuardChaseState : EnemyChaseState
{
    private TurretGuard TurretGuard;
    public TurrentGuardChaseState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
        TurretGuard = enemy as TurretGuard;
    }
    public override bool CheckIsCanAttack()
    {
        return TurretGuard.CheckIsCanAttack();
    }
}
