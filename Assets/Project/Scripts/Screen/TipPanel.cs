using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipPanel : BasePanel
{
    public Text Content;
    public void Init(string Content)
    {
        this.Content.text = Content;
    }
}
