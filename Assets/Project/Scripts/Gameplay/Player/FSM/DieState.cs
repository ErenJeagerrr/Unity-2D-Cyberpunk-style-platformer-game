using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieState : StateBase
{
    public DieState(Player player, StateMachine state, string AniName) : base(player, state, AniName)
    {
    }
    public override void Enter()
    {
        base.Enter();
        player.SetVelocity(Vector2.zero);
    }
}
