using UnityEngine;

public class TurretGuard : MinorBase
{
    public Transform BowPos;
    public Transform ArrowPos;
    public GameObject ArrowPrefab;

    protected override void InitState()
    {
        AniSuffix = "TurretGuard";
        base.InitState();

        ChaseState = new TurrentGuardChaseState(this, state, "Run_" + AniSuffix);
        AttackState = new TurrentGuardAttackState(this, state, "Attack_" + AniSuffix);
    }

    public override bool CheckIsCanAttack()
    {

        if (BowPos == null || AttackCDTimer > 0) return false;

        return Physics2D.OverlapBox(BowPos.position, Data.BowRadius, 0, LayerMask.GetMask("Player"));
    }
}