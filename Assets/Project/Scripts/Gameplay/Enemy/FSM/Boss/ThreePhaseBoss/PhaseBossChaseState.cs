using UnityEngine;

public class PhaseBossChaseState : BossChaseState
{
    private ThreePhaseBoss PhaseBoss;

    public PhaseBossChaseState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
        PhaseBoss = enemy as ThreePhaseBoss;
    }

    public override void Enter()
    {
        base.Enter();
        // 已移除所有卡死检测相关的变量初始化 (positionAtLastCheck, checkTimer, stuckDuration 等)
        Debug.Log($"<color=cyan>[BossChase] 进入追逐状态: {PhaseBoss.gameObject.name}</color>");
    }

    public override void FrameUpdate()
    {
        // 1. 动画状态保护
        AnimatorStateInfo info = PhaseBoss.ani.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName(AniName))
        {
            PhaseBoss.ani.Play(AniName);
        }

        if (PhaseBoss.player == null) return;

        float dist = Vector2.Distance(PhaseBoss.transform.position, PhaseBoss.player.position);

        // --- 技能判定逻辑 ---

        if (PhaseBoss.TryUseSkill("ScreenDash"))
        {
            state.ChangeState(PhaseBoss.ScreenDashState);
            return;
        }

        if (PhaseBoss.Phase == 3)
        {
            if (PhaseBoss.TryUseSkill("Summon"))
            {
                state.ChangeState(PhaseBoss.SummonState);
                return;
            }
        }

        if (PhaseBoss.Phase >= 2)
        {
            if (dist > 7f && PhaseBoss.TryUseSkill("Teleport"))
            {
                state.ChangeState(PhaseBoss.TeleportState);
                return;
            }

            if (dist > 3f && dist <= 8f && PhaseBoss.TryUseSkill("Wave"))
            {
                state.ChangeState(PhaseBoss.WaveState);
                return;
            }
        }

        if (dist > 3.5f && PhaseBoss.TryUseSkill("Dash"))
        {
            state.ChangeState(PhaseBoss.DashState);
            return;
        }

        bool closeRangeMixup = dist < 1.5f;
        bool midRangeGapClose = dist > 2.5f;

        if ((midRangeGapClose || closeRangeMixup) && PhaseBoss.TryUseSkill("Jump"))
        {
            state.ChangeState(PhaseBoss.JumpState);
            return;
        }

        if (PhaseBoss.TryUseSkill("Missile"))
        {
            state.ChangeState(PhaseBoss.MissileState);
            return;
        }

        if (PhaseBoss.CheckIsCanAttack())
        {
            state.ChangeState(PhaseBoss.FanState);
            return;
        }

        // --- 基础移动逻辑 ---
        MoveTowardsPlayer();
    }

    private void MoveTowardsPlayer()
    {
        float xDiff = PhaseBoss.player.position.x - PhaseBoss.transform.position.x;

        if (Mathf.Abs(xDiff) > 0.5f)
        {
            if (xDiff > 0)
            {
                PhaseBoss.Flip(true);
                PhaseBoss.SetVelocityX(PhaseBoss.Data.ChaseSpeed);
            }
            else
            {
                PhaseBoss.Flip(false);
                PhaseBoss.SetVelocityX(-PhaseBoss.Data.ChaseSpeed);
            }
        }
        else
        {
            PhaseBoss.SetVelocityX(0);
        }
    }
}