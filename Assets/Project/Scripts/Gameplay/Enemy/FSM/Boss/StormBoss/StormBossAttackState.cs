using UnityEngine;

public class StormBossAttackState : EnemyStateBase
{
    private StormBoss stormBoss;

    public StormBossAttackState(Enemy enemy, EnemyStateMachine stateMachine, string AniName)
        : base(enemy, stateMachine, AniName)
    {
        stormBoss = enemy as StormBoss;
    }

    public override void Enter()
    {
        base.Enter();
        stormBoss.SetVelocityX(0);

        if (AudioService.Instance != null)
        {
            AudioService.Instance.PlayEffect("StormBoss_Attack02");
        }
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        if (AniComplete)
        {
            state.ChangeState(stormBoss.BossIdleState);
        }
    }

    public override void AniEvent()
    {
        if (stormBoss.BossAttackPos == null) return;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            stormBoss.BossAttackPos.position,
            stormBoss.BossAttackRadius,
            0,
            LayerMask.GetMask("Player")
        );

        foreach (var hit in hits)
        {
            hit.GetComponent<IHurt>()?.Hurt(stormBoss.transform, stormBoss.CurrentAtk);
        }

        if (hits.Length > 0 && AttackEffect.Instance != null)
        {
            AttackEffect.Instance.Shake();
        }
    }
}