using UnityEngine;
using System.Collections;

public class StormBossLaserState : EnemyStateBase
{
    private StormBoss stormBoss;
    private Coroutine laserCoroutine;

    private float chargeTime = 1.0f;
    private int laserCount = 6;
    private float spawnInterval = 0.2f;

    private float laserWarningTime = 0.8f;
    private float laserActiveTime = 0.6f;

    private float waveSpacing = 3.0f;
    private float waveStartOffset = 2.0f;

    public StormBossLaserState(Enemy enemy, EnemyStateMachine stateMachine, string AniName)
        : base(enemy, stateMachine, AniName)
    {
        stormBoss = enemy as StormBoss;
    }

    public override void Enter()
    {
        base.Enter();
        stormBoss.SetVelocityX(0);
        stormBoss.IsImmunity = true;

        laserCoroutine = stormBoss.StartCoroutine(LaserSequence());
    }

    public override void FrameUpdate()
    {
    }

    IEnumerator LaserSequence()
    {
        yield return new WaitForSeconds(chargeTime);

        stormBoss.ani.Play("Attack_StormBoss");

        if (Random.value < 0.5f)
        {
            yield return stormBoss.StartCoroutine(StandardLaserPattern());
        }
        else
        {
            yield return stormBoss.StartCoroutine(DualOutwardPattern());
        }

        yield return new WaitForSeconds(1.0f);

        state.ChangeState(stormBoss.StormChaseState);
    }

    IEnumerator StandardLaserPattern()
    {
        for (int i = 0; i < laserCount; i++)
        {
            float targetX = 0;
            if (stormBoss.player != null && Random.value < 0.8f)
            {
                float offsetX = Random.Range(-3f, 3f);
                targetX = stormBoss.player.position.x + offsetX;
            }
            else
            {
                float randomX = Random.Range(-10f, 10f);
                targetX = stormBoss.transform.position.x + randomX;
            }

            SpawnLaserAt(targetX);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator DualOutwardPattern()
    {
        for (int i = 0; i < 4; i++)
        {
            float currentOffset = waveStartOffset + (i * waveSpacing);
            float bossX = stormBoss.transform.position.x;

            SpawnLaserAt(bossX + currentOffset);
            SpawnLaserAt(bossX - currentOffset);

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnLaserAt(float targetX)
    {
        if (stormBoss.LaserPrefab == null) return;
        if (IsInsideWall(targetX)) return;

        if (AudioService.Instance != null)
        {
            AudioService.Instance.PlayEffect("Storm_Electricity");
        }

        float spawnY = GetGroundY(targetX);
        Vector3 spawnPos = new Vector3(targetX, spawnY, 0);

        GameObject go = GameObject.Instantiate(stormBoss.LaserPrefab, spawnPos, Quaternion.identity);

        StormBossLaser laser = go.GetComponent<StormBossLaser>();
        if (laser != null)
        {
            laser.Init(stormBoss.CurrentAtk, laserWarningTime, laserActiveTime);
        }
    }

    private bool IsInsideWall(float x)
    {
        Vector2 checkPos = new Vector2(x, stormBoss.transform.position.y + 1.0f);
        Collider2D hit = Physics2D.OverlapPoint(checkPos, LayerMask.GetMask("Ground"));
        return hit != null;
    }

    private float GetGroundY(float x)
    {
        float checkStartY = stormBoss.transform.position.y + 15f;
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(x, checkStartY), Vector2.down, 50f, LayerMask.GetMask("Ground"));

        if (hit.collider != null) return hit.point.y;
        if (stormBoss.player != null) return stormBoss.player.position.y;
        return stormBoss.transform.position.y;
    }

    public override void Exit()
    {
        base.Exit();

        if (AudioService.Instance != null)
        {
            AudioService.Instance.StopEffect();
        }

        if (laserCoroutine != null && stormBoss != null)
        {
            stormBoss.StopCoroutine(laserCoroutine);
            laserCoroutine = null;
        }

        if (stormBoss != null) stormBoss.IsImmunity = false;
    }
}