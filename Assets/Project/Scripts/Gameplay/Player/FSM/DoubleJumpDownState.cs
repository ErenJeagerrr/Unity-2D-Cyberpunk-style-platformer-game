using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Double jump down state - handles falling after double jump
/// </summary>
public class DoubleJumpDownState : StateBase
{
    public DoubleJumpDownState(Player player, StateMachine state, string AniName) : base(player, state, AniName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

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

        // Landing detection
        if (player.IsOnGround)
        {
            if (InputX == 0)
            {
                state.ChangeState(player.IdleState);
                return;
            }
            else
            {
                state.ChangeState(player.RunState);
                return;
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