using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResService : SingletonBase<ResService>
{
    public Image BlackUI;
    public string CurrentSceneName => SceneManager.GetActiveScene().name;

    private GameObject CoinPrefab;

    public override void Init()
    {
        base.Init();
        CoinPrefab = Resources.Load<GameObject>("Prefab/Coin");
    }

    public GameObject LoadPrefab(string Path)
    {
        GameObject prefab = Resources.Load<GameObject>(Path);
        GameObject go = Instantiate(prefab);
        return go;
    }

    public void LoadScene(string SceneName, Action Loaded)
    {
        StartCoroutine(IELoadScene(SceneName, Loaded));
    }

    private IEnumerator IELoadScene(string SceneName, Action Loaded)
    {
        AudioService.Instance.StopBK();
        LoadPanel panel = UIService.Instance.ShowPanel<LoadPanel>(3);
        
        float progress = 0;
        while (true)
        {
            yield return null;
            progress += Time.deltaTime * 100;
            if (progress >= 100) break;
            if (panel != null) panel.SetProgress(progress);
        }

        AsyncOperation ar = SceneManager.LoadSceneAsync(SceneName);
        
        if (ar == null)
        {
            Debug.LogError($"【严重错误】无法加载场景: {SceneName}。请检查：1. Build Settings是否已添加该场景。 2. 场景名字是否拼写正确。");
            if (panel != null) UIService.Instance.HidePanel<LoadPanel>();
            yield break; 
        }

        while (!ar.isDone)
        {
            yield return null;
        }
        
        Loaded?.Invoke();

        if (panel != null)
        {
            UIService.Instance.HidePanel<LoadPanel>();
        }
    } 

    public void CreatCoin(int Count, Vector3 Pos)
    {
        if (CoinPrefab == null) return;
        for (int i = 0; i < Count; i++)
        {
            Instantiate(CoinPrefab, Pos, Quaternion.identity).GetComponent<Coin>().Init();
        }
    }
}