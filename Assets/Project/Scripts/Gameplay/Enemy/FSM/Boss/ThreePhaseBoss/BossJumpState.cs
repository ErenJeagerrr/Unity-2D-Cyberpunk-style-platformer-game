using UnityEngine;

public class BossJumpState : EnemyStateBase
{
    private ThreePhaseBoss Boss;
    private bool hasLanded = false;
    private float timer;

    // === 修复核心：最大滞空时间保险 ===
    private const float MAX_AIR_TIME = 2.0f; // 如果跳了2秒还没落地，强制结算

    public BossJumpState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
        Boss = enemy as ThreePhaseBoss;
    }

    public override void Enter()
    {
        base.Enter();
        hasLanded = false;
        timer = 0;

        float jumpX = Boss.Dir * (Boss.DashSpeed * 0.8f);
        float jumpY = Boss.JumpForce;

        if (jumpY < 15) jumpY = 20;

        Boss.rig.velocity = new Vector2(jumpX, jumpY);
    }

    public override void FrameUpdate()
    {
        timer += Time.deltaTime;

        // 起跳保护期：防止刚按下跳跃还没离地就被判定为落地
        // 0.15秒通常足够起跳离地了
        if (timer < 0.15f) return;

        // === 修复逻辑 1: 超时强制落地 (保险) ===
        if (timer > MAX_AIR_TIME)
        {
            Debug.LogWarning($"[BossJump] 滞空超时 ({MAX_AIR_TIME}s)，强制落地！");
            PerformLanding();
            return;
        }

        // === 优化逻辑: 极速落地响应 ===
        // 只要碰到地面Layer直接触发，不再等待速度稳定
        // CheckGround() 发射的是向下射线，只要检测到地面即视为落地
        if (Boss.CheckGround())
        {
            if (!hasLanded)
            {
                // 可选：为了逻辑更严谨，通常只在下落阶段(velocity.y <= 0)才判定落地
                // 但如果追求极致的“碰到就算”，且有起跳保护期，直接触发也可以
                // 这里加一个宽容的判定：只要不是快速上升中(velocity.y > 5.0f)就行，防止贴墙跳时误触
                if (Boss.rig.velocity.y <= 5.0f)
                {
                    PerformLanding();
                }
            }
        }
    }

    private void PerformLanding()
    {
        if (hasLanded) return;
        hasLanded = true;

        // 核心：一检测到落地，立刻把速度归零，消除物理滑行感
        Boss.SetVelocityX(0);
        Boss.SetVelocityY(0); // 确保垂直方向也停住

        LandAttack(); // 触发落地伤害和特效

        // 切换回追逐状态或连招
        if (Boss.Phase >= 2 && Random.value < 0.3f)
        {
            Boss.StartCombo("Fan", 0.3f);
        }
        else
        {
            // 极短延迟后切回 Chase，保证特效能播放一瞬间
            TimerSystem.Instance.AddTask(0.1f, () => {
                if (Boss != null && state != null && state.CurrentState == this)
                    state.ChangeState(Boss.BossChaseState);
            });
        }
    }

    private void LandAttack()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(Boss.transform.position, 3f, LayerMask.GetMask("Player"));
        foreach (var c in cols)
        {
            c.GetComponent<IHurt>()?.Hurt(Boss.transform, Boss.Data.Attack * 1.5f);
        }

        if (AttackEffect.Instance != null)
            AttackEffect.Instance.Shake();

        if (Boss.GetComponent<ThreePhaseBoss>() != null)
            Boss.CreateAfterImage();

        // 只有当检测点有效时才生成特效
        if (Boss.LandEffectPrefab != null && Boss.GroundCheckPos != null)
        {
            Vector3 offset = new Vector3(0, 0.5f, 0);
            Vector3 spawnPos = Boss.GroundCheckPos.position;
            spawnPos -= offset;
            GameObject effect = GameObject.Instantiate(Boss.LandEffectPrefab, spawnPos, Quaternion.identity);
            effect.transform.localScale = Vector3.one * 2f;
            GameObject.Destroy(effect, 0.6f);
        }

        if (AudioService.Instance != null)
        {
            AudioService.Instance.PlayEffect("Boss_Land");
        }
    }
}