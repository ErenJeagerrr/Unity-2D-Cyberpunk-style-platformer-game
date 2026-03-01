using UnityEngine;

public class SkateZ : MinorBase
{
    [Header("SkateZ Specific Properties")]
    public float AttackRadius = 1.2f;

    protected override void InitState()
    {
        AniSuffix = "SkateZ";

        base.InitState();

        AttackState = null;

        state.Init(IdleState);
    }

    private float attackCoolDown = 2.0f;
    private float simpleTimer = 0;

    public override void Update()
    {
        if (IsDie || IsHurtLocked) return;
        base.Update();

        if (simpleTimer > 0) simpleTimer -= Time.deltaTime;

        Collider2D collider = Physics2D.OverlapCircle(transform.position, AttackRadius, LayerMask.GetMask("Player"));
        if (collider != null)
        {
            if (simpleTimer <= 0 && AudioService.Instance != null)
            {
                AudioService.Instance.PlayEffect("SkateZ_Bang01");
                simpleTimer = attackCoolDown;
                collider.GetComponent<IHurt>()?.Hurt(transform, Data.Attack);
            }
        }
    }

    public override bool CheckIsCanAttack() => false;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRadius);

        if (Data != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, Data.CheckRadius);
        }
    }
}