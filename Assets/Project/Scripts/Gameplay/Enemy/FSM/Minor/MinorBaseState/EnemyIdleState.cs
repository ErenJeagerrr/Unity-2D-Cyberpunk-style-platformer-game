using UnityEngine;

public class EnemyIdleState : EnemyStateBase
{
    private float Timer;
    public EnemyIdleState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName) { }

    public override void Enter()
    {
        base.Enter();
        enemy.SetVelocityX(0);
        Timer = 0;
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        Timer += Time.deltaTime;

        if (Timer >= enemy.Data.IdleTime)
        {
            enemy.state.ChangeState(enemy.RoamingState);
            return;
        }

        if (enemy.CheckPlayer())
        {
            state.ChangeState(enemy.ChaseState);
            return;
        }
    }
}