using UnityEngine;

public class EnemyAttackState : EnemyStateBase
{
    public EnemyAttackState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        enemy.SetVelocityX(0);
        enemy.IsAttack = true;

        if (enemy is MinorBase minor && minor.AniSuffix == "SluggerBuck")
        {
            if (AudioService.Instance != null)
                AudioService.Instance.PlayEffect("SluggerBuck_Attack");
        }
    }

    public override void Exit()
    {
        base.Exit();
        enemy.IsAttack = false;

        // 攻击结束后设置冷却时间
        if (enemy is MinorBase minor)
        {
            minor.AttackCDTimer = enemy.Data.AttackCD;
        }
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        if (AniComplete)
        {
            // 攻击完切回追击状态
            state.ChangeState(enemy.ChaseState);
        }
    }
}