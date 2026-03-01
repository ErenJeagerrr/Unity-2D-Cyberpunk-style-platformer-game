using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dash Jump State - Special jump during dash
/// Maintains high horizontal speed while adding vertical jump force
/// </summary>
public class DashJumpState : StateBase
{
    private float timer;

    public DashJumpState(Player player, StateMachine state, string AniName) : base(player, state, AniName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        timer = 0;

        // Reset vertical velocity and add upward force (use player's configurable value)
        player.SetVelocityY(0);
        player.rig.AddForce(player.transform.up * player.dashJumpVerticalForce, ForceMode2D.Impulse);

        // Maintain high horizontal speed from dash (use player's configurable value)
        player.SetVelocityX((int)(player.dashJumpHorizontalSpeed * player.Dir));

        // Reset gravity to normal (in case it was modified during dash)
        player.rig.gravityScale = 5;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        timer += Time.deltaTime;

        // Maintain horizontal momentum during dash jump
        player.SetVelocityX((int)(player.dashJumpHorizontalSpeed * player.Dir));

        // Transition to falling state when starting to fall
        if (!player.IsOnGround && player.IsFalling)
        {
            state.ChangeState(player.JumpDownState);
            return;
        }

        // Land on ground (with timer to prevent instant landing)
        if (player.IsOnGround && timer >= 0.1f)
        {
            state.ChangeState(player.IdleState);
            return;
        }

        // Can attack in air during dash jump
        if (InputJ)
        {
            state.ChangeState(player.AttackState);
            return;
        }

        // Can use gun skill in air
        if (InputK && player.CanUseGunSkill())
        {
            state.ChangeState(player.GunSkillState);
            return;
        }

        // Can dash again in air if available
        if (InputShift && player.IsCanDash)
        {
            state.ChangeState(player.DashState);
            return;
        }

        // Allow slight direction control (but maintain most momentum)
        if (InputX != 0)
        {
            // Flip character based on input
            if (InputX > 0)
                player.Flip(true);
            else
                player.Flip(false);
        }
    }

    public override void FixUpdate()
    {
        base.FixUpdate();
    }
}