using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiePanel : BasePanel
{
    public Button OK;
    public override void Show()
    {
        base.Show();
        OK.onClick.AddListener(() =>
        {
            ResService.Instance.LoadScene("Start", () =>
            {
                UIService.Instance.HideAllPanel();
                DataSystem.Instance.Load();
                UIService.Instance.ShowPanel<StartPanel>();
            });
        });
    }
}
