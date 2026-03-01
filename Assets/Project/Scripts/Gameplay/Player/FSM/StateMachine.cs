using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public StateBase CurrentState;
    public void Init(StateBase state)
    {
        CurrentState = state;
        CurrentState.Enter();
    }
    public void ChangeState(StateBase newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}
