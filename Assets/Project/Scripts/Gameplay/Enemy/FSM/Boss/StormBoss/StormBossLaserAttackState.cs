using UnityEngine;
using System.Collections;

public class StormBossLaserAttackState : EnemyStateBase
{
    private StormBoss stormBoss;
    private Coroutine attackCoroutine;

    private float preCastTime = 1.0f;
    private float laserDuration = 2.0f;
    private float laserWidth = 0.4f;

    public StormBossLaserAttackState(Enemy enemy, EnemyStateMachine stateMachine, string AniName)
        : base(enemy, stateMachine, AniName)
    {
        stormBoss = enemy as StormBoss;
    }

    public override void Enter()
    {
        base.Enter();
        stormBoss.SetVelocityX(0);

        if (stormBoss.player != null)
        {
            if (stormBoss.player.position.x > stormBoss.transform.position.x) stormBoss.Flip(true);
            else stormBoss.Flip(false);
        }

        attackCoroutine = stormBoss.StartCoroutine(AttackSequence());
    }

    public override void FrameUpdate()
    {
    }

    IEnumerator AttackSequence()
    {
        // 1. 前摇阶段
        yield return new WaitForSeconds(preCastTime);

        // 2. 发射阶段（开始射出激光）
        if (AudioService.Instance != null)
        {
            AudioService.Instance.PlayEffect("Storm_Electricity");
        }

        FireLaser();

        // 3. 持续阶段
        yield return new WaitForSeconds(laserDuration);

        // 4. 收招阶段
        yield return new WaitForSeconds(0.5f);

        state.ChangeState(stormBoss.StormChaseState);
    }

    private void FireLaser()
    {
        if (stormBoss.WeaponLaserPrefab == null || stormBoss.WeaponPos == null)
        {
            Debug.LogError("Boss 缺少 WeaponLaserPrefab 或 WeaponPos 设置！");
            return;
        }

        GameObject go = GameObject.Instantiate(stormBoss.WeaponLaserPrefab, stormBoss.WeaponPos.position, Quaternion.identity);

        StormBossWeaponLaser laserScript = go.GetComponent<StormBossWeaponLaser>();
        if (laserScript != null)
        {
            laserScript.Init(
                stormBoss.WeaponPos,
                stormBoss.player,
                stormBoss.CurrentAtk,
                laserDuration,
                laserWidth
            );
        }
    }

    public override void Exit()
    {
        base.Exit();

        if (AudioService.Instance != null)
        {
            AudioService.Instance.StopEffect();
        }

        if (attackCoroutine != null && stormBoss != null)
        {
            stormBoss.StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }
}