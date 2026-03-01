using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinPanel : BasePanel
{
    public Button BackMenu;
    public Button NextLevel;

    public override void Show()
    {
        base.Show();
        PlayerSystem.Instance.Pause();

        BackMenu.onClick.AddListener(() =>
        {
            HideMe();
            ResService.Instance.LoadScene("Start", () =>
            {
                UIService.Instance.HideAllPanel();
                UIService.Instance.ShowPanel<StartPanel>();
            });
        });

        
         if (LevelSystem.Instance.CurrentLevel >= 6) NextLevel.GetComponentInChildren<Text>().text = "Finish";

        NextLevel.onClick.RemoveAllListeners(); // 防止重复绑定
        NextLevel.onClick.AddListener(() =>
        {
            
            if (LevelSystem.Instance.CurrentLevel >= 6)
            {
                
                LevelSystem.Instance.CurrentLevel++; 
                DataSystem.Instance.Save();

              
                HideMe();
                ResService.Instance.LoadScene("Start", () =>
                {
                    UIService.Instance.HideAllPanel();
                    UIService.Instance.ShowPanel<StartPanel>();
                    
                    // 可选：弹窗提示通关
                    UIService.Instance.ShowPanel<FloatPanel>(3).Init("Congratulations! You beat the game!", null);
                });
            }
            else
            {
                // 正常进入下一关流程
                LevelSystem.Instance.CurrentLevel++;
                DataSystem.Instance.Save();
                LevelSystem.Instance.LoadTown();
                HideMe();
            }
        });
    }

    public override void Hide()
    {
        base.Hide();
        PlayerSystem.Instance.Continue();
    }
}