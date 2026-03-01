using UnityEngine;
using UnityEngine.UI;

public class StartPanel : BasePanel
{
    public Button StartButton;
    public Button SetButton;

    public Text NameText;
    public Text CoinText;
    public Text LevelText;

    public override void Show()
    {
        base.Show();

        AudioService.Instance.PlayBK("StartBK");
        UpdatePanel();

        StartButton.onClick.RemoveListener(OnClickStart);
        StartButton.onClick.AddListener(OnClickStart);

        SetButton.onClick.RemoveListener(OnClickSet);
        SetButton.onClick.AddListener(OnClickSet);
    }

    private void OnClickStart()
    {
        AudioService.Instance.PlayEffect("Button");

        if (LevelSystem.Instance.CurrentLevel > 6)
        {
            UIService.Instance.ShowPanel<FloatPanel>(3).Init(
                "Success! \n Restart?",
                () =>
                {
                    DataSystem.Instance.ResetData();
                    UpdatePanel();
                    UIService.Instance.ShowPanel<FloatPanel>(3)
                        .Init("Data has been deleted", null);
                }
            );
            return;
        }

        HideMe();
        LevelSystem.Instance.LoadTown();
    }

    private void OnClickSet()
    {
        AudioService.Instance.PlayEffect("Button");
        UIService.Instance.ShowPanel<SetPanel>(1);
    }

    public override void UpdatePanel()
    {
        if (DataSystem.Instance.CurrentLoginData != null && NameText != null)
            NameText.text = DataSystem.Instance.CurrentLoginData.PlayerName;

        if (PlayerSystem.Instance != null)
        {
            if (CoinText != null)
                CoinText.text = PlayerSystem.Instance.CoinCount.ToString();

            if (LevelText != null)
                LevelText.text = PlayerSystem.Instance.Level.ToString();
        }
    }

    public void OnClickProducer()
    {
        AudioService.Instance.PlayEffect("Button");
        UIService.Instance.ShowPanel<ProducerPanel>();
    }
}
