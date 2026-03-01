using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallRed : BossBase
{
    [Header("--- SmallRed Specific ---")]
    [Header("SP Attack Settings")]
    public Transform BossSPAttackPos;
    public Vector2 BossSPAttackRadius;
    public float SPCD = 8f;

    [Header("Summon Skill Settings")]
    public GameObject[] MinionPrefabs;
    public float SummonCD = 15f;

    [Header("UI Optimization")]
    public Vector3 HealthBarOffset = new Vector3(0, 2.2f, 0);

    // --- State References ---
    public SmallRedSPAttackState SPAttackState { get; private set; }
    public SmallRedChaseState RedChaseState { get; private set; }
    public SmallRedSummonState SummonState { get; private set; }

    // 技能冷却字典
    public Dictionary<string, float> SkillCDs = new Dictionary<string, float>();

    protected override void InitState()
    {
        CurrentAtk = Data.Attack;
        CurrentMoveSpeed = Data.MoveSpeed;
        CurrentChaseSpeed = Data.ChaseSpeed;

        SkillCDs["SPAttack"] = SPCD;
        SkillCDs["Summon"] = SummonCD;

        BossStartState = new BossStartState(this, state, "Idle");
        BossIdleState = new BossIdleState(this, state, "Idle");
        BossDieState = new BossDieState(this, state, "Die");

        BossAttackState = new SmallRedAttackState(this, state, "Attack");

        RedChaseState = new SmallRedChaseState(this, state, "Chase");
        SPAttackState = new SmallRedSPAttackState(this, state, "SPAttack");
        SummonState = new SmallRedSummonState(this, state, "Attack");

        BossChaseState = RedChaseState;
        state.Init(BossStartState);
    }

    public override void Update()
    {
        base.Update();

        List<string> keys = new List<string>(SkillCDs.Keys);
        foreach (var key in keys)
        {
            if (SkillCDs[key] > 0)
                SkillCDs[key] -= Time.deltaTime;
        }
    }

    public bool TryUseSkill(string skillName)
    {
        if (SkillCDs.ContainsKey(skillName) && SkillCDs[skillName] <= 0)
        {
            if (skillName == "SPAttack") SkillCDs[skillName] = SPCD;
            else if (skillName == "Summon") SkillCDs[skillName] = SummonCD;

            return true;
        }
        return false;
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        if (IsDie) return;
        if (BleedSlider != null)
        {
            BleedSlider.transform.position = transform.position + HealthBarOffset;
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (BossSPAttackPos != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(BossSPAttackPos.position, BossSPAttackRadius);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position + HealthBarOffset, 0.1f);
    }
}