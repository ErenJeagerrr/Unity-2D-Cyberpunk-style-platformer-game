using UnityEngine;

public class BossGuardState : EnemyStateBase
{
    private ThreePhaseBoss Boss;
    private float timer;
    private float guardDuration = 1.5f;

    public BossGuardState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
        Boss = enemy as ThreePhaseBoss;
    }

    public override void Enter()
    {
        base.Enter();
        Boss.SetVelocityX(0);
        Boss.IsGuarding = true;
        timer = 0;

        if (Boss.EnemySpriteRenderer() != null)
        {
            Boss.EnemySpriteRenderer().color = Color.cyan;
        }
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        timer += Time.deltaTime;

        if (timer >= guardDuration)
        {
            state.ChangeState(Boss.BossChaseState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        Boss.IsGuarding = false;

        if (Boss.EnemySpriteRenderer() != null)
        {
            if (Boss.Phase == 3) Boss.EnemySpriteRenderer().color = Color.red;
            else if (Boss.Phase == 2) Boss.EnemySpriteRenderer().color = Color.yellow;
            else Boss.EnemySpriteRenderer().color = Color.white;
        }
    }
}