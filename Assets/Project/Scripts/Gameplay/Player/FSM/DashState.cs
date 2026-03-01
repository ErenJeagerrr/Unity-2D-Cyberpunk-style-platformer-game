using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashState : StateBase
{
    public DashState(Player player, StateMachine state, string AniName) : base(player, state, AniName)
    {
    }
    public override void Enter()
    {
        base.Enter();
        player.IsDash = true;
        player.IsCanDash = false;
        TimerSystem.Instance.AddTask(0.3f, () => { player.IsCanDash = true; });
        player.rig.gravityScale = 0;
        player.SetVelocity(Vector2.zero);
        player.SetVelocityX(20 * player.Dir);
    }
    public override void Exit()
    {
        base.Exit();
        player.IsDash = false;
        player.rig.gravityScale = 5;
        player.SetVelocityX(0);
    }
    public override void FrameUpdate()
    {
        base.FrameUpdate();

        // Check for dash jump input - allow jumping during dash
        if (InputSpace)
        {
            // Check if dash jump limit is reached
            if (player.CurrentDashJumpCount >= player.MaxDashJumpCount)
            {
                Debug.Log($"<color=yellow>Dash jump limit reached! ({player.CurrentDashJumpCount}/{player.MaxDashJumpCount})</color>");
                return;
            }

            player.CurrentDashJumpCount++;
            state.ChangeState(player.DashJumpState);
            return;
        }

        if (IsComplete)
        {
            if (player.IsOnGround)
            {
                state.ChangeState(player.IdleState);
            }
            else
            {
                state.ChangeState(player.JumpDownState);
            }
        }
    }
}