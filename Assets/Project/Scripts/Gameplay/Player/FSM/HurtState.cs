using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtState : StateBase
{
    public HurtState(Player player, StateMachine state, string AniName) : base(player, state, AniName)
    {
    }
    public override void Enter()
    {
        base.Enter();
        player.SetVelocity(Vector2.zero);
    }
    public override void Exit()
    {
        base.Exit();
        player.SetVelocity(Vector2.zero);
    }
    public override void FrameUpdate()
    {
        base.FrameUpdate();
        if (IsComplete)
        {
            state.ChangeState(player.IdleState);
        }
    }
}
