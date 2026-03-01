using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseState : StateBase
{
    public PauseState(Player player, StateMachine state, string AniName) : base(player, state, AniName)
    {
    }
    public override void Enter()
    {
        player.SetVelocity(Vector2.zero);
    }
    public override void Exit()
    {
        player.SetVelocity(Vector2.zero);
    }
}
