using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))] // 强制要求有 SpriteRenderer，防止报错
public class StormBossLaser : MonoBehaviour
{
    private int damage;
    private float warningTime;
    private float activeTime;

    [Header("Art Settings")]
    public Sprite WarningSprite;
    public Sprite[] ActiveSprites;
    public float AniSpeed = 0.1f;

    [Header("Visual Settings")]
    [Tooltip("预警时的宽度比例。例如 0.2 表示预警线宽度是攻击宽度的 20%")]
    [Range(0.01f, 1f)]
    public float warningWidthRatio = 0.2f;

    [Header("Gameplay Settings")]
    [Tooltip("实际伤害判定的宽度比例 (0~1)。例如 0.33 表示只有中间 1/3 的区域有伤害")]
    [Range(0.01f, 1f)]
    public float hitBoxWidthRatio = 0.33f; // 默认为 1/3，您可以根据素材在面板调整

    private SpriteRenderer sr;
    private Vector3 originalScale; // 记录 Prefab 原始大小

    // Init 不再需要传入 size，直接用 Prefab 自己的大小
    public void Init(int dmg, float warnTime, float actTime)
    {
        this.damage = dmg;
        this.warningTime = warnTime;
        this.activeTime = actTime;

        sr = GetComponent<SpriteRenderer>();

        // 1. 记录下您在编辑器里调好的大小
        originalScale = transform.localScale;

        // 2. 初始设为预警宽度 (变细)
        transform.localScale = new Vector3(originalScale.x * warningWidthRatio, originalScale.y, originalScale.z);

        StartCoroutine(LaserProcess());
    }

    IEnumerator LaserProcess()
    {
        // === 预警阶段 ===
        if (WarningSprite != null) sr.sprite = WarningSprite;
        // 预警时稍微半透明
        sr.color = new Color(1f, 1f, 1f, 0.8f);

        yield return new WaitForSeconds(warningTime);

        // === 攻击阶段 ===
        // 1. 恢复为您在编辑器里设置的原始大小
        transform.localScale = originalScale;

        // 2. 恢复不透明
        sr.color = Color.white;

        if (AttackEffect.Instance != null) AttackEffect.Instance.Shake();

        float timer = 0;
        float aniTimer = 0;
        int spriteIndex = 0;
        List<GameObject> hitList = new List<GameObject>();

        // 立即显示第一张攻击图
        if (ActiveSprites != null && ActiveSprites.Length > 0)
            sr.sprite = ActiveSprites[0];

        while (timer < activeTime)
        {
            float dt = Time.deltaTime;
            timer += dt;
            aniTimer += dt;

            // 动画循环
            if (ActiveSprites != null && ActiveSprites.Length > 0)
            {
                if (aniTimer >= AniSpeed)
                {
                    aniTimer = 0;
                    spriteIndex = (spriteIndex + 1) % ActiveSprites.Length;
                    sr.sprite = ActiveSprites[spriteIndex];
                }
            }

            CheckDamage(hitList);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void CheckDamage(List<GameObject> hitList)
    {
        if (sr == null) return;

        // 【关键修改】计算缩小的判定框尺寸
        // sr.bounds.size 是图片的完整世界尺寸
        // 我们只缩放 X 轴 (宽度)，Y 轴保持不变
        Vector2 hitBoxSize = new Vector2(sr.bounds.size.x * hitBoxWidthRatio, sr.bounds.size.y);

        // 使用计算后的 hitBoxSize 进行检测
        Collider2D[] hits = Physics2D.OverlapBoxAll(sr.bounds.center, hitBoxSize, 0, LayerMask.GetMask("Player"));

        foreach (var hit in hits)
        {
            if (!hitList.Contains(hit.gameObject))
            {
                hit.GetComponent<IHurt>()?.Hurt(transform, damage);
                hitList.Add(hit.gameObject);
            }
        }
    }

    private void OnDrawGizmos()
    {
        // 只有当有 SpriteRenderer 时才能画出准确的框
        SpriteRenderer s = GetComponent<SpriteRenderer>();
        if (s != null)
        {
            Gizmos.color = Color.cyan;

            // 在 Gizmos 中也显示实际的缩水判定范围，方便调试
            // 这样你在 Scene 窗口看到的青色框就是实际会造成伤害的区域
            Vector2 drawSize = s.bounds.size;
            drawSize.x *= hitBoxWidthRatio; // 应用宽度比例

            Gizmos.DrawWireCube(s.bounds.center, drawSize);
        }
    }
}