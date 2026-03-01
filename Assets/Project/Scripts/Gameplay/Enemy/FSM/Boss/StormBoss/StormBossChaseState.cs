using UnityEngine;

public class StormBossChaseState : BossChaseState
{
    private StormBoss stormBoss;

    private float stopDistance = 0.8f;
    private float turnThreshold = 0.5f;

    public StormBossChaseState(Enemy enemy, EnemyStateMachine stateMachine, string AniName)
        : base(enemy, stateMachine, AniName)
    {
        stormBoss = enemy as StormBoss;
    }

    public override void FrameUpdate()
    {
        AnimatorStateInfo info = stormBoss.ani.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName(AniName)) stormBoss.ani.Play(AniName);

        if (stormBoss.player == null) return;

        float dist = Vector2.Distance(stormBoss.player.position, stormBoss.transform.position);


        float hpPercent = stormBoss.CurrentHeathl / stormBoss.MaxHeathl;
        if (hpPercent < stormBoss.HealThreshold && stormBoss.TryUseSkill("Heal"))
        {
            state.ChangeState(stormBoss.StormHealState);
            return;
        }

        if (stormBoss.TryUseSkill("TeleportSlam"))
        {
            state.ChangeState(stormBoss.StormTeleportSlamState);
            return;
        }

        if (stormBoss.TryUseSkill("WeaponLaser"))
        {
            state.ChangeState(stormBoss.StormWeaponLaserState);
            return;
        }

        if (stormBoss.TryUseSkill("Laser"))
        {
            state.ChangeState(stormBoss.StormLaserState);
            return;
        }

        if (stormBoss.TryUseSkill("ThunderSlam"))
        {
            state.ChangeState(stormBoss.StormThunderSlamState);
            return;
        }

        if (stormBoss.CheckIsCanAttack())
        {
            state.ChangeState(stormBoss.StormAttackState);
            return;
        }

        MoveTowardsPlayerSmart();
    }

    private void MoveTowardsPlayerSmart()
    {
        float xDiff = stormBoss.player.position.x - stormBoss.transform.position.x;
        float absDiff = Mathf.Abs(xDiff);

        if (absDiff > stopDistance)
        {
            if (xDiff > turnThreshold && !stormBoss.IsFaceRight)
            {
                stormBoss.Flip(true);
            }
            else if (xDiff < -turnThreshold && stormBoss.IsFaceRight)
            {
                stormBoss.Flip(false);
            }

            if (xDiff > 0) stormBoss.SetVelocityX(stormBoss.CurrentChaseSpeed);
            else stormBoss.SetVelocityX(-stormBoss.CurrentChaseSpeed);
        }
        else
        {
            stormBoss.SetVelocityX(0);
        }
    }
}