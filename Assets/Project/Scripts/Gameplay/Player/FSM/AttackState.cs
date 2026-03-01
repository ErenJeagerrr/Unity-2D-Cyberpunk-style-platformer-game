using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : StateBase
{
    public AttackState(Player player, StateMachine state, string AniName) : base(player, state, AniName)
    {
    }
    private int Compat = 1;
    public override void Enter()
    {
        base.Enter();
        player.SetVelocity(Vector2.zero);
        player.ani.SetFloat("Compat", Compat);
        AudioService.Instance.PlayEffect("Attack");
        Compat++;
        if (Compat > 3)
        {
            Compat = 1;
        }
        TimerSystem.Instance.RemoveTask("AttackTask");
        TimerSystem.Instance.AddTask(1.5f, () =>
        {
            Compat = 1;
        }, "AttackTask");
    }
    public override void FrameUpdate()
    {
        base.FrameUpdate();
        if (InputShift && player.IsCanDash)
        {
            state.ChangeState(player.DashState);
            return;
        }
        if (IsComplete)
        {
            state.ChangeState(player.IdleState);
            return;
        }
        else
        {
            player.SetVelocity(Vector2.zero);
        }
    }
    public override void AniEvent()
    {
        base.AniEvent();
        Vector3 AttackPos = new Vector3(player.Dir * player.AttackDatas[Compat - 1].AttackPos.x, player.AttackDatas[Compat - 1].AttackPos.y);
        Collider2D[] colliders = Physics2D.OverlapBoxAll(player.transform.position + AttackPos, player.AttackDatas[Compat - 1].AttackRaduis, 0, LayerMask.GetMask("Enemy"));
        if (colliders.Length != 0)
        {
            AttackEffect.Instance.Shake();
            AttackEffect.Instance.StopFrame(3);
            AudioService.Instance.PlayEffect("Hurt");
            foreach (var i in colliders)
            {
                i.GetComponent<IHurt>()?.Hurt(player.transform, PlayerSystem.Instance.Attack);
            }
        }
    }
}
