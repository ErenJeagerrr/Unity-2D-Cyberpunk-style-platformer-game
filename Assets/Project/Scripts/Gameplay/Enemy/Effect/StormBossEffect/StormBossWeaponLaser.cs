using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class StormBossWeaponLaser : MonoBehaviour
{
    private int damage;
    private float duration;
    private Transform firePoint;
    private Transform target;

    private LineRenderer lr;
    private float timer;
    private bool isInit = false;

    private float damageInterval = 0.2f;
    private float damageTimer = 0;

    // --- Tracking Parameters ---
    private Vector3 currentDir;    // Current laser direction
    [Header("Tracking")]
    public float TurnSpeed = 0.3f;

    [Header("Visual")]
    public float MaxLength = 30f;
    public GameObject HitEffect;
    public float ScrollSpeed = 4f; // Texture scroll speed

    // Width Animation Curve
    public AnimationCurve WidthCurve = new AnimationCurve(
        new Keyframe(0, 0),
        new Keyframe(0.2f, 1f),
        new Keyframe(0.8f, 1f),
        new Keyframe(1f, 0)
    );
    private float baseWidthMultiplier = 1f;

    // --- Fluctuation Parameters ---
    [Header("Jitter")]
    public float JitterIntensity = 0.1f;
    public float JitterSpeed = 20f;

    /// <summary>
    /// 初始化激光
    /// </summary>
    /// <param name="point">Launch site</param>
    /// <param name="targetPlayer">Tracking Target (can be empty)</param>
    /// <param name="dmg">damage</param>
    /// <param name="dur">duration</param>
    /// <param name="laserWidth">base width</param>
    public void Init(Transform point, Transform targetPlayer, int dmg, float dur, float laserWidth)
    {
        this.firePoint = point;
        this.target = targetPlayer;
        this.damage = dmg;
        this.duration = dur;
        this.baseWidthMultiplier = laserWidth;

        lr = GetComponent<LineRenderer>();

        lr.positionCount = 2;
        lr.useWorldSpace = true;

        // Initialize orientation to point directly forward from the launch point.
        currentDir = point.right;

        isInit = true;

        if (AttackEffect.Instance != null) AttackEffect.Instance.Shake();

        Destroy(gameObject, duration);
    }

    void Update()
    {
        if (!isInit || firePoint == null) return;

        timer += Time.deltaTime;
        damageTimer += Time.deltaTime;

        // Tracking Logic
        if (target != null)
        {
            Vector3 targetDir = (target.position - firePoint.position).normalized;
            currentDir = Vector3.RotateTowards(currentDir, targetDir, TurnSpeed * Time.deltaTime, 0.0f);
        }

        currentDir.Normalize();


        // ================= Visual Effects =================

        // Dynamic Width Variation (Using Curves + Noise)
        float lifeProgress = timer / duration;
        float curveValue = WidthCurve.Evaluate(lifeProgress);

        // Berlin noise exhibits random fluctuations.
        float noise = Mathf.PerlinNoise(Time.time * JitterSpeed, 0f);

        // Final Width = Base Width * Animation Curve * (Random Fluctuation between 0.8 and 1.2)
        float finalWidth = baseWidthMultiplier * curveValue * (0.8f + noise * JitterIntensity * 2f);

        lr.startWidth = finalWidth;
        lr.endWidth = finalWidth * 1.5f;


        // ================= Ray Detection and Position Update =================
        Vector3 startPos = firePoint.position;

        // 射线检测
        RaycastHit2D hit = Physics2D.Raycast(startPos, currentDir, MaxLength, LayerMask.GetMask("Ground"));

        Vector3 endPos;
        if (hit.collider != null)
        {
            endPos = hit.point;
            if (HitEffect != null)
            {
                HitEffect.SetActive(true);
                HitEffect.transform.position = endPos;

                float angle = Mathf.Atan2(hit.normal.y, hit.normal.x) * Mathf.Rad2Deg;
                HitEffect.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
        else
        {
            endPos = startPos + currentDir * MaxLength;
            if (HitEffect != null) HitEffect.SetActive(false);
        }

        lr.SetPosition(0, startPos);
        lr.SetPosition(1, endPos);

        if (damageTimer >= damageInterval)
        {
            CheckDamage(startPos, endPos, currentDir);
            damageTimer = 0;
        }
    }

    private void CheckDamage(Vector3 start, Vector3 end, Vector3 dir)
    {
        float distance = Vector3.Distance(start, end);
        Vector2 center = (start + end) / 2f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        float checkWidth = Mathf.Max(0.2f, lr.startWidth);

        Collider2D[] cols = Physics2D.OverlapBoxAll(center, new Vector2(distance, checkWidth), angle, LayerMask.GetMask("Player"));

        foreach (var col in cols)
        {
            col.GetComponent<IHurt>()?.Hurt(transform, damage);
        }
    }
}