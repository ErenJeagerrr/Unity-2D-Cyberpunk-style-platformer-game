using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoot : MonoBehaviour
{
    public static GameRoot Instance;
    public bool IsPause;
    private void Awake()
    {
        if (GameRoot.Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (GetComponent<DeveloperMode>() == null)
        {
            gameObject.AddComponent<DeveloperMode>();
        }
        Init();
        IsPause = false;
       DataSystem.Instance.Load(); 
        UIService.Instance.ShowPanel<StartPanel>();
    }

    public void Init()
    {
        GetComponent<ResService>().Init();
        GetComponent<UIService>().Init();
        GetComponent<AudioService>().Init();
        GetComponent<TimerSystem>().Init();
        GetComponent<DataSystem>().Init();
        GetComponent<LevelSystem>().Init();
        GetComponent<DialogueSystem>().Init();
        GetComponent<PlayerSystem>().Init();
        GetComponent<DeveloperMode>().Init();
    }
    public void Pause()
    {
        Time.timeScale = 0;
        GameRoot.Instance.IsPause = true;
        UIService.Instance.ShowPanel<PausePanel>(3);
    }
    public void Continue()
    {
        Time.timeScale = 1;
        GameRoot.Instance.IsPause = false;
        UIService.Instance.HidePanel<PausePanel>();
    }
}
