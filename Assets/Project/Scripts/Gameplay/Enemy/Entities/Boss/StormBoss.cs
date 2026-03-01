using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormBoss : BossBase
{
    [Header("--- Storm Boss Properties ---")]
    public float HealCooldown = 15f;
    public int HealAmount = 30;
    public float HealThreshold = 0.5f;

    [Header("--- Skills Config ---")]
    public float ThunderSlamCooldown = 12f;
    public float LaserCooldown = 15f;
    public float TeleportSlamCooldown = 18f;
    public float WeaponLaserCooldown = 14f;

    [Header("--- Prefab ---")]
    public GameObject LaserPrefab;
    public GameObject WeaponLaserPrefab;

    [Header("New Skills")]
    public GameObject ShockwavePrefab;

    [Header("--- Weapon Position ---")]
    public Transform WeaponPos;

    public StormBossChaseState StormChaseState { get; private set; }
    public StormBossAttackState StormAttackState { get; private set; }
    public StormBossHealState StormHealState { get; private set; }
    public StormBossThunderSlamState StormThunderSlamState { get; private set; }
    public StormBossLaserState StormLaserState { get; private set; }
    public StormBossTeleportSlamState StormTeleportSlamState { get; private set; }
    public StormBossLaserAttackState StormWeaponLaserState { get; private set; }

    public Dictionary<string, float> SkillCDs = new Dictionary<string, float>();
    public bool IsImmunity { get; set; } = false;

    protected override void InitState()
    {
        CurrentAtk = Data.Attack;
        CurrentMoveSpeed = Data.MoveSpeed;
        CurrentChaseSpeed = Data.ChaseSpeed;

        if (!SkillCDs.ContainsKey("Heal")) SkillCDs.Add("Heal", 0);
        if (!SkillCDs.ContainsKey("ThunderSlam")) SkillCDs.Add("ThunderSlam", 5f);
        if (!SkillCDs.ContainsKey("Laser")) SkillCDs.Add("Laser", 8f);
        if (!SkillCDs.ContainsKey("TeleportSlam")) SkillCDs.Add("TeleportSlam", 10f);
        if (!SkillCDs.ContainsKey("WeaponLaser")) SkillCDs.Add("WeaponLaser", 6f);

        BossStartState = new BossStartState(this, state, "Idle_StormBoss");
        BossIdleState = new BossIdleState(this, state, "Idle_StormBoss");
        BossDieState = new BossDieState(this, state, "Die_StormBoss");

        StormChaseState = new StormBossChaseState(this, state, "Run_StormBoss");
        StormAttackState = new StormBossAttackState(this, state, "Attack_StormBoss");
        StormHealState = new StormBossHealState(this, state, "Heal_StormBoss");
        StormThunderSlamState = new StormBossThunderSlamState(this, state, "Jump_StormBoss");
        StormLaserState = new StormBossLaserState(this, state, "BeamAttack_StormBoss");
        StormTeleportSlamState = new StormBossTeleportSlamState(this, state, "Teleport_StormBoss");
        StormWeaponLaserState = new StormBossLaserAttackState(this, state, "ChargeLaser_StormBoss");

        this.BossChaseState = StormChaseState;

        state.Init(BossStartState);
    }

    public override void Update()
    {
        if (IsDie) return;
        base.Update();

        List<string> keys = new List<string>(SkillCDs.Keys);
        foreach (var key in keys)
        {
            if (SkillCDs[key] > 0)
            {
                SkillCDs[key] -= Time.deltaTime;
            }
        }
    }

    public override void Hurt(Transform pos, float Value)
    {
        if (IsImmunity) return;
        base.Hurt(pos, Value);
    }

    public bool TryUseSkill(string skillName)
    {
        if (!SkillCDs.ContainsKey(skillName)) return false;

        if (SkillCDs[skillName] <= 0)
        {
            float cd = 0;
            switch (skillName)
            {
                case "Heal": cd = HealCooldown; break;
                case "ThunderSlam": cd = ThunderSlamCooldown; break;
                case "Laser": cd = LaserCooldown; break;
                case "TeleportSlam": cd = TeleportSlamCooldown; break;
                case "WeaponLaser": cd = WeaponLaserCooldown; break;
            }
            SkillCDs[skillName] = cd;
            return true;
        }
        return false;
    }

    public void UseHeal()
    {
        CurrentHeathl += HealAmount;
        if (CurrentHeathl > MaxHeathl) CurrentHeathl = MaxHeathl;
    }
}