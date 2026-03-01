using System;
using UnityEngine;
using UnityEngine.UI;

public class FloatPanel : BasePanel
{
    public Text Content;
    public Button OK;
    public Button Cancel;

    private Action OnConfirm;

    public override void Show()
    {
        base.Show();

        OK.onClick.RemoveAllListeners();
        if (Cancel != null)
            Cancel.onClick.RemoveAllListeners();

        bool needConfirm = OnConfirm != null;

        if (Cancel != null)
        {
            Cancel.gameObject.SetActive(needConfirm);
        }


        OK.onClick.AddListener(() =>
        {
            AudioService.Instance.PlayEffect("Button");
            HideMe();
            OnConfirm?.Invoke();
        });

        if (needConfirm && Cancel != null)
        {
            Cancel.onClick.AddListener(() =>
            {
                AudioService.Instance.PlayEffect("Button");
                HideMe();
            });
        }
    }

    public void Init(string str, Action confirmAction)
    {
        Content.text = str;
        OnConfirm = confirmAction;
    }
}
