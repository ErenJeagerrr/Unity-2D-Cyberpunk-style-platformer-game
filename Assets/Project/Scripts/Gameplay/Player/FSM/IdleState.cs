using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : StateBase
{
    public IdleState(Player player, StateMachine state, string AniName) : base(player, state, AniName)
    {
    }
    public override void Enter()
    {
        base.Enter();
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
        if (InputX != 0)
        {
            state.ChangeState(player.RunState);
            return;
        }
    }
}
