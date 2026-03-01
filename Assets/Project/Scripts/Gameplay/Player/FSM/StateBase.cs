using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateBase
{
    protected Player player;
    protected StateMachine state;
    protected string AniName;
    protected bool IsComplete;

    #region 输入
    protected int InputX => (int)Input.GetAxisRaw("Horizontal");
    protected bool InputSpace => Input.GetKeyDown(KeyCode.Space);
    protected bool InputJ => Input.GetKeyDown(KeyCode.J);
    protected bool InputK => Input.GetKeyDown(KeyCode.K);
    protected bool InputShift => Input.GetKeyDown(KeyCode.LeftShift);
    protected bool InputB => Input.GetKeyDown(KeyCode.B);
    protected bool InputS => Input.GetKeyDown(KeyCode.S);
    #endregion

    public StateBase(Player player, StateMachine state, string AniName)
    {
        this.player = player;
        this.state = state;
        this.AniName = AniName;
    }
    public virtual void Enter()
    {
        player.PlayAni("Default");
        player.PlayAni(AniName);
        IsComplete = false;
    }
    public virtual void FrameUpdate()
    {
        AnimatorStateInfo info = player.ani.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName(AniName))
        {
            player.PlayAni(AniName);
            return;
        }
        if (info.normalizedTime >= 1)
        {
            IsComplete = true;
        }
        PlayerInput();
    }
    public virtual void FixUpdate() { }
    public virtual void Exit() { }
    public virtual void AniEvent() { }
    private void PlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !GameRoot.Instance.IsPause && !(this is DieState))
        {
            GameRoot.Instance.Pause();
        }
    }
}
