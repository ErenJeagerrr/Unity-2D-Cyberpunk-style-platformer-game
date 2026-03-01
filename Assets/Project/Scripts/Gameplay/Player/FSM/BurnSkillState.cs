using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnSkillState : StateBase
{
    public BurnSkillState(Player player, StateMachine state, string AniName) : base(player, state, AniName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.UseBurnSkill();
        player.rig.velocity = Vector2.zero;
        player.PlayAni("Burn");
        AudioService.Instance.PlayEffect("BurnSkill");

        ActivateBurnArea();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        // Allow dash to cancel burn skill
        if (InputShift && player.IsCanDash)
        {
            state.ChangeState(player.DashState);
            return;
        }

        // Lock player in place during burn skill (like normal attack)
        if (!IsComplete)
        {
            player.SetVelocity(Vector2.zero);
        }

        if (IsComplete)
        {
            if (player.IsOnGround)
                state.ChangeState(player.IdleState);
            else
                state.ChangeState(player.JumpDownState);
        }
    }
    public override void AniEvent()
    {
        base.AniEvent();
        // This will be called by animation event
        AudioService.Instance.PlayEffect("Burn");
    }
    private void ActivateBurnArea()
    {
        if (player.burnArea == null)
        {
            Debug.LogError("<color=red>Burn area is NULL!</color>");
            return;
        }

        player.burnArea.Activate();
    }
}