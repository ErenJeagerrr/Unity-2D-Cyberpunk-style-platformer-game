using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurrentGuardAttackState : EnemyAttackState
{
    private TurretGuard TurretGuard;
    public TurrentGuardAttackState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
        TurretGuard = enemy as TurretGuard;
    }

    public override void Enter()
    {
        base.Enter();

        if (AudioService.Instance != null)
            AudioService.Instance.PlayEffect("TurretGuard_Attack");
    }

    public override void AniEvent()
    {
        base.AniEvent();
        GameObject go = GameObject.Instantiate(TurretGuard.ArrowPrefab);
        go.transform.position = TurretGuard.ArrowPos.position;
        go.GetComponent<EnemyArrow>().Init(enemy.transform.right, enemy.Data.Attack);
    }
}
