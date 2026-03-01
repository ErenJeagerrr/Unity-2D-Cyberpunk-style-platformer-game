using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class DataSystem : SingletonBase<DataSystem>
{

    public Data CurrentLoginData { get; private set; }

    public override void Init()
    {
        base.Init();
        string path = Application.persistentDataPath + "UserData.json";

        if (File.Exists(path))
        {
            string JsonStr = File.ReadAllText(path);
            CurrentLoginData = JsonUtility.FromJson<Data>(JsonStr);

            if (CurrentLoginData.CurrentLevel <= 0)
            {
                CurrentLoginData.CurrentLevel = 1;
            }
        }
        else
        {
            CreateNewData();
        }
    }

    private void CreateNewData()
    {
        CurrentLoginData = new Data();
        CurrentLoginData.PlayerName = "Hero";
        CurrentLoginData.CoinCount = 1000;
        CurrentLoginData.Level = 1;
        CurrentLoginData.CurrentEXP = 0;
        CurrentLoginData.CurrentLevel = 1; 
        
        CurrentLoginData.EquipWeaponAtk = 0;
        CurrentLoginData.EquipArmorHP = 0;
        CurrentLoginData.EquipShoeSpd = 0;

        Save();
    }


    public void ResetData()
    {

        string path = Application.persistentDataPath + "UserData.json";
        if (File.Exists(path))
        {
            File.Delete(path);
        }

  
        if (PlayerSystem.Instance != null)
        {
            PlayerSystem.Instance.CoinCount = 0;
            PlayerSystem.Instance.Level = 1;
            PlayerSystem.Instance.CurrentEXP = 0;
            PlayerSystem.Instance.CurrentHeathl = PlayerSystem.Instance.MaxHeathl;

        }

        if (LevelSystem.Instance != null)
        {
            LevelSystem.Instance.CurrentLevel = 1;
        }

        CreateNewData();
        
        if (PlayerSystem.Instance != null)
        {
            PlayerSystem.Instance.Load();
        }

        Debug.Log("存档已删除并重置为初始状态");
    }

    public void Save()
    {
        if (PlayerSystem.Instance != null)
        {
            CurrentLoginData.CoinCount = PlayerSystem.Instance.CoinCount;
            CurrentLoginData.Level = PlayerSystem.Instance.Level;
            CurrentLoginData.CurrentEXP = PlayerSystem.Instance.CurrentEXP;
        }

        if (LevelSystem.Instance != null)
        {
            CurrentLoginData.CurrentLevel = LevelSystem.Instance.CurrentLevel;
        }

        if (CurrentLoginData.CurrentLevel <= 0) CurrentLoginData.CurrentLevel = 1;

        string str = JsonUtility.ToJson(CurrentLoginData);
        File.WriteAllText(Application.persistentDataPath + "UserData.json", str);
    }

    public void Load()
    {
        if (PlayerSystem.Instance != null) PlayerSystem.Instance.Load();
        if (LevelSystem.Instance != null) LevelSystem.Instance.Load();
    }
}

[System.Serializable]
public class Data
{
    public string PlayerName;
    public int CoinCount;
    public int Level;
    public int CurrentEXP;
    public int CurrentLevel;

    public int EquipWeaponAtk; 
    public int EquipArmorHP;   
    public int EquipShoeSpd;  
}