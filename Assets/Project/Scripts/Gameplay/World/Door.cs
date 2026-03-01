using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorType
{
    Level,
    Town,
    Win,
}
public class Door : TriggerBase
{
    private DoorType doorType;
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Init(DoorType doorType)
    {
        this.doorType = doorType;
    }
    public override void TriggerEvent()
    {
        base.TriggerEvent();
        switch (doorType)
        {
            case DoorType.Level:
                LevelSystem.Instance.LoadCurrentLevel();
                break;
            case DoorType.Town:
                LevelSystem.Instance.LoadTown();
                break;
            case DoorType.Win:
                UIService.Instance.ShowPanel<WinPanel>(2);
                break;
        }
    }
}
