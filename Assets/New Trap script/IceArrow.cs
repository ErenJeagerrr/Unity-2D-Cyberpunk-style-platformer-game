using UnityEngine;

public class IceArrow : MonoBehaviour
{
    [Header("箭矢属性")]
    public float speed = 10f;
    public float lifeTime = 5f;
    public int damage = 15;

    [Header("减速设置")]
    [Range(0.1f, 0.9f)]
    public float slowPercentage = 0.5f;
    public float slowDuration = 3.0f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 dir)
    {
        // 1. 设置速度方向
        rb.velocity = dir.normalized * speed;

        // 2. 计算旋转角度，让箭头指向飞行方向
        // Atan2 可以根据向量计算出角度
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        
        // 3. 应用旋转
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(gameObject, lifeTime);
    }

    // ... 下面的 OnTriggerEnter2D 保持不变 ...
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.Hurt(transform, damage);

                // 动态挂载减速脚本
                PlayerSlowEffect existingDebuff = player.GetComponent<PlayerSlowEffect>();
                if (existingDebuff != null)
                {
                    existingDebuff.RefreshDuration(slowDuration);
                }
                else
                {
                    PlayerSlowEffect newDebuff = player.gameObject.AddComponent<PlayerSlowEffect>();
                    newDebuff.Init(player, slowDuration, slowPercentage);
                }
            }
            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || 
                 collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }
    }
}