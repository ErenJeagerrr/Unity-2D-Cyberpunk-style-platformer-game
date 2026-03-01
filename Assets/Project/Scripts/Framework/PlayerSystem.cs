using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public enum ShopItemType
{
    Heart,
    Weapon,
    Armor,
    Shoes
}

public class PlayerSystem : SingletonBase<PlayerSystem>
{
    #region 属性定义
    private float _CurrentHeathl;
    public float CurrentHeathl
    {
        get => _CurrentHeathl;
        set
        {
            _CurrentHeathl = value;
            if (_CurrentHeathl > MaxHeathl) _CurrentHeathl = MaxHeathl;
            if (_CurrentHeathl <= 0) _CurrentHeathl = 0;
            UIService.Instance.UpdatePanel<BattlePanel>();
        }
    }

    private float _MaxHeathl;
    public float MaxHeathl
    {
        get => _MaxHeathl;
        set
        {
            _MaxHeathl = value;
            UIService.Instance.UpdatePanel<BattlePanel>();
        }
    }

    private int _CoinCount;
    public int CoinCount
    {
        get => _CoinCount;
        set
        {
            if (value > _CoinCount)
            {
                AudioService.Instance.PlayEffect("Coin"); 
            }
            _CoinCount = value;
            UIService.Instance.UpdatePanel<BattlePanel>();
            UIService.Instance.UpdatePanel<StartPanel>();
        }
    }

    private int _CurrentEXP;
    public int CurrentEXP
    {
        get => _CurrentEXP;
        set
        {
            _CurrentEXP = value;
            if (_CurrentEXP >= 100)
            {
                Level++;
                _CurrentEXP = 0;
            }
            UIService.Instance.UpdatePanel<BattlePanel>();
        }
    }

    private int _Level;
    public int Level
    {
        get => _Level;
        set
        {
            if (value > 10) return;
            _Level = value;
            BaseAttack = 10 + (_Level - 1) * 3;
            BaseMaxHeathl = 100 + (_Level - 1) * 10;
            ReCalculateData();
            UIService.Instance.UpdatePanel<BattlePanel>();
        }
    }

    public int Attack { get; set; }
    public int MoveSpeed { get; set; }
    #endregion

    #region 基础变量
    private int BaseMoveSpeed;
    private float BaseMaxHeathl;
    private int BaseAttack;
    public float BaseRecoverCD { get; private set; }

    public GameObject PlayerPrefab;
    public GameObject CameraPrefab;
    public Player player { get; private set; }
    public bool IsImmunity { get; set; }
    public bool IsPause { get; private set; }
    public float RecoverCD { get; private set; }
    #endregion

    public override void Init()
    {
        base.Init();
        BaseMaxHeathl = 100;
        BaseMoveSpeed = 7;
        BaseAttack = 10;
        BaseRecoverCD = 50;
        RecoverDefaultData();
        RecoverCD = 0;
    }

    public void Load()
    {
        if (DataSystem.Instance.CurrentLoginData != null)
        {
            CoinCount = DataSystem.Instance.CurrentLoginData.CoinCount;
            CurrentEXP = DataSystem.Instance.CurrentLoginData.CurrentEXP;
            Level = DataSystem.Instance.CurrentLoginData.Level;
        }
        ReCalculateData();
    }

    private void Update()
    {
        if (RecoverCD > 0)
        {
            RecoverCD -= Time.deltaTime;
            UIService.Instance.GetPanel<BattlePanel>()?.UpdateRecover();
        }
    }

    public void UseRecover()
    {
        if (RecoverCD > 0) return;
        CurrentHeathl += 40;
        AudioService.Instance.PlayEffect("Heal"); 
        RecoverCD = BaseRecoverCD;
    }

    private void RecoverDefaultData()
    {
        MoveSpeed = BaseMoveSpeed;
        MaxHeathl = BaseMaxHeathl;
        Attack = BaseAttack;
    }

    public void CreatPlayer(Vector3 pos)
    {
        CurrentHeathl = MaxHeathl;
        player = Instantiate(PlayerPrefab, pos, Quaternion.identity).GetComponent<Player>();
        player.Init();

        GameObject go = Instantiate(CameraPrefab);
        GameObject boundObj = GameObject.Find("Bound");
        if (boundObj != null)
        {
            PolygonCollider2D Bound = boundObj.GetComponent<PolygonCollider2D>();
            go.GetComponent<CinemachineConfiner2D>().m_BoundingShape2D = Bound;
        }
        go.GetComponent<CinemachineVirtualCamera>().Follow = player.transform;
    }

    public void ReCalculateData()
    {
        RecoverDefaultData();

        if (DataSystem.Instance != null && DataSystem.Instance.CurrentLoginData != null)
        {
            Attack += DataSystem.Instance.CurrentLoginData.EquipWeaponAtk;
            MaxHeathl += DataSystem.Instance.CurrentLoginData.EquipArmorHP;
            MoveSpeed += DataSystem.Instance.CurrentLoginData.EquipShoeSpd;
        }
    }

    public bool BuyItem(ShopItemType type, int value, int price)
    {
        if (CoinCount < price)
        {
            UIService.Instance.ShowPanel<FloatPanel>(3).Init("Not enough money!", null);
            return false;
        }

        CoinCount -= price;

        switch (type)
        {
            case ShopItemType.Heart:
                CurrentHeathl += value;
                UIService.Instance.ShowPanel<FloatPanel>(3).Init($"Recovered {value} HP!", null);
                break;

            case ShopItemType.Weapon:
                int oldAtk = DataSystem.Instance.CurrentLoginData.EquipWeaponAtk;
                Attack -= oldAtk;
                Attack += value;
                DataSystem.Instance.CurrentLoginData.EquipWeaponAtk = value;
                UIService.Instance.ShowPanel<FloatPanel>(3).Init($"Weapon Upgraded! Atk +{value}", null);
                break;

            case ShopItemType.Armor:
                int oldHP = DataSystem.Instance.CurrentLoginData.EquipArmorHP;
                MaxHeathl -= oldHP;
                MaxHeathl += value;
                DataSystem.Instance.CurrentLoginData.EquipArmorHP = value;
                CurrentHeathl = MaxHeathl;
                UIService.Instance.ShowPanel<FloatPanel>(3).Init($"Armor Equipped! MaxHP +{value}", null);
                break;

            case ShopItemType.Shoes:
                int oldSpd = DataSystem.Instance.CurrentLoginData.EquipShoeSpd;
                MoveSpeed -= oldSpd;
                MoveSpeed += value;
                DataSystem.Instance.CurrentLoginData.EquipShoeSpd = value;
                UIService.Instance.ShowPanel<FloatPanel>(3).Init($"Shoes Equipped! Speed +{value}", null);
                break;
        }

        UIService.Instance.UpdatePanel<BattlePanel>();
        DataSystem.Instance.Save();
        return true;
    }

    public void Pause()
    {
        if (player == null) return;
        player.PausePlayer();
        IsPause = true;
    }

    public void Continue()
    {
        if (player == null) return;
        player.ContinuePlayer();
        IsPause = false;
    }
}