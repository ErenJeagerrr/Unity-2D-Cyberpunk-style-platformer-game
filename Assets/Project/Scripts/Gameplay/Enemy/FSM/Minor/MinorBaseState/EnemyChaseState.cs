using UnityEngine;

public class EnemyChaseState : EnemyStateBase
{
    private GameObject Target;
    public EnemyChaseState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName) { }

    public override void Enter()
    {
        base.Enter();
        Target = enemy.CheckPlayer();
        enemy.ani.speed = 1.5f;
    }

    public override void Exit()
    {
        base.Exit();
        enemy.ani.speed = 1f;
        enemy.SetVelocityX(0);
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        Target = enemy.CheckPlayer();

        if (Target != null)
        {
            float xDiff = Target.transform.position.x - enemy.Position.x;

            if (Mathf.Abs(xDiff) >= 0.5f)
            {
                bool moveRight = xDiff > 0;
                enemy.Flip(moveRight);

                if (enemy.CheckGround())
                {
                    enemy.SetVelocityX(moveRight ? enemy.Data.ChaseSpeed : -enemy.Data.ChaseSpeed);
                }
                else
                {
                    enemy.SetVelocityX(0);
                }
            }
            else
            {
                enemy.SetVelocityX(0);
            }

            if (CheckIsCanAttack())
            {
                state.ChangeState(enemy.AttackState);
                return;
            }
        }
        else
        {
            state.ChangeState(enemy.IdleState);
            return;
        }
    }

    public virtual bool CheckIsCanAttack() => enemy.CheckIsCanAttack();
}