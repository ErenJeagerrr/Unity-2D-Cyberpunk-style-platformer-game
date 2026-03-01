using UnityEngine;
using System.Collections;

public class BossWaveState : EnemyStateBase
{
    private ThreePhaseBoss Boss;
    private Coroutine waveCoroutine;

    // Swinging the blade's wind-up time
    private float swingDelay = 0.25f;

    public BossWaveState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
        Boss = enemy as ThreePhaseBoss;
    }

    public override void Enter()
    {
        if (Boss == null || Boss.IsDie) return;

        Boss.SetVelocityX(0);

        waveCoroutine = Boss.StartCoroutine(WaveSequence());
    }

    IEnumerator WaveSequence()
    {
        // --- wave 1 ---
        TriggerAnimation(); 
        yield return new WaitForSeconds(swingDelay);
        ExecuteAttackLogic(1f); // 产生伤害和剑气

        // Waiting for the next cut (Total interval: 0.8s)
        yield return new WaitForSeconds(0.8f - swingDelay);

        // --- wave 2 ---
        TriggerAnimation();
        yield return new WaitForSeconds(swingDelay);
        ExecuteAttackLogic(1f);

        yield return new WaitForSeconds(0.8f - swingDelay);

        // --- wave 3 ---
        TriggerAnimation();
        yield return new WaitForSeconds(swingDelay);
        ExecuteAttackLogic(2f);

        // Waiting for the move
        yield return new WaitForSeconds(0.6f - swingDelay);

        state.ChangeState(Boss.BossChaseState);
    }

    public override void Exit()
    {
        if (Boss != null && waveCoroutine != null)
        {
            Boss.StopCoroutine(waveCoroutine);
            waveCoroutine = null;
        }
    }

    public override void FrameUpdate()
    {
    }

    private void TriggerAnimation()
    {
        if (Boss != null && Boss.ani != null)
        {
            Boss.ani.Play(AniName, 0, 0f);
        }
    }

    private void ExecuteAttackLogic(float scaleMultiplier)
    {
        if (Boss == null || Boss.IsDie || Boss.gameObject == null) return;

        FireWaveLogic(scaleMultiplier);

        CheckMeleeHit(scaleMultiplier);
        
        if (AudioService.Instance != null)
            AudioService.Instance.PlayEffect("Boss_WaveAttack");
    }

    private void CheckMeleeHit(float scaleMultiplier)
    {
        if (Boss.BossAttackPos == null) return;

        float radius = Boss.Data.AttackRadius * (scaleMultiplier > 1f ? 1.5f : 1f);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(Boss.BossAttackPos.position, radius, LayerMask.GetMask("Player"));

        foreach (var target in colliders)
        {
            target.GetComponent<IHurt>()?.Hurt(Boss.transform, Boss.CurrentAtk);
        }
    }

    private void FireWaveLogic(float scaleMultiplier)
    {
        CreateProjectile(scaleMultiplier, 0f);

        if (Boss.Phase == 3)
        {
            float subScale = scaleMultiplier * 0.8f;
            CreateProjectile(subScale, 0f);
            CreateProjectile(subScale, 20f);
            CreateProjectile(subScale, 60f);
            CreateProjectile(subScale, -25f);
            CreateProjectile(subScale, -65f);
        }
    }

    private void CreateProjectile(float scaleMultiplier, float angleOffset)
    {
        if (Boss.WavePrefab == null || Boss.BossAttackPos == null) return;

        Vector3 spawnPos = Boss.BossAttackPos.position;
        float yOffset = (scaleMultiplier - 1) * 1.5f;
        spawnPos.y += yOffset;

        GameObject go = GameObject.Instantiate(Boss.WavePrefab, spawnPos, Quaternion.identity);
        go.transform.localScale = go.transform.localScale * scaleMultiplier;

        BossWaveProjectile projectile = go.GetComponent<BossWaveProjectile>();
        if (projectile != null)
        {
            float finalSpeed = Boss.WaveSpeed;
            if (Boss.Phase == 3) finalSpeed *= 1.2f;
            projectile.Init(Boss.Dir, finalSpeed, Boss.CurrentAtk, angleOffset);
        }
    }
}