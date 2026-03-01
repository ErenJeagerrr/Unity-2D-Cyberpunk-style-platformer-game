using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class LoadPanel : BasePanel
{
    public Slider ProgressSlider;
    public Text ProgressText;

    public override void Show()
    {
        base.Show();
        if (ProgressSlider != null) ProgressSlider.value = 0;
        if (ProgressText != null) ProgressText.text = "0%";
    }

    public void SetProgress(float value)
    {
        if (ProgressSlider != null)
        {
            ProgressSlider.value = value / 100;
        }
        
        if (ProgressText != null)
        {
            ProgressText.text = (int)value + "%";
        }
    }
}