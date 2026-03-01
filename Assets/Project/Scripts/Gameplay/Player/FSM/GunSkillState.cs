using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSkillState : StateBase
{
    private bool hasFired = false;

    public GunSkillState(Player player, StateMachine state, string AniName) : base(player, state, AniName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        if (!player.CanUseGunSkill())
        {
            state.ChangeState(player.IdleState);
            return;
        }
        hasFired = false;

        player.UseGunSkill();

        AudioService.Instance.PlayEffect("Gun");
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        if (InputShift && player.IsCanDash)
        {
            state.ChangeState(player.DashState);
            return;
        }
        if (IsComplete)
        {
            state.ChangeState(player.IdleState);
            return;
        }
        else
        {
            // Lock player in place during gun skill
            player.SetVelocity(Vector2.zero);
        }
    }

    public override void AniEvent()
    {
        base.AniEvent();

        if (hasFired)
            return;

        hasFired = true;
        FireBullet();
    }

    private void FireBullet()
    {
        if (player.bulletPrefab == null)
        {
            Debug.LogWarning("Bullet prefab is not assigned! Please drag a prefab into Player's bulletPrefab field.");
            return;
        }

        if (player.firePoint == null)
        {
            Debug.LogWarning("Fire point is not assigned! Please create a FirePoint child object under Player.");
            return;
        }

        GameObject bullet = GameObject.Instantiate(
            player.bulletPrefab,
            player.firePoint.position,
            Quaternion.identity
        );

        Vector2 direction = new Vector2(player.Dir, 0);

        PlayerBullet playerBullet = bullet.GetComponent<PlayerBullet>();
        if (playerBullet != null)
        {
            // Use the damage multiplier from Player's inspector settings
            int damage = PlayerSystem.Instance.Attack * player.gunSkillDamageMultiplier;
            playerBullet.Init(direction, damage, player.bulletSpeed, player.bulletLifetime);
        }

        AudioService.Instance.PlayEffect("Hurt");
    }
}