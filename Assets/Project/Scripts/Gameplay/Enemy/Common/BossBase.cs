using System.Collections.Generic;
using UnityEngine;

public class BossBase : Enemy
{
    [Header("--- Boss Specific Properties ---")]
    public Transform BossAttackPos;
    public Vector3 BossAttackRadius;
    public Transform player { get; set; }

    #region State References
    public BossStartState BossStartState { get; protected set; }
    public BossIdleState BossIdleState { get; protected set; }
    public BossChaseState BossChaseState { get; protected set; }
    public BossAttackState BossAttackState { get; protected set; }
    public BossDieState BossDieState { get; protected set; }
    #endregion

    #region Runtime Properties (修复报错：补充被误删的变量)
    [HideInInspector] public int CurrentAtk;
    [HideInInspector] public int CurrentMoveSpeed;
    [HideInInspector] public int CurrentChaseSpeed;
    #endregion

    protected override void Start()
    {
        base.Start();

        if (Data != null)
        {
            CurrentAtk = Data.Attack;
            CurrentMoveSpeed = Data.MoveSpeed;
            CurrentChaseSpeed = Data.ChaseSpeed;
        }

        if (PlayerSystem.Instance != null && PlayerSystem.Instance.player != null)
        {
            SetPlayer(PlayerSystem.Instance.player.transform);
        }
    }

    public void SetPlayer(Transform player) => this.player = player;

    public override bool CheckIsCanAttack()
    {
        if (BossAttackPos == null) return false;
        Collider2D collider = Physics2D.OverlapBox(BossAttackPos.position, BossAttackRadius, 0, LayerMask.GetMask("Player"));
        return collider != null;
    }

    // The boss does not need to be affected by stun or knockback effects.
    public override void Hurt(Transform pos, float Value)
    {
        if (!IsCanHurt) return;
        IsCanHurt = false;
        TimerSystem.Instance.AddTask(0.25f, () => { if (this != null) IsCanHurt = true; });

        CurrentHeathl -= Value;

        if (Data.HitEffect != null)
            Instantiate(Data.HitEffect, Center.position, Quaternion.identity);

        if (CurrentHeathl <= 0)
        {
            state.ChangeState(BossDieState);
            Die();
        }
    }

    protected virtual void OnDrawGizmos()
    {
        if (BossAttackPos != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(BossAttackPos.position, BossAttackRadius);
        }
    }
}