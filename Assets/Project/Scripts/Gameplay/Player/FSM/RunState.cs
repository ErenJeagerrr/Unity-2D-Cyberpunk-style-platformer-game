using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunState : StateBase
{
    public RunState(Player player, StateMachine state, string AniName) : base(player, state, AniName)
    {
    }
    public override void Exit()
    {
        base.Exit();
        player.SetVelocityX(0);
    }
    public override void FrameUpdate()
    {
        base.FrameUpdate();
        if (Input.GetKeyDown(KeyCode.O))
        {
            PlayerSystem.Instance.UseRecover();
        }
        if (InputSpace && player.IsOnGround)
        {
            player.CurrentJumpCount = 1;
            state.ChangeState(player.JumpUpState);
            return;
        }
        if (!player.IsOnGround && player.IsFalling)
        {
            state.ChangeState(player.JumpDownState);
            return;
        }
        if (InputJ)
        {
            state.ChangeState(player.AttackState);
            return;
        }
        if (InputK)
        {
            state.ChangeState(player.GunSkillState);
            return;
        }
        if (Input.GetKeyDown(KeyCode.I) && player.CanUseBurnSkill())
        {
            state.ChangeState(player.BurnSkillState);
            return;
        }
        if (InputShift && player.IsCanDash)
        {
            state.ChangeState(player.DashState);
            return;
        }
        //站立
        if (InputX == 0)
        {
            state.ChangeState(player.IdleState);
            return;
        }
        player.SetVelocityX(PlayerSystem.Instance.MoveSpeed * InputX);
        if (InputX >= 0)
            player.Flip(true);
        else
            player.Flip(false);
    }
}
