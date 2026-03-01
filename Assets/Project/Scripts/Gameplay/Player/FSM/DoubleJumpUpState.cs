using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Double jump up state - handles second jump in air
/// </summary>
public class DoubleJumpUpState : StateBase
{
    public DoubleJumpUpState(Player player, StateMachine state, string AniName) : base(player, state, AniName)
    {
    }

    private float timer;

    public override void Enter()
    {
        base.Enter();
        timer = 0;

        // Reset vertical velocity and apply double jump force (use player's configurable value)
        player.SetVelocityY(0);
        player.rig.AddForce(player.transform.up * player.doubleJumpForce, ForceMode2D.Impulse);
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        timer += Time.deltaTime;

        // Transition to double jump falling state
        if (!player.IsOnGround && player.IsFalling)
        {
            state.ChangeState(player.DoubleJumpDownState);
            return;
        }

        // Land on ground (with timer to prevent instant landing)
        if (player.IsOnGround && timer >= 0.1f)
        {
            state.ChangeState(player.IdleState);
            return;
        }

        // Dash in air
        if (InputShift && player.IsCanDash)
        {
            state.ChangeState(player.DashState);
            return;
        }

        // Attack in air
        if (InputJ)
        {
            state.ChangeState(player.AttackState);
            return;
        }

        // Gun skill in air
        if (InputK)
        {
            if (player.CanUseGunSkill())
            {
                state.ChangeState(player.GunSkillState);
                return;
            }
            else
            {
                Debug.Log("Gun skill is on cooldown!");
            }
        }

        // Horizontal movement control in air
        if (InputX != 0)
        {
            player.SetVelocityX(PlayerSystem.Instance.MoveSpeed * InputX);

            if (InputX > 0)
                player.Flip(true);
            else if (InputX < 0)
                player.Flip(false);
        }
        else
        {
            player.SetVelocityX(0);
        }
    }
}