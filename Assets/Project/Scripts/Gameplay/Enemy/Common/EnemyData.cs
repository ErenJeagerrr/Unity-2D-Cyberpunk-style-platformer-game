using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Data/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("生命与属性")]
    public float MaxHeathl;
    public int Attack;

    [Header("移动与 AI")]
    public int MoveSpeed;
    public int ChaseSpeed;
    public float CheckRadius;
    public float ReactTime;
    public float PatrolDistance;
    public float IdleTime;

    [Header("战斗参数")]
    public float AttackRadius;
    public Vector3 BowRadius; // 仅限远程兵种使用
    public float AttackCD;

    [Header("表现与奖励")]
    public GameObject HitEffect;
    public int MaxEXP;  
    public int FallCoin;
}