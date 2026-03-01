using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateBase
{
    public Enemy enemy;
    public EnemyStateMachine state;
    public string AniName;            
    protected bool AniComplete;       
    public EnemyStateBase(Enemy enemy, EnemyStateMachine stateMachine, string AniName)
    {
        this.enemy = enemy;
        this.state = stateMachine;
        this.AniName = AniName;
    }
    public virtual void Enter()
    {
        enemy.ani.Play(AniName);
        AniComplete = false;
    }
    public virtual void Exit() { }
    public virtual void FrameUpdate()
    {
        AnimatorStateInfo info = enemy.ani.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName(AniName))
        {
            enemy.ani.Play(AniName);
            return;
        }
        if (info.normalizedTime >= 1f)
        {
            AniComplete = true;
        }
    }
    public virtual void FixUpdate() { }

    public virtual void AniEvent() { }
}

