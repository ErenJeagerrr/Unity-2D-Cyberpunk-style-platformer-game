using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreePhaseBoss : BossBase
{
    [Header("--- Stage Setup ---")]
    public int Phase = 1;
    private bool HasEnterP2 = false;
    private bool HasEnterP3 = false;
    public bool IsImmunity { get; set; } = false;
    public bool IsGuarding { get; set; } = false;
    [Header("--- Skill Parameter Configuration ---")]
    public float FanAngle = 60f;
    public float JumpForce = 15f;
    public float DashSpeed = 12f;
    public float WaveSpeed = 10f;
    [Header("--- Full-Screen Sprint Skill Configuration ---")]
    public float ScreenDashSpeed = 50f;
    public float ScreenDashWarningTime = 0.6f;
    public GameObject DashWarningPrefab;
    public GameObject DashProjectilePrefab;
    public TrailRenderer DashTrail;
    [Header("--- Precast element reference ---")]
    public GameObject WavePrefab;
    public GameObject SpikePrefab;
    public GameObject LandEffectPrefab;
    public GameObject MissilePrefab;
    [Header("--- Cooling Time Monitoring ---")]
    public Dictionary<string, float> SkillCDs = new Dictionary<string, float>();
    private float BaseDashCD = 8f;
    private float BaseJumpCD = 10f;
    private float BaseWaveCD = 10f;
    private float BaseSummonCD = 15f;
    private float BaseTeleportCD = 8f;
    private float BaseScreenDashtCD = 16f;
    private float BaseMissileCD = 12f;
    public BossFanAttackState FanState { get; private set; }
    public BossDashState DashState { get; private set; }
    public BossJumpState JumpState { get; private set; }
    public BossWaveState WaveState { get; private set; }
    public BossSummonState SummonState { get; private set; }
    public BossRoarState RoarState { get; private set; }
    public BossGuardState GuardState { get; private set; }
    public BossTeleportState TeleportState { get; private set; }
    public BossScreenDashState ScreenDashState { get; private set; }
    public BossMissileState MissileState { get; private set; }

    protected override void InitState()
    {
        CurrentAtk = Data.Attack;
        CurrentMoveSpeed = Data.MoveSpeed;
        CurrentChaseSpeed = Data.ChaseSpeed;

        SkillCDs["Dash"] = 0;
        SkillCDs["Jump"] = 0;
        SkillCDs["Wave"] = 0;
        SkillCDs["Summon"] = 0;
        SkillCDs["Teleport"] = 0;
        SkillCDs["ScreenDash"] = 0;
        SkillCDs["Missile"] = 0;

        BossStartState = new BossStartState(this, state, "Idle_Boss");
        BossIdleState = new BossIdleState(this, state, "Idle_Boss");
        BossChaseState = new PhaseBossChaseState(this, state, "Chase_Boss");
        BossDieState = new BossDieState(this, state, "Die_Boss");

        FanState = new BossFanAttackState(this, state, "Attack_Boss");
        DashState = new BossDashState(this, state, "Chase_Boss");
        JumpState = new BossJumpState(this, state, "Chase_Boss");
        WaveState = new BossWaveState(this, state, "Attack_Boss");
        SummonState = new BossSummonState(this, state, "SPAttack_Boss");
        RoarState = new BossRoarState(this, state, "Roar_Boss");
        GuardState = new BossGuardState(this, state, "Guard_Boss");
        TeleportState = new BossTeleportState(this, state, "Idle_Boss");
        ScreenDashState = new BossScreenDashState(this, state, "Idle_Boss");
        MissileState = new BossMissileState(this, state, "Attack_Boss");

        state.Init(BossIdleState);
        SetVelocityX(0);
    }

    public override void Update()
    {
        if (IsDie) return;

        base.Update();

        if (player == null)
        {
            if (PlayerSystem.Instance != null && PlayerSystem.Instance.player != null)
            {
                SetPlayer(PlayerSystem.Instance.player.transform);
            }
        }

        List<string> keys = new List<string>(SkillCDs.Keys);
        foreach (var key in keys)
        {
            if (SkillCDs[key] > 0)
            {
                SkillCDs[key] -= Time.deltaTime;
            }
        }
    }

    public override void Hurt(Transform pos, float Value)
    {
        if (IsDie) return;
        if (IsImmunity) return;

        if (IsGuarding)
        {
            float reducedDamage = 1;
            CurrentHeathl -= reducedDamage;
            if (AudioService.Instance != null)
            {
                AudioService.Instance.PlayEffect("Boss_Guard");
            }
            state.ChangeState(DashState);
            CheckPhase();
            return;
        }

        bool triggerGuard = Phase == 1 && Random.value < 0.15f;
        if (Phase >= 2)
        {
            triggerGuard = Random.value < 0.33f;
        }

        // First determine whether death has occurred.
        float pendingHealth = CurrentHeathl - Value;

        // If the damage is lethal, Guard will not activate; the death sequence will proceed immediately.
        if (pendingHealth <= 0)
        {
            base.Hurt(pos, Value); // Call the parent class to handle death
            return;
        }

        if (triggerGuard)
        {
            CurrentHeathl -= Value;
            if (Data.HitEffect != null)
                Instantiate(Data.HitEffect, Center.position, Quaternion.identity);

            Vector2 Back = (transform.position - pos.position).normalized;
            Back = new Vector2(Back.x * 50, 10);
            rig.AddForce(Back, ForceMode2D.Impulse);

            state.ChangeState(GuardState);
        }
        else
        {
            base.Hurt(pos, Value);
        }

        CheckPhase();
    }

    private void CheckPhase()
    {
        if (IsDie) return;
        float hpPercent = CurrentHeathl / MaxHeathl;

        if (hpPercent < 0.7f && !HasEnterP2)
        {
            HasEnterP2 = true;
            EnterPhase2();
        }
        if (hpPercent < 0.4f && !HasEnterP3)
        {
            HasEnterP3 = true;
            EnterPhase3();
        }
    }

    private void EnterPhase2()
    {
        Phase = 2;
        if (EnemySpriteRenderer() != null) EnemySpriteRenderer().color = Color.yellow;

        CurrentMoveSpeed += 2;
        CurrentChaseSpeed += 2;

        state.ChangeState(RoarState);
        Debug.Log("Boss Phase 2 Activated! ROAR!");
    }

    private void EnterPhase3()
    {
        Phase = 3;
        if (EnemySpriteRenderer() != null) EnemySpriteRenderer().color = Color.red;
        transform.localScale = Vector3.one * 1.3f;

        CurrentAtk = (int)(CurrentAtk * 1.5f);

        BaseDashCD /= 2;
        BaseJumpCD /= 2;
        BaseWaveCD /= 2;
        BaseSummonCD /= 2;
        BaseTeleportCD = 6f;
        state.ChangeState(RoarState);
        Debug.Log("Boss Phase 3 Activated! ROAR!");
    }

    public bool TryUseSkill(string skillName)
    {
        if (!SkillCDs.ContainsKey(skillName)) return false;

        if (SkillCDs[skillName] <= 0)
        {
            float cd = 0;
            switch (skillName)
            {
                case "Dash": cd = BaseDashCD; break;
                case "Jump": cd = BaseJumpCD; break;
                case "Wave": cd = BaseWaveCD; break;
                case "Summon": cd = BaseSummonCD; break;
                case "Teleport": cd = BaseTeleportCD; break;
                case "ScreenDash": cd = BaseScreenDashtCD; break;
                case "Missile": cd = BaseMissileCD; break;
            }
            SkillCDs[skillName] = cd;
            return true;
        }
        return false;
    }

    public void StartCombo(string nextSkill, float delay)
    {
        if (IsDie) return;

        if (player != null)
        {
            if (player.position.x > transform.position.x) Flip(true);
            else Flip(false);
        }

        TimerSystem.Instance.AddTask(delay, () =>
        {
            if (state == null || IsDie || gameObject == null) return;

            switch (nextSkill)
            {
                case "Fan": state.ChangeState(FanState); break;
                case "Jump": state.ChangeState(JumpState); break;
                case "Wave": state.ChangeState(WaveState); break;
                default: state.ChangeState(BossChaseState); break;
            }
        });
    }

    public void CreateAfterImage()
    {
        if (IsDie) return;

        GameObject ghost = new GameObject("BossGhost");
        ghost.transform.position = transform.position;
        ghost.transform.rotation = transform.rotation;
        ghost.transform.localScale = transform.localScale;

        SpriteRenderer sr = ghost.AddComponent<SpriteRenderer>();
        SpriteRenderer mySr = EnemySpriteRenderer();

        if (mySr != null)
        {
            sr.sprite = mySr.sprite;
            sr.flipX = mySr.flipX;
            sr.color = new Color(mySr.color.r, mySr.color.g, mySr.color.b, 0.6f);
            sr.sortingLayerName = mySr.sortingLayerName;
            sr.sortingOrder = mySr.sortingOrder - 1;
        }

        if (ghost.GetComponent<GhostTrail>() == null)
        {
            GhostTrail trail = ghost.AddComponent<GhostTrail>();
            trail.LifeTime = 0.4f;
        }
    }
}