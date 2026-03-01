using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerSystem : SingletonBase<TimerSystem>
{
    private List<TimerTask> TaskList;
    public override void Init()
    {
        base.Init();
        TaskList = new List<TimerTask>();
    }
    private void Update()
    {
        for (int i = 0; i < TaskList.Count; i++)
        {
            if (Time.realtimeSinceStartup >= TaskList[i].destTime)
            {

                TaskList[i].action?.Invoke();
                TaskList.RemoveAt(i);
                i--;
            }
        }
    }

    public void AddTask(float delayTime, Action action, string TaskName = "")
    {
        float destTime = Time.realtimeSinceStartup + delayTime;
        TimerTask task = new TimerTask();
        task.TaskName = TaskName;
        task.destTime = destTime;
        task.action = action;
        TaskList.Add(task);
    }
    public void RemoveTask(string TaskName)
    {
        for (int i = 0; i < TaskList.Count; i++)
        {
            if (TaskName == TaskList[i].TaskName)
            {
                if (Time.realtimeSinceStartup < TaskList[i].destTime)
                {
                    TaskList.RemoveAt(i);
                    break;
                }
                else
                {
                    Debug.LogError("移除任务错误");
                }
            }
        }
    }
}

public class TimerTask
{
    public string TaskName;
    public float destTime;
    public Action action;
}
