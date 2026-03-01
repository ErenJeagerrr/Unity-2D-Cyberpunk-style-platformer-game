using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
public abstract class BasePanel : MonoBehaviour
{
    private Dictionary<string, List<UIBehaviour>> MyDict = new Dictionary<string, List<UIBehaviour>>();
    public int layCount { get; set; }
    private void Awake()
    {
        GetChildConponent<Button>();
        GetChildConponent<Image>();
        GetChildConponent<Text>();
        if (this.GetType().Name != gameObject.name.Replace("(Clone)", ""))
        {
            Debug.LogError("类名与面板名字不一致，面板为" + gameObject.name.Replace("(Clone)", "") + "类为" + this.GetType().Name);
        }
    }
    public void HideMe()
    {
        UIService.Instance.HidePanel(gameObject.name.Replace("(Clone)", ""));
    }
    public virtual void Hide() { }
    public virtual void Show() { }
    public virtual void UpdatePanel() { }
    public T GetElement<T>(string name) where T : UIBehaviour
    {
        if (MyDict.ContainsKey(name))
        {
            for (int i = 0; i < MyDict[name].Count; i++)
            {
                if (MyDict[name][i] is T)
                {
                    return MyDict[name][i] as T;
                }
            }
        }
        Debug.Log("No component found." + typeof(T).Name);
        return null;
    }
    private void GetChildConponent<T>() where T : UIBehaviour
    {
        T[] compontents = GetComponentsInChildren<T>();
        string objName;
        for (int i = 0; i < compontents.Length; i++)
        {
            objName = compontents[i].gameObject.name;
            if (MyDict.ContainsKey(objName))
            {
                MyDict[objName].Add(compontents[i]);
            }
            else
            {
                MyDict.Add(objName, new List<UIBehaviour>() { compontents[i] });
            }
        }
    }
}
