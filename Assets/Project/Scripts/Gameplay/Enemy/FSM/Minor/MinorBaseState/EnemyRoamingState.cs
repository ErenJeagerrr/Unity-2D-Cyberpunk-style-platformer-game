using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRoamingState : EnemyStateBase
{
    private List<Vector3> RoamingPoints;
    private Vector3 CurrentTargetPos;
    private int TargetIndex;

    public EnemyRoamingState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName) { }

    public override void Enter()
    {
        base.Enter();
        if (RoamingPoints == null) CreateNewPoints();
        CurrentTargetPos = RoamingPoints[TargetIndex];
    }

    private void CreateNewPoints()
    {
        RoamingPoints = new List<Vector3>();
        float dist = Random.Range(2, enemy.Data.PatrolDistance + 1);
        RoamingPoints.Add(enemy.Position + enemy.transform.right * dist);
        RoamingPoints.Add(enemy.Position - enemy.transform.right * dist);
        TargetIndex = Random.Range(0, RoamingPoints.Count);
        CurrentTargetPos = RoamingPoints[TargetIndex];
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        if (enemy.CheckPlayer()) { state.ChangeState(enemy.ChaseState); return; }

        if (CurrentTargetPos.x > enemy.Position.x) { enemy.Flip(true); enemy.SetVelocityX(enemy.Data.MoveSpeed); }
        else { enemy.Flip(false); enemy.SetVelocityX(-enemy.Data.MoveSpeed); }

        bool isArrived = Mathf.Abs(enemy.Position.x - CurrentTargetPos.x) <= 0.5f;

        bool isBlocked = enemy.CheckWall();
        bool isCliff = !enemy.CheckGroundAhead();

        if (isArrived || isBlocked || isCliff)
        {
            TargetIndex = (TargetIndex + 1) % RoamingPoints.Count;
            state.ChangeState(enemy.IdleState);
        }
    }
}