using UnityEngine;

public class SluggerBuck : MinorBase
{
    public Transform AttackPos;
    private int hurtCount = 0;
    private float counterAttackCD = 2.0f; 
    private float counterTimer = 0;

    protected override void InitState()
    {
        AniSuffix = "SluggerBuck";
        base.InitState();
    }

    public override void Update()
    {
        base.Update();
        if (counterTimer > 0) counterTimer -= Time.deltaTime;
    }

    public override bool CheckIsCanAttack()
    {
        if (AttackPos == null || AttackCDTimer > 0) return false;

        return Physics2D.OverlapCircle(AttackPos.position, Data.AttackRadius, LayerMask.GetMask("Player"));
    }

    protected override void HurtToBack(Transform pos)
    {
        if (IsAttack) return;

        hurtCount++;

        if (hurtCount >= 4 && counterTimer <= 0)
        {
            Flip(pos.position.x > transform.position.x);

            StartReact(() => {
                if (!IsDie && !IsHurtLocked)
                {
                    state.ChangeState(AttackState);
                    counterTimer = counterAttackCD;
                }
            });

            hurtCount = 0;
            return;
        }
        base.HurtToBack(pos);
    }

    public override void AniEvent()
    {
        Collider2D coll = Physics2D.OverlapCircle(AttackPos.position, Data.AttackRadius, LayerMask.GetMask("Player"));
        if (coll != null)
        {
            coll.GetComponent<IHurt>()?.Hurt(transform, Data.Attack);
        }
    }
}