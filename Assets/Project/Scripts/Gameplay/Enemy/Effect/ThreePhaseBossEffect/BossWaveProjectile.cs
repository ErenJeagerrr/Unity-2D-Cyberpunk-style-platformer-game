using UnityEngine;

public class BossWaveProjectile : MonoBehaviour
{
    private int damage;
    private float speed;
    private int dir;

    public void Init(int dir, float speed, int damage, float angleOffset = 0)
    {
        this.speed = speed;
        this.damage = damage;

        float yRotation = dir > 0 ? 0 : 180;

        float finalZ = (dir > 0) ? angleOffset : -angleOffset;

        transform.rotation = Quaternion.Euler(0, yRotation, finalZ);

        Destroy(gameObject, 5f);
    }
    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<IHurt>()?.Hurt(transform, damage);
            Destroy(gameObject);
        }
    }
}