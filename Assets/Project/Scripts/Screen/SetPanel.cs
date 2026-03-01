using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetPanel : BasePanel
{
    public Slider BKAudio;
    public Slider EffectAudio;
    public Button Back;
    public override void Show()
    {
        base.Show();
        Back.onClick.AddListener(() =>
        {
            AudioService.Instance.PlayEffect("Button");
            HideMe();
        });
        BKAudio.value = AudioService.Instance.BKVolume / 0.2f;
        EffectAudio.value = AudioService.Instance.EffectVolume;
        BKAudio.onValueChanged.AddListener((value) =>
        {
            AudioService.Instance.ChangeBKVolume(value);
        });
        EffectAudio.onValueChanged.AddListener((value) =>
        {
            AudioService.Instance.ChangeEffectVolume(value);
        });
    }
}
