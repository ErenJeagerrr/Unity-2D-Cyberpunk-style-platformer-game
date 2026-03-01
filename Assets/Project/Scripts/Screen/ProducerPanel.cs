using UnityEngine;

public class ProducerPanel : BasePanel
{
    public override void Show()
    {
        base.Show();
    }

    public void OnClickBack()
    {
        AudioService.Instance.PlayEffect("Button");

        UIService.Instance.HidePanel<ProducerPanel>();

        StartPanel start = UIService.Instance.GetPanel<StartPanel>();
        if (start != null)
        {
            start.Show();
        }
    }

}
