using UnityEngine;

public class StormBossHealState : EnemyStateBase
{
    private StormBoss stormBoss;
    private bool hasHealed;

    public StormBossHealState(Enemy enemy, EnemyStateMachine stateMachine, string AniName)
        : base(enemy, stateMachine, AniName)
    {
        stormBoss = enemy as StormBoss;
    }

    public override void Enter()
    {
        base.Enter();
        stormBoss.SetVelocityX(0);
        hasHealed = false;

        // 【新增】进入状态即刻播放音效
        if (AudioService.Instance != null)
        {
            AudioService.Instance.PlayEffect("Storm_Heal");
        }
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        if (AniComplete)
        {
            state.ChangeState(stormBoss.BossChaseState);
        }
    }

    public override void AniEvent()
    {
        if (!hasHealed)
        {
            stormBoss.UseHeal();
            hasHealed = true;
        }
    }

    public override void Exit()
    {
        base.Exit();

        if (AudioService.Instance != null)
        {
            AudioService.Instance.StopEffect();
        }
    }
}