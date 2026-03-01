using System.Collections;
using UnityEngine;

public class FireTrap : MonoBehaviour
{
    [Header("组件引用")]
    public Animator anim;
    public BoxCollider2D fireCollider;

    [Header("时间设置")]
    public float startDelay = 0f;      // 初始延迟（错开陷阱）
    public float cooldownTime = 3f;    // 两次喷火的间隔
    
    [Header("动画配合设置 (关键)")]
    // 这一步需要你去Animation窗口看一眼，火从第几秒开始变大的？
    public float damageDelay = 0.3f;   // 动画开始播放后，过多久开启伤害（火苗变大）
    public float damageDuration = 1.0f;// 伤害持续多久（大火持续时间）

    [Header("伤害设置")]
    public int damage = 30;

    private void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (fireCollider == null) fireCollider = GetComponent<BoxCollider2D>();

        // 初始状态：关闭碰撞
        fireCollider.enabled = false;
        
        StartCoroutine(FireRoutine());
    }

    IEnumerator FireRoutine()
    {
        // 初始等待
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            // 1. 冷却休息阶段
            // 确保播放的是空状态或者第一帧（如果动画没设Loop，它播完会自动停在最后一帧，可能需要重置）
            // 这里建议加一个 "Idle" 的空状态，或者直接 Disable SpriteRenderer，简单起见我们只控制逻辑
            
            yield return new WaitForSeconds(cooldownTime);

            // 2. 喷火开始
            // 播放动画名字，你需要确认你的Animation Clip名字是不是叫 "Fire_Clip"
            // 或者用 Play("Fire_Clip", 0, 0) 从头播放
            anim.Play("Fire_Clip", 0, 0f); 

            // 3. 等待火苗变大
            yield return new WaitForSeconds(damageDelay);

            // 4. 开启伤害
            fireCollider.enabled = true;

            // 5. 伤害持续时间
            yield return new WaitForSeconds(damageDuration);

            // 6. 关闭伤害
            fireCollider.enabled = false;
        }
    }

    // 伤害判定 (和激光陷阱一样)
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IHurt hurt = other.GetComponent<IHurt>();
            if (hurt == null) hurt = other.GetComponentInParent<IHurt>();

            if (hurt != null)
            {
                hurt.Hurt(transform, damage);
            }
        }
    }
}