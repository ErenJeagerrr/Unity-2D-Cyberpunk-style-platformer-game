using UnityEngine;

public class BossFanAttackState : EnemyStateBase
{
    private ThreePhaseBoss Boss;
    public BossFanAttackState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
        Boss = enemy as ThreePhaseBoss;
    }

    public override void Enter()
    {
        base.Enter();
        Boss.SetVelocityX(0);
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
        Collider2D[] colliders = Physics2D.OverlapCircleAll(Boss.BossAttackPos.position, Boss.Data.AttackRadius, LayerMask.GetMask("Player"));

        foreach (var target in colliders)
        {
            Vector3 dirToTarget = (target.transform.position - Boss.transform.position).normalized;
            
            Vector3 faceDir = Boss.IsFaceRight ? Vector3.right : Vector3.left;

            float angle = Vector3.Angle(faceDir, dirToTarget);

            
            if (angle < Boss.FanAngle / 2)
            {
                target.GetComponent<IHurt>()?.Hurt(Boss.transform, Boss.Data.Attack);
            }
        }
    }
}