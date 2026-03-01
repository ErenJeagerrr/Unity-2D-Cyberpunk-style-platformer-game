using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallRedSummonState : EnemyStateBase
{
    private SmallRed smallRed;

    // --- 召唤时间参数 ---
    private float timer;
    private bool hasSummoned = false;

    // 动作前摇与总持续时间
    private float summonTriggerTime = 0.5f; // 前摇
    private float summonDuration = 1.0f;    // 总时长

    public SmallRedSummonState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
        smallRed = enemy as SmallRed;
    }

    public override void Enter()
    {
        smallRed.ani.Play(AniName);

        smallRed.SetVelocityX(0);
        timer = 0;
        hasSummoned = false;

        if (smallRed.EnemySpriteRenderer() != null)
            smallRed.EnemySpriteRenderer().color = Color.magenta;

        if (AudioService.Instance != null)
        {
            AudioService.Instance.PlayEffect("SmallRed_Summon"); 
        }
    }

    public override void FrameUpdate()
    {
        AnimatorStateInfo info = smallRed.ani.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName(AniName)) smallRed.ani.Play(AniName);

        timer += Time.deltaTime;

        // --- 阶段 1: 触发召唤 ---
        if (timer >= summonTriggerTime && !hasSummoned)
        {
            hasSummoned = true;
            ExecuteSummon();

            if (AttackEffect.Instance != null) AttackEffect.Instance.Shake();
        }

        // --- 阶段 2: 结束状态 ---
        if (timer >= summonDuration)
        {
            state.ChangeState(smallRed.RedChaseState);
        }
    }

    public override void AniEvent()
    {
    }

    private void ExecuteSummon()
    {
        if (smallRed.MinionPrefabs == null || smallRed.MinionPrefabs.Length == 0) return;

        float hpPercent = smallRed.CurrentHeathl / smallRed.MaxHeathl;
        int summonCount = 1;

        if (hpPercent <= 0.33f)
        {
            summonCount = 3;
        }
        else if (hpPercent <= 0.66f)
        {
            summonCount = 2;
        }

        for (int i = 0; i < summonCount; i++)
        {
            SpawnMinion();
        }
    }

    private void SpawnMinion()
    {
        int index = Random.Range(0, smallRed.MinionPrefabs.Length);
        GameObject prefab = smallRed.MinionPrefabs[index];

        Vector3 startPos = smallRed.transform.position + new Vector3(0, 2f, 0);
        float randomX = Random.Range(-3f, 3f);

        Vector2 direction = randomX > 0 ? Vector2.right : Vector2.left;
        float checkDistance = Mathf.Abs(randomX);

        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, checkDistance, LayerMask.GetMask("Ground"));

        if (hit.collider != null)
        {
            float safeDistance = Mathf.Max(0, hit.distance - 0.5f);
            randomX = (randomX > 0 ? 1 : -1) * safeDistance;
        }

        Vector3 spawnPos = startPos + new Vector3(randomX, 0, 0);

        GameObject.Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    public override void Exit()
    {
        if (smallRed.EnemySpriteRenderer() != null)
            smallRed.EnemySpriteRenderer().color = Color.white;
    }
}