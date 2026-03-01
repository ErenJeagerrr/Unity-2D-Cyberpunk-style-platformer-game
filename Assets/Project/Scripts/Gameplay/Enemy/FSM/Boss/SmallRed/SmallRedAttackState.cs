using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallRedAttackState : BossAttackState
{
    private SmallRed smallRed;

    public SmallRedAttackState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
        smallRed = enemy as SmallRed;
    }

    public override void Enter()
    {
        base.Enter();
        smallRed.SetVelocityX(0);
    }

    public override void AniEvent()
    {
        if (smallRed.BossAttackPos == null) return;

        if (AudioService.Instance != null)
        {
            AudioService.Instance.PlayEffect("Attack2");
        }

        Collider2D collider = Physics2D.OverlapBox(
            smallRed.BossAttackPos.position,
            smallRed.BossAttackRadius,
            0,
            LayerMask.GetMask("Player")
        );

        if (collider != null)
        {

            collider.GetComponent<IHurt>()?.Hurt(smallRed.transform, smallRed.CurrentAtk);

            if (AttackEffect.Instance != null)
            {
                AttackEffect.Instance.Shake();
            }
        }
    }
}