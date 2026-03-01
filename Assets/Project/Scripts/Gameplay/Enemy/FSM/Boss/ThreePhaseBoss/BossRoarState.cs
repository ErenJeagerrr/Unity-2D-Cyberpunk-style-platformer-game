using UnityEngine;

public class BossRoarState : EnemyStateBase
{
    private ThreePhaseBoss phaseBoss;
    private bool hasTriggeredEffect = false;
    private int RoarHurt = 5;

    public BossRoarState(Enemy enemy, EnemyStateMachine stateMachine, string aniName)
        : base(enemy, stateMachine, aniName)
    {
        phaseBoss = enemy as ThreePhaseBoss;
    }

    public override void Enter()
    {
        base.Enter();

        phaseBoss.SetVelocityX(0);
        phaseBoss.IsImmunity = true;
        hasTriggeredEffect = false;


        if (AudioService.Instance != null)
        {
            AudioService.Instance.PlayEffect("Boss_Roar");
        }
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        AnimatorStateInfo info = phaseBoss.ani.GetCurrentAnimatorStateInfo(0);

        if (info.IsName(AniName) && info.normalizedTime >= 0.3f && !hasTriggeredEffect)
        {
            ApplyRoarImpact();
            hasTriggeredEffect = true;
        }

        if (AniComplete)
        {
            state.ChangeState(phaseBoss.BossChaseState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        if (phaseBoss != null)
        {
            phaseBoss.IsImmunity = false;
        }
    }

    private void ApplyRoarImpact()
    {
        if (AttackEffect.Instance != null)
        {
            AttackEffect.Instance.Shake();
        }

        float roarRadius = 6.0f;    
        float knockbackForce = 18f; 

        Collider2D[] players = Physics2D.OverlapCircleAll(phaseBoss.transform.position, roarRadius, LayerMask.GetMask("Player"));

        foreach (var col in players)
        {
            Rigidbody2D playerRig = col.GetComponent<Rigidbody2D>();
            IHurt playerHurt = col.GetComponent<IHurt>();

            if (playerRig != null)
            {
                Vector2 pushDir = (col.transform.position - phaseBoss.transform.position).normalized;

                pushDir += Vector2.up * 0.4f;

                playerRig.velocity = Vector2.zero;
                playerRig.AddForce(pushDir * knockbackForce, ForceMode2D.Impulse);

                if (playerHurt != null)
                {
                    playerHurt.Hurt(phaseBoss.transform, RoarHurt);
                }
            }
        }
    }
}