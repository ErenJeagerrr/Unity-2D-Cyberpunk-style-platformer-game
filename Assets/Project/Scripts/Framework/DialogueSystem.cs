using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSystem : SingletonBase<DialogueSystem>
{
    private Action Callback;
    public override void Init()
    {
        base.Init();
    }
    public void StartDialogue(DialogueData_SO Data, Action Callback)
    {
        this.Callback = Callback;
        PlayerSystem.Instance.Pause();
        UIService.Instance.ShowPanel<DialoguePanel>(3).Init(Data);
    }
    public void EndDialogue()
    {
        PlayerSystem.Instance.Continue();
        UIService.Instance.HidePanel<DialoguePanel>();
        Callback?.Invoke();
        Callback = null;
    }
}
