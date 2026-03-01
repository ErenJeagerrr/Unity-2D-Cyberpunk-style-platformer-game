using UnityEngine;
using System.Collections.Generic;

public class BossDashState : EnemyStateBase
{
    private ThreePhaseBoss Boss;

    private float preDashTime = 0.5f;                
    private float dashDuration = 0.25f;              
    private float speedMultiplier = 3.5f;           

    private float timer;
    private bool isDashing;
    private float ghostTimer;

    private List<Collider2D> hitTargets = new List<Collider2D>();

    public BossDashState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
        Boss = enemy as ThreePhaseBoss;
    }

    public override void Enter()
    {
        base.Enter();
        timer = 0;
        isDashing = false;
        ghostTimer = 0;
        hitTargets.Clear();

        Boss.SetVelocityX(0);

        if (Boss.EnemySpriteRenderer() != null)
        {
            Boss.EnemySpriteRenderer().color = Color.red;
        }

        if (Boss.player != null)
        {
            if (Boss.player.position.x > Boss.transform.position.x) Boss.Flip(true);
            else Boss.Flip(false);
        }
    }

    public override void FrameUpdate()
    {
        timer += Time.deltaTime;

        // Phase One: Warning
        if (!isDashing)
        {
            Boss.ani.Play(AniName);
            Boss.ani.speed = 0;

            if (timer >= preDashTime)
            {
                StartDash();
            }
            return;
        }

        // Phase Two: dash
        float finalSpeed = Boss.DashSpeed * speedMultiplier;
        Boss.rig.velocity = new Vector2(Boss.Dir * finalSpeed, 0);

        ghostTimer += Time.deltaTime;
        if (ghostTimer > 0.015f)
        {
            Boss.CreateAfterImage();
            ghostTimer = 0;
        }

        Collider2D[] cols = Physics2D.OverlapBoxAll(Boss.transform.position, new Vector2(2f, 2f), 0, LayerMask.GetMask("Player"));

        foreach (var col in cols)
        {
            if (!hitTargets.Contains(col))
            {
                col.GetComponent<IHurt>()?.Hurt(Boss.transform, Boss.Data.Attack * 1.5f);
                hitTargets.Add(col);
                if (AttackEffect.Instance != null) AttackEffect.Instance.Shake();
            }
        }

        // Ending and Combo Judgment
        if (timer >= preDashTime + dashDuration)
        {
            bool triggerCombo = Boss.Phase >= 2 && Random.value < 0.4f;

            if (triggerCombo)
            {
                Boss.StartCombo("Fan", 0.1f);
            }
            
            else if (Random.value < 0.5f)
            {
                state.ChangeState(Boss.JumpState);
            }
            else
            {
                state.ChangeState(Boss.BossChaseState);
            }
        }
    }

    private void StartDash()
    {
        isDashing = true;
        if (Boss.EnemySpriteRenderer() != null)
        {
            if (Boss.Phase == 3) Boss.EnemySpriteRenderer().color = Color.red;
            else if (Boss.Phase == 2) Boss.EnemySpriteRenderer().color = Color.yellow;
            else Boss.EnemySpriteRenderer().color = Color.white;
        }
        if (AudioService.Instance != null)
        {
            AudioService.Instance.PlayEffect("Boss_Dash");
        }
        Boss.ani.speed = 4f;
    }

    public override void Exit()
    {
        base.Exit();
        Boss.SetVelocityX(0);
        Boss.ani.speed = 1f;

        if (Boss.EnemySpriteRenderer() != null)
        {
            if (Boss.Phase == 3) Boss.EnemySpriteRenderer().color = Color.red;
            else if (Boss.Phase == 2) Boss.EnemySpriteRenderer().color = Color.yellow;
            else Boss.EnemySpriteRenderer().color = Color.white;
        }
    }
}