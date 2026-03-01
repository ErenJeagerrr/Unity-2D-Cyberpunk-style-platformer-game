using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PausePanel : BasePanel
{
    public Button Continue;
    public Button Set;
    public Button BackMenu;
    public override void Show()
    {
        base.Show();
        GameRoot.Instance.Pause();
        Continue.onClick.AddListener(() =>
        {
            GameRoot.Instance.Continue();
            HideMe();
        });
        Set.onClick.AddListener(() =>
        {
            UIService.Instance.ShowPanel<SetPanel>(3);
        });
        BackMenu.onClick.AddListener(() =>
        {
            GameRoot.Instance.Continue();
            ResService.Instance.LoadScene("Start", () =>
            {
                UIService.Instance.HideAllPanel();
                UIService.Instance.ShowPanel<StartPanel>();
            });
        });
    }
}
