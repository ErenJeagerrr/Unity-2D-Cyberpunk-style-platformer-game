using System.Collections;
using UnityEngine;

public class LaserTrap : MonoBehaviour
{
    // 定义枚举：激光的方向
    public enum LaserDirection
    {
        Horizontal, // 横向（改 Y 轴作为宽度）
        Vertical    // 竖向（改 X 轴作为宽度）
    }

    [Header("方向设置")]
    public LaserDirection direction = LaserDirection.Horizontal;

    [Header("组件引用")]
    public SpriteRenderer beamSprite;   // 拖入激光子物体的 Sprite
    public BoxCollider2D beamCollider;  // 拖入激光子物体的 Collider

    [Header("时间设置")]
    public float startDelay = 0f;       // 初始延迟
    public float interval = 3f;         // 休息间隔
    public float warnTime = 1.0f;       // 预警时间
    public float activeTime = 1.5f;     // 激活时间
    public float fadeOutTime = 0.5f;    // 消失时间

    [Header("外观设置")]
    public float minWidth = 0.1f;       // 细线宽度
    public float maxWidth = 1.0f;       // 粗线宽度
    
    [Header("伤害设置")]
    public int damage = 20;

    private bool isDangerous = false;
    private Vector3 originalScale; // 记录原始缩放，保留长度信息

    void Start()
    {
        if (beamSprite == null || beamCollider == null)
        {
            Debug.LogError("请在 Inspector 中赋值 Beam 组件！");
            return;
        }

        // 记录激光初始长度（我们只改宽度，不改长度）
        originalScale = beamSprite.transform.localScale;

        // 初始隐藏
        SetLaserVisual(0f, 0f);
        beamCollider.enabled = false;
        
        StartCoroutine(LaserLoop());
    }

    IEnumerator LaserLoop()
    {
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            // 1. 休息阶段
            isDangerous = false;
            beamCollider.enabled = false;
            SetLaserVisual(0f, 0f);
            yield return new WaitForSeconds(interval);

            // 2. 预警阶段 (Fade In)
            float timer = 0f;
            while (timer < warnTime)
            {
                timer += Time.deltaTime;
                float progress = timer / warnTime;
                
                // 透明度 0 -> 0.5
                float alpha = Mathf.Lerp(0f, 0.5f, progress);
                SetLaserVisual(alpha, minWidth);
                yield return null;
            }

            // 3. 激活阶段 (变宽 + 伤害)
            isDangerous = true;
            beamCollider.enabled = true;

            // 瞬间变宽动画
            float expandTime = 0.2f;
            timer = 0f;
            while (timer < expandTime)
            {
                timer += Time.deltaTime;
                float progress = timer / expandTime;
                float width = Mathf.Lerp(minWidth, maxWidth, progress);
                SetLaserVisual(1f, width);
                yield return null;
            }

            // 保持粗度
            yield return new WaitForSeconds(activeTime - expandTime);

            // 4. 消失阶段 (Fade Out)
            isDangerous = false;
            beamCollider.enabled = false;

            timer = 0f;
            while (timer < fadeOutTime)
            {
                timer += Time.deltaTime;
                float progress = timer / fadeOutTime;
                
                // 透明度 1 -> 0
                float alpha = Mathf.Lerp(1f, 0f, progress);
                // 宽度变回 0
                float width = Mathf.Lerp(maxWidth, 0f, progress);
                SetLaserVisual(alpha, width);
                yield return null;
            }
        }
    }

    // 根据方向设置宽高
    private void SetLaserVisual(float alpha, float width)
    {
        // 1. 改颜色透明度
        Color c = beamSprite.color;
        c.a = alpha;
        beamSprite.color = c;

        // 2. 改缩放
        Vector3 newScale = originalScale;

        if (direction == LaserDirection.Horizontal)
        {
            // 横向激光：X是长度（保持不变），Y是宽度（变化）
            newScale.y = width;
        }
        else
        {
            // 竖向激光：X是宽度（变化），Y是长度（保持不变）
            newScale.x = width;
        }

        beamSprite.transform.localScale = newScale;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isDangerous) return;

        if (other.CompareTag("Player"))
        {
            IHurt hurt = other.GetComponent<IHurt>();
            if (hurt != null)
            {
                hurt.Hurt(transform, damage);
            }
        }
    }
}