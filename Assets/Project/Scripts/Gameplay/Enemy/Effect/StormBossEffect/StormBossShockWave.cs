using UnityEngine;

public class StormBossShockWave : MonoBehaviour
{
    private float speed;
    private int damage;
    private Vector3 direction;

    /// <summary>
    /// Initial Shockwave
    /// </summary>
    /// <param name="dir">Flight Direction(Vector3.left 或 Vector3.right)</param>
    public void Init(float moveSpeed, int dmg, Vector3 dir)
    {
        this.speed = moveSpeed;
        this.damage = dmg;
        this.direction = dir;

        if (direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);

        Destroy(gameObject, 3f);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<IHurt>()?.Hurt(transform, damage);

        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground") ||
                 other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }
    }
}