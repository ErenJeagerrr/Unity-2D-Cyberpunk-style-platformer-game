using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player controller script
/// </summary>
public class Player : MonoBehaviour, IFriendHurt
{
    #region Components

    public Rigidbody2D rig { get; private set; }
    public Animator ani { get; private set; }

    #endregion Components

    #region State Machine

    public StateMachine state;
    public IdleState IdleState;
    public RunState RunState;
    public JumpUpState JumpUpState;
    public JumpDownState JumpDownState;
    public AttackState AttackState;
    public GunSkillState GunSkillState;
    public PauseState PauseState;
    public HurtState HurtState;
    public DieState DieState;
    public DashState DashState;
    public DashJumpState DashJumpState;
    public BurnSkillState BurnSkillState;
    public DoubleJumpUpState DoubleJumpUpState;
    public DoubleJumpDownState DoubleJumpDownState;

    #endregion State Machine

    #region Jump Settings

    [Header("Jump Detection")]
    public Transform JumpCheckTrans; // Jump detection point
    public float JumpCheckRadius; // Jump detection range
    public bool IsOnGround { get; private set; } // Is on ground
    public bool IsFalling { get; private set; } // Is falling

    [Header("Jump Forces")]
    [Tooltip("Normal jump force")]
    public float normalJumpForce = 190f;

    [Tooltip("Double jump force")]
    public float doubleJumpForce = 190f;

    [Header("Jump Count")]
    [Tooltip("Maximum number of jumps (1 = single jump, 2 = double jump, etc.)")]
    public int MaxJumpCount = 2; // Max jump count
    public int CurrentJumpCount { get; set; } // Current jump count

    #endregion Jump Settings

    #region Dash Jump Settings

    [Header("Dash Jump")]
    [Tooltip("Maximum number of dash jumps in air before landing")]
    public int MaxDashJumpCount = 2;
    public int CurrentDashJumpCount { get; set; }

    [Tooltip("Horizontal speed during dash jump")]
    public float dashJumpHorizontalSpeed = 18f;

    [Tooltip("Vertical force for dash jump")]
    public float dashJumpVerticalForce = 150f;

    #endregion Dash Jump Settings

    #region State Flags

    // Can dash
    private bool _isCanDash;
    public bool IsCanDash
    {
        get
        {
            return _isCanDash;
        }
        set { _isCanDash = value; }
    }
    public bool IsCanAirAttack { get; set; } // Can air attack
    private bool IsInit;
    private bool IsFaceRight = true; // Facing right
    public int Dir => IsFaceRight ? 1 : -1; // Direction

    public bool IsDash { get; set; } // Is dashing

    #endregion State Flags

    public void Init()
    {
        // Get components
        rig = GetComponent<Rigidbody2D>();
        ani = GetComponentInChildren<Animator>();
        state = new StateMachine();
        // Initialize states
        InitState();
        state.Init(IdleState);
        // Initialize flags
        _isCanDash = true;
        IsCanAirAttack = true;
        IsCanHurt = true;
        IsInit = true;
    }

    #region Gun Skill Settings

    [Header("Gun Skill")]
    [Tooltip("Bullet prefab")]
    public GameObject bulletPrefab;

    [Tooltip("Fire point")]
    public Transform firePoint;

    [Tooltip("Bullet flight speed")]
    public float bulletSpeed = 10f;

    [Tooltip("Bullet exists time")]
    public float bulletLifetime = 1f;

    [Tooltip("Damage multiplier (Attack * damageMultiplier)")]
    public int gunSkillDamageMultiplier = 4;

    [Tooltip("Skill cooldown time")]
    public float gunSkillCooldown = 3f;

    // Private variables
    private float lastGunSkillTime = -999f;

    // Check if gun skill is ready
    public bool CanUseGunSkill()
    {
        // Check cooldown
        if (Time.time < lastGunSkillTime + gunSkillCooldown)
        {
            float remaining = (lastGunSkillTime + gunSkillCooldown) - Time.time;
            Debug.Log($"<color=yellow>Gun skill on cooldown: {remaining:F1}s remaining</color>");
            return false;
        }

        // Can't use in certain states
        if (state.CurrentState == DieState || state.CurrentState == HurtState)
        {
            return false;
        }

        return true;
    }

    // Record gun skill usage
    public void UseGunSkill()
    {
        lastGunSkillTime = Time.time;
    }

    // Get cooldown info (for UI display)
    public float GetGunSkillCDRemaining()
    {
        return Mathf.Max(0, (lastGunSkillTime + gunSkillCooldown) - Time.time);
    }

    public float GetGunSkillCDPercent()
    {
        float elapsed = Time.time - lastGunSkillTime;
        return Mathf.Clamp01(elapsed / gunSkillCooldown);
    }

    #endregion Gun Skill Settings

    #region Burn Skill Settings

    [Header("Burn Skill")]
    [Tooltip("Burn area component")]
    public BurnArea burnArea;

    [Tooltip("Skill cooldown time")]
    public float burnSkillCooldown = 10f;

    [Tooltip("Burn activation delay time")]
    public float burnActivationDelay = 0.3f;

    [Tooltip("Burn area duration")]
    public float burnDuration = 1.5f;

    [Tooltip("Burn area radius")]
    public float burnAreaRadius = 2.5f;

    [Tooltip("Initial burst damage")]
    public float burnInitialDamage = 50f;

    [Tooltip("Damage over time per tick")]
    public float burnDOTDamage = 10f;

    [Tooltip("Time between DOT ticks")]
    public float burnTickInterval = 0.5f;

    [Tooltip("Burn debuff duration")]
    public float burnDebuffDuration = 3f;

    [Tooltip("If true, player has super armor during burn skill (can't be interrupted by damage)")]
    public bool burnSkillHasSuperArmor = false;

    // Private variables
    private float lastBurnSkillTime = -999f;

    // Check if burn skill is ready
    public bool CanUseBurnSkill()
    {
        // Must be on ground
        if (!IsOnGround)
        {
            Debug.Log("<color=yellow>Can't use Burn skill in air!</color>");
            return false;
        }

        // Check cooldown
        if (Time.time < lastBurnSkillTime + burnSkillCooldown)
        {
            float remaining = (lastBurnSkillTime + burnSkillCooldown) - Time.time;
            Debug.Log($"<color=yellow>Burn skill on cooldown: {remaining:F1}s remaining</color>");
            return false;
        }

        // Can't use in certain states
        if (state.CurrentState == DieState || state.CurrentState == HurtState)
        {
            return false;
        }

        return true;
    }

    // Use burn skill (only records cooldown, state change happens elsewhere)
    public void UseBurnSkill()
    {
        lastBurnSkillTime = Time.time;
        Debug.Log("<color=orange>Burn skill cooldown started!</color>");
    }

    // Get cooldown info (for UI display)
    public float GetBurnSkillCDRemaining()
    {
        return Mathf.Max(0, (lastBurnSkillTime + burnSkillCooldown) - Time.time);
    }

    public float GetBurnSkillCDPercent()
    {
        float elapsed = Time.time - lastBurnSkillTime;
        return Mathf.Clamp01(elapsed / burnSkillCooldown);
    }

    #endregion Burn Skill Settings

    // Initialize states
    public void InitState()
    {
        IdleState = new IdleState(this, state, "Idle");
        RunState = new RunState(this, state, "Run");
        JumpUpState = new JumpUpState(this, state, "JumpUp");
        JumpDownState = new JumpDownState(this, state, "JumpDown");
        AttackState = new AttackState(this, state, "Attack");
        GunSkillState = new GunSkillState(this, state, "Gun");
        PauseState = new PauseState(this, state, "Idle");
        HurtState = new HurtState(this, state, "Hurt");
        DieState = new DieState(this, state, "Die");
        DashState = new DashState(this, state, "Dash");
        DashJumpState = new DashJumpState(this, state, "DashJump");
        BurnSkillState = new BurnSkillState(this, state, "Burn");
        DoubleJumpUpState = new DoubleJumpUpState(this, state, "DoubleJumpUp");
        DoubleJumpDownState = new DoubleJumpDownState(this, state, "DoubleJumpDown");
    }

    // Update state logic every frame if game is not paused
    private void Update()
    {
        if (!IsInit)
            return;
        if (GameRoot.Instance.IsPause)
            return;
        state.CurrentState.FrameUpdate();
    }

    private void FixedUpdate()
    {
        if (!IsInit)
            return;
        state.CurrentState.FixUpdate();

        // Check if on ground
        bool wasOnGround = IsOnGround;
        IsOnGround = Physics2D.OverlapCircle(JumpCheckTrans.position, JumpCheckRadius, LayerMask.GetMask("Ground"));

        // Check if falling
        IsFalling = rig.velocity.y < -0.1f;

        // Reset jump count when landing
        if (IsOnGround && !wasOnGround)
        {
            CurrentJumpCount = 0;
            CurrentDashJumpCount = 0;
        }
    }

    // Play animation
    public void PlayAni(string AniName)
    {
        ani.Play(AniName);
    }

    // Enter pause state
    public void PausePlayer()
    {
        state.ChangeState(PauseState);
    }

    // Continue from pause
    public void ContinuePlayer()
    {
        state.ChangeState(IdleState);
    }

    /// <summary>
    /// Flip character
    /// </summary>
    /// <param name="Right">Facing right?</param>
    public void Flip(bool Right)
    {
        if (Right && !IsFaceRight)
        {
            IsFaceRight = true;
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (!Right && IsFaceRight)
        {
            IsFaceRight = false;
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    // Set X velocity
    public void SetVelocityX(int Value)
    {
        rig.velocity = new Vector2(Value, rig.velocity.y);
    }

    // Set Y velocity
    public void SetVelocityY(int Value)
    {
        rig.velocity = new Vector2(rig.velocity.x, Value);
    }

    // Set velocity
    public void SetVelocity(Vector2 Value)
    {
        rig.velocity = Value;
    }

    // Animation event
    public void AniEvent()
    {
        state.CurrentState.AniEvent();
    }

    // Can take damage, invincible for a period after being hurt
    private bool IsCanHurt;

    // Implement hurt interface
    public void Hurt(Transform pos, float Value)
    {
        if (DeveloperMode.Instance != null && DeveloperMode.Instance.ShouldBlockDamage())
        {
            Debug.Log("<color=cyan>[Dev Mode] God Mode - Block Damage</color>");
            return;
        }

        if (state.CurrentState == BurnSkillState && burnSkillHasSuperArmor)
        {
            Debug.Log("<color=lime>[Super Armor] Burn Skill - Complete Immunity!</color>");
            return;
        }


        if (!IsCanHurt || PlayerSystem.Instance.IsImmunity || IsDash)
        {
            return;
        }

        IsCanHurt = false;
        TimerSystem.Instance.RemoveTask("PlayerHurt");
        TimerSystem.Instance.AddTask(1f, () => { IsCanHurt = true; }, "PlayerHurt");

        AttackEffect.Instance.StopFrame(12);
        AudioService.Instance.PlayEffect("IHurt");

        PlayerSystem.Instance.CurrentHeathl -= Value;

        if (PlayerSystem.Instance.CurrentHeathl <= 0)
        {
            Die();
            return;
        }

        state.ChangeState(HurtState);

        Vector2 BackDir = transform.position - pos.position;
        BackDir = new Vector2(BackDir.normalized.x * 8, 15);
        rig.AddForce(BackDir, ForceMode2D.Impulse);
    }

    // Die
    private void Die()
    {
        state.ChangeState(DieState);
        rig.simulated = false;
        GetComponent<BoxCollider2D>().enabled = false;

        // Display death panel after 1 second
        TimerSystem.Instance.AddTask(1, () =>
        {
            UIService.Instance.ShowPanel<DiePanel>();
        });

        // Destroy player object after 2 seconds to prevent corpse in next scene
        TimerSystem.Instance.AddTask(2, () =>
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        });
    }

    // For testing
    [HideInInspector]
    public List<PlayerAttackData> AttackDatas = new List<PlayerAttackData>();

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(JumpCheckTrans.position, JumpCheckRadius);
    }
}

// Player attack data class
[System.Serializable]
public class PlayerAttackData
{
    public Vector3 AttackPos; // Position relative to player
    public Vector3 AttackRaduis; // Attack range
}