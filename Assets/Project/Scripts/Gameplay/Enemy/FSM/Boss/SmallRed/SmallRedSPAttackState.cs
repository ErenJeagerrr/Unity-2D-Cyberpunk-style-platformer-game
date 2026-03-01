using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallRedSPAttackState : EnemyStateBase
{
    private SmallRed smallRed;
    private int SPContinueTime = 2; // SP攻击持续时间
    private float timer;

    public SmallRedSPAttackState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
        smallRed = enemy as SmallRed;
    }

    public override void Enter()
    {
        base.Enter();
        timer = 0;

        if (smallRed.player != null)
        {
            if (smallRed.player.position.x > smallRed.transform.position.x)
                smallRed.Flip(true);
            else
                smallRed.Flip(false);
        }

        if (AudioService.Instance != null)
        {
            AudioService.Instance.PlayEffect("SmallRed_Spin", 1f, 2.5f);
        }
    }

    public override void Exit()
    {
        base.Exit();
        smallRed.SetVelocityX(0);
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        // 持续向当前朝向移动
        smallRed.SetVelocityX(smallRed.Dir * 5);

        timer += Time.deltaTime;
        if (timer >= SPContinueTime)
        {
            state.ChangeState(smallRed.BossIdleState);
            return;
        }
    }

    public override void AniEvent()
    {
        if (smallRed.BossSPAttackPos == null) return;

        Collider2D coll = Physics2D.OverlapBox(smallRed.BossSPAttackPos.position, smallRed.BossSPAttackRadius, 0, LayerMask.GetMask("Player"));

        if (coll != null)
        {
            coll.GetComponent<IHurt>()?.Hurt(enemy.transform, enemy.Data.Attack);
        }
    }
}