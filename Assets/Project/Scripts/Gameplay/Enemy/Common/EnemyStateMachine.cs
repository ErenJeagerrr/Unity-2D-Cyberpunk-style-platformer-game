using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine
{
    public EnemyStateBase CurrentState;
    public void Init(EnemyStateBase state)
    {
        CurrentState = state;
        CurrentState.Enter();
    }
    public void ChangeState(EnemyStateBase NewState)
    {
        CurrentState.Exit();
        CurrentState = NewState;
        CurrentState.Enter();
    }
}
