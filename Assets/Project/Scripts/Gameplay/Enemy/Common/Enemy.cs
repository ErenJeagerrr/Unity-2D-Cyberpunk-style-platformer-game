using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour, IEnemyHurt
{
    public EnemyData Data;

    #region 组件引用
    public Rigidbody2D rig { get; private set; }
    public Animator ani { get; private set; }
    #endregion

    #region 状态机
    public EnemyStateMachine state;
    public EnemyIdleState IdleState { get; set; }
    public EnemyRoamingState RoamingState { get; set; }
    public EnemyChaseState ChaseState { get; set; }
    public EnemyAttackState AttackState { get; set; }
    public EnemyDieState DieState { get; set; }
    #endregion

    public Transform Center;
    public float MaxHeathl { get; set; }
    private float _CurrentHeathl;

    public float CurrentHeathl
    {
        get => _CurrentHeathl;
        set
        {
            _CurrentHeathl = value;
            if (BleedSlider != null)
                BleedSlider.value = Mathf.Clamp01(_CurrentHeathl / MaxHeathl);
        }
    }

    public Vector3 Position => transform.position;
    public bool IsFaceRight { get; private set; }
    public int Dir { get; set; }
    public bool IsAttack { get; set; }
    public bool IsHurtLocked { get; protected set; }

    public Transform FrontPos; 
    public Transform GroundCheckPos;
    public Slider BleedSlider;
    public bool IsDie { get; protected set; }

    protected virtual void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        ani = GetComponentInChildren<Animator>();
        if (ani == null) ani = GetComponent<Animator>();
        state = new EnemyStateMachine();
        MaxHeathl = Data.MaxHeathl;
        CurrentHeathl = MaxHeathl;
        IsFaceRight = true;
        Dir = 1;
        IsCanHurt = true;
        InitState();
    }

    protected virtual void InitState() { }

    public virtual void Update()
    {
        if (IsDie || IsHurtLocked) return;
        state.CurrentState.FrameUpdate();
    }

    protected virtual void FixedUpdate()
    {
        if (IsDie || IsHurtLocked) return;
        state.CurrentState.FixUpdate();
    }

    protected virtual void LateUpdate()
    {
        if (IsDie) return;
        if (BleedSlider != null) BleedSlider.transform.rotation = Quaternion.identity;
    }

    public SpriteRenderer EnemySpriteRenderer()
    {
        if (this == null) return null;
        var sr = GetComponent<SpriteRenderer>();
        return sr == null ? GetComponentInChildren<SpriteRenderer>() : sr;
    }

    public void Flip(bool IsFlipRight)
    {
        if (IsFlipRight && !IsFaceRight)
        {
            IsFaceRight = true; Dir = 1;
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        else if (!IsFlipRight && IsFaceRight)
        {
            IsFaceRight = false; Dir = -1;
            transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
    }

    public void SetVelocityX(float value) { if (rig != null) rig.velocity = new Vector2(value, rig.velocity.y); }
    public void SetVelocityY(float value) { if (rig != null) rig.velocity = new Vector2(rig.velocity.x, value); }

    public GameObject CheckPlayer()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, Data.CheckRadius, LayerMask.GetMask("Player"));
        return hit.collider != null ? hit.collider.gameObject : null;
    }

    public virtual bool CheckIsCanAttack() => true;

    public bool CheckWall()
    {
        if (FrontPos == null) return false;
        return Physics2D.Raycast(FrontPos.position, transform.right, 0.5f, LayerMask.GetMask("Ground"));
    }

    public bool CheckGround()
    {
        if (GroundCheckPos == null) return false;
        return Physics2D.Raycast(GroundCheckPos.position, -transform.up, 0.3f, LayerMask.GetMask("Ground"));
    }


    public bool CheckGroundAhead()
    {
        if (FrontPos != null)
        {
            RaycastHit2D hit = Physics2D.Raycast(FrontPos.position, Vector2.down, 1.0f, LayerMask.GetMask("Ground"));
            return hit.collider != null;
        }
        return CheckGround();
    }


    #region 基础受伤与死亡
    protected bool IsCanHurt { get; set; }

    public virtual void Hurt(Transform pos, float Value)
    {
        if (IsDie || !IsCanHurt) return;
        IsCanHurt = false;
        TimerSystem.Instance.AddTask(0.25f, () => { if (this != null) IsCanHurt = true; });

        CurrentHeathl -= Value;
        if (CurrentHeathl <= 0) HurtToDie();
        else HurtToBack(pos);
    }

    protected virtual void HurtToDie()
    {
        if (IsDie) return;
        state.ChangeState(DieState);
        if (Data.HitEffect != null) Instantiate(Data.HitEffect, Center.position, Quaternion.identity);
    }

    protected virtual void HurtToBack(Transform pos)
    {
        IsHurtLocked = true;
        // ani.Play("Hurt"); TODO: Fix hurt animation
        TimerSystem.Instance.AddTask(0.5f, () => { if (this != null && !IsDie) IsHurtLocked = false; });

        if (Data.HitEffect != null) Instantiate(Data.HitEffect, Center.position, Quaternion.identity);

        Vector2 backDir = (transform.position - pos.position).normalized;
        rig.velocity = Vector2.zero;
        rig.AddForce(new Vector2(backDir.x * 50, 15), ForceMode2D.Impulse);
    }
    #endregion

    public void Die()
    {
        if (IsDie) return;
        IsDie = true;

        this.enabled = false;

        StopAllCoroutines();

        if (GetComponent<BoxCollider2D>() != null) GetComponent<BoxCollider2D>().enabled = false;
        if (rig != null) rig.simulated = false;

        ResService.Instance.CreatCoin(Data.FallCoin, Center.position);

        TimerSystem.Instance.AddTask(1, () =>
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
                int Value = UnityEngine.Random.Range(10, Data.MaxEXP);
                if (PlayerSystem.Instance != null) PlayerSystem.Instance.CurrentEXP += Value;

                TimerSystem.Instance.AddTask(0.2f, () =>
                {
                    if (LevelSystem.Instance != null) LevelSystem.Instance.CheckIsHaveEnemy();
                });
            }
        });
    }

    public virtual void AniEvent() { if (!IsDie) state.CurrentState.AniEvent(); }
}