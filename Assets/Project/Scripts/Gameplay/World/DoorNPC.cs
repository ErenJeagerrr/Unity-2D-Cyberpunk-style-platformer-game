using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorNPC : TriggerBase
{
    public List<DialogueData_SO> Dialogues;

    public override void TriggerEvent()
    {
        base.TriggerEvent();

        int index = LevelSystem.Instance.CurrentLevel - 1;

        if (Dialogues == null || Dialogues.Count == 0)
        {
            Debug.LogError("【错误】DoorNPC 上的 Dialogues 列表是空的！请在 Inspector 面板中赋值。");
            return;
        }

        if (index < 0 || index >= Dialogues.Count)
        {
            Debug.LogWarning($"【警告】找不到对应关卡 {LevelSystem.Instance.CurrentLevel} 的对话。将默认播放最后一个对话。");
            index = Dialogues.Count - 1; 
        }

        DialogueSystem.Instance.StartDialogue(Dialogues[index], () =>
        {
            LevelSystem.Instance.OpenDoor(DoorType.Level);
        });
    }
}