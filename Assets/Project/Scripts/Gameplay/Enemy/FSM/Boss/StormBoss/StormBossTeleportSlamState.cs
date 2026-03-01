using UnityEngine;
using System.Collections;

public class StormBossTeleportSlamState : EnemyStateBase
{
    private StormBoss stormBoss;
    private Coroutine skillCoroutine;

    private float teleportHeight = 6.0f;   
    private float vanishTime = 0.4f;       

    // hover
    private float baseHoverTime = 0.6f;     
    private float fastHoverTime = 0.25f;    

    // slam
    private float slamSpeed = 40f;         
    private Vector2 impactSize = new Vector2(16f, 6f); 
    private int damageMultiplier = 3;       

    public StormBossTeleportSlamState(Enemy enemy, EnemyStateMachine stateMachine, string AniName)
        : base(enemy, stateMachine, AniName)
    {
        stormBoss = enemy as StormBoss;
    }

    public override void Enter()
    {
        base.Enter();
        stormBoss.SetVelocityX(0);
        stormBoss.IsImmunity = true;

        skillCoroutine = stormBoss.StartCoroutine(SkillSequence());
    }

    public override void FrameUpdate()
    {
    }

    IEnumerator SkillSequence()
    {
        int totalSlams = (Random.value < 0.5f) ? 1 : 2;

        for (int i = 0; i < totalSlams; i++)
        {
            bool isSecondSlam = (i == 1);
            float currentHover = isSecondSlam ? fastHoverTime : baseHoverTime;

            // === 1: Vanish ===
            stormBoss.ani.Play("Teleport_StormBoss");

            float timer = 0;
            while (timer < vanishTime)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(1, 0, timer / vanishTime);
                SetAlpha(alpha);
                yield return null;
            }
            SetAlpha(0);

            // === 2. Teleport & Hover ===
            TeleportToPlayer();

            stormBoss.rig.velocity = Vector2.zero;
            stormBoss.rig.gravityScale = 0;

            SetAlpha(1);

            stormBoss.ani.Play("Jump_StormBoss");

            yield return new WaitForSeconds(currentHover);

            stormBoss.rig.gravityScale = 5;
            stormBoss.rig.velocity = Vector2.down * slamSpeed;

            stormBoss.ani.Play("ThunderSlam_StormBoss");

            float fallTimer = 0;

            while (!stormBoss.CheckGround() && fallTimer < 3.0f)
            {
                fallTimer += Time.deltaTime;

                if (fallTimer > 0.1f && Mathf.Abs(stormBoss.rig.velocity.y) < 0.1f)
                {
                    break;
                }

                yield return null;
            }

            LandingImpact();

            stormBoss.SetVelocityX(0);

            float recoverTime = (i == totalSlams - 1) ? 1.0f : 0.1f;
            yield return new WaitForSeconds(recoverTime);
        }

        stormBoss.IsImmunity = false;
        state.ChangeState(stormBoss.StormChaseState);
    }

    private void TeleportToPlayer()
    {
        if (stormBoss.player == null) return;

        float targetX = stormBoss.player.position.x;
        float groundY = GetGroundY(targetX);
        float targetY = teleportHeight;

        stormBoss.transform.position = new Vector3(targetX, targetY, 0);
    }

    private void LandingImpact()
    {
        if (AttackEffect.Instance != null) AttackEffect.Instance.Shake();
        if (AudioService.Instance != null) AudioService.Instance.PlayEffect("Storm_Slam");

        Collider2D[] hits = Physics2D.OverlapBoxAll(stormBoss.transform.position, impactSize, 0, LayerMask.GetMask("Player"));
        foreach (var hit in hits)
        {
            hit.GetComponent<IHurt>()?.Hurt(stormBoss.transform, stormBoss.CurrentAtk * damageMultiplier);
        }

        if (stormBoss.Data.HitEffect != null)
            GameObject.Instantiate(stormBoss.Data.HitEffect, stormBoss.transform.position, Quaternion.identity);
    }

    private void SetAlpha(float a)
    {
        if (stormBoss.EnemySpriteRenderer() != null)
        {
            Color c = stormBoss.EnemySpriteRenderer().color;
            c.a = a;
            stormBoss.EnemySpriteRenderer().color = c;
        }
    }

    private float GetGroundY(float x)
    {
        float checkStartY = stormBoss.player != null ? stormBoss.player.position.y + 5f : stormBoss.transform.position.y;
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(x, checkStartY), Vector2.down, 20f, LayerMask.GetMask("Ground"));
        if (hit.collider != null) return hit.point.y;
        if (stormBoss.player != null) return stormBoss.player.position.y;
        return stormBoss.transform.position.y;
    }

    public override void Exit()
    {
        base.Exit();
        if (skillCoroutine != null && stormBoss != null)
        {
            stormBoss.StopCoroutine(skillCoroutine);
            skillCoroutine = null;
        }

        if (stormBoss != null)
        {
            stormBoss.rig.gravityScale = 5;
            stormBoss.IsImmunity = false;
            SetAlpha(1);
        }
    }
}