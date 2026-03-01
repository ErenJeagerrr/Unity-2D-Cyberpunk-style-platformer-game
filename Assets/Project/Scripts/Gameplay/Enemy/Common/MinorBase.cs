using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MinorBase : Enemy
{
    [Header("--- Soldier-Specific Configuration ---")]
    public string AniSuffix;

    public float AttackCDTimer { get; set; }
    public bool IsReacting { get; private set; }
    public EnemyHurtState HurtState { get; set; }

    protected override void InitState()
    {
        IdleState = new EnemyIdleState(this, state, "Idle_" + AniSuffix);
        RoamingState = new EnemyRoamingState(this, state, "Run_" + AniSuffix);
        ChaseState = new EnemyChaseState(this, state, "Run_" + AniSuffix);
        AttackState = new EnemyAttackState(this, state, "Attack_" + AniSuffix);
        HurtState = new EnemyHurtState(this, state, "Hurt_" + AniSuffix);
        DieState = new EnemyDieState(this, state, "Die_" + AniSuffix);

        state.Init(IdleState);
    }

    public override void Update()
    {
        if (IsDie || IsHurtLocked || IsReacting) return;

        if (AttackCDTimer > 0) AttackCDTimer -= Time.deltaTime;

        base.Update();
    }

    #region Reaction Logic
    public void StartReact(Action onComplete)
    {
        if (IsReacting || IsAttack) return;
        StartCoroutine(ReactCoroutine(onComplete));
    }

    private IEnumerator ReactCoroutine(Action onComplete)
    {
        IsReacting = true;
        SetVelocityX(0);
        ani.Play("Idle_" + AniSuffix);

        yield return new WaitForSeconds(Data.ReactTime);

        IsReacting = false;
        if (!IsDie && !IsHurtLocked)
        {
            onComplete?.Invoke();
        }
    }
    #endregion

    #region Attack Stun
    protected override void HurtToBack(Transform pos)
    {
        StopAllCoroutines();
        IsReacting = false;

        state.ChangeState(HurtState);

        base.HurtToBack(pos);
    }
    #endregion
}