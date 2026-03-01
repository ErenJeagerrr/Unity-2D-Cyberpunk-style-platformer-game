using UnityEngine;
using System.Collections;

public class StormBossThunderSlamState : EnemyStateBase
{
    private StormBoss stormBoss;
    private Coroutine slamCoroutine;

    private float riseForce = 18f;
    private float riseDuration = 0.4f;
    private float hoverTime = 0.5f;
    private float slamSpeed = 35f;
    private Vector2 impactSize = new Vector2(16f, 6f);
    private int damageMultiplier = 3;
    private bool hasImpacted = false;
    private float originalGravity;

    public StormBossThunderSlamState(Enemy enemy, EnemyStateMachine stateMachine, string AniName)
        : base(enemy, stateMachine, AniName)
    {
        stormBoss = enemy as StormBoss;
    }

    public override void Enter()
    {
        base.Enter();

        stormBoss.SetVelocityX(0);
        stormBoss.IsImmunity = true;
        hasImpacted = false;
        originalGravity = stormBoss.rig.gravityScale;

        if (AudioService.Instance != null)
            AudioService.Instance.PlayEffect("Storm_Jump");

        if (slamCoroutine != null)
        {
            stormBoss.StopCoroutine(slamCoroutine);
        }

        slamCoroutine = stormBoss.StartCoroutine(SlamSequence());
    }

    public override void AniEvent()
    {
        TriggerImpact();
    }

    IEnumerator SlamSequence()
    {
        // === 1. Rise ===
        stormBoss.rig.gravityScale = 0;
        stormBoss.SetVelocityY(riseForce);

        float riseTimer = 0;
        while (riseTimer < riseDuration)
        {
            riseTimer += Time.deltaTime;
            if (stormBoss.player != null)
            {
                float targetX = stormBoss.player.position.x;
                float moveDir = targetX > stormBoss.transform.position.x ? 1 : -1;
                stormBoss.SetVelocityX(moveDir * 4f);
            }
            yield return null;
        }

        // === 2. Hover ===
        stormBoss.rig.velocity = Vector2.zero;
        yield return new WaitForSeconds(hoverTime);

        // === 3. Slam ===
        stormBoss.rig.gravityScale = 5;
        stormBoss.rig.velocity = Vector2.down * slamSpeed;
        stormBoss.ani.Play("ThunderSlam_StormBoss");

        yield return new WaitForSeconds(0.05f);

        float safetyTimer = 0;
        while (!stormBoss.CheckGround() && safetyTimer < 3.0f)
        {
            safetyTimer += Time.deltaTime;
            yield return null;
        }

        // === 4. Impact ===
        TriggerImpact();

        // === 5. Recover ===
        stormBoss.SetVelocityX(0);
        stormBoss.rig.velocity = Vector2.zero;
        stormBoss.rig.gravityScale = originalGravity;

        yield return new WaitForSeconds(1.0f);

        stormBoss.IsImmunity = false;
        state.ChangeState(stormBoss.StormChaseState);
    }

    private void TriggerImpact()
    {
        if (hasImpacted) return;
        hasImpacted = true;

        if (AttackEffect.Instance != null) AttackEffect.Instance.Shake();

        if (AudioService.Instance != null)
            AudioService.Instance.PlayEffect("Storm_Slam");

        Collider2D[] hits = Physics2D.OverlapBoxAll(stormBoss.transform.position, impactSize, 0, LayerMask.GetMask("Player"));
        foreach (var hit in hits)
        {
            hit.GetComponent<IHurt>()?.Hurt(stormBoss.transform, stormBoss.CurrentAtk * damageMultiplier);
        }
        SpawnShockwaves();
    }

    private void SpawnShockwaves()
    {
        if (stormBoss.ShockwavePrefab == null) return;
        CreateShockwave(Vector3.left);
        CreateShockwave(Vector3.right);
    }

    private void CreateShockwave(Vector3 dir)
    {
        Vector3 spawnPos = stormBoss.transform.position;
        spawnPos.y = stormBoss.FrontPos != null ? stormBoss.FrontPos.position.y : spawnPos.y;
        spawnPos.y -= 0.1f;
        GameObject go = GameObject.Instantiate(stormBoss.ShockwavePrefab, spawnPos, Quaternion.identity);
        go.GetComponent<StormBossShockWave>()?.Init(12f, (int)stormBoss.CurrentAtk, dir);
    }

    // !!! FrameUpdate is not used in this state
    public override void FrameUpdate()
    {
    }

    public override void Exit()
    {
        base.Exit();

        if (slamCoroutine != null && stormBoss != null)
        {
            stormBoss.StopCoroutine(slamCoroutine);
            slamCoroutine = null;
        }

        if (stormBoss != null)
        {
            stormBoss.rig.gravityScale = 5;
            stormBoss.IsImmunity = false;
        }
    }
}