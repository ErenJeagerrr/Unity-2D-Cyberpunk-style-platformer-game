using UnityEngine;

public class HomingMissile : MonoBehaviour
{
    [Header("Missle Paramaters")]
    public float Speed = 10f;          
    public float TurnSpeed = 200f;    
    public float LifeTime = 5f;        
    public int Damage = 15;            

    [Header("Effect")]
    public GameObject ExplosionEffect;

    private Transform target;
    private Rigidbody2D rb;
    private bool isExploded = false;

    public void Init(Transform targetPlayer, int damage)
    {
        this.target = targetPlayer;
        this.Damage = damage;

        float randomAngle = Random.Range(-30f, 30f);
        transform.Rotate(0, 0, randomAngle);

        Destroy(gameObject, LifeTime);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (isExploded) return;

        rb.velocity = transform.right * Speed;

        if (target != null)
        {
            Vector2 direction = (Vector2)target.position - rb.position;
            direction.Normalize();

            float rotateAmount = Vector3.Cross(direction, transform.right).z;

            rb.angularVelocity = -rotateAmount * TurnSpeed;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isExploded) return;

        if (other.CompareTag("Player"))
        {
            other.GetComponent<IHurt>()?.Hurt(transform, Damage);
            Explode();
        }

        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Explode();
        }
    }

    void OnDestroy()
    {
        if (!isExploded)
        {
            ExplodeEffect();
        }
    }

    private void Explode()
    {
        isExploded = true;
        ExplodeEffect();

        if (AttackEffect.Instance != null) AttackEffect.Instance.Shake();

        Destroy(gameObject);
    }

    private void ExplodeEffect()
    {
        if (ExplosionEffect != null)
        {
            Instantiate(ExplosionEffect, transform.position, Quaternion.identity);
        }
        if (AudioService.Instance != null)
        {
            AudioService.Instance.PlayEffect("Boss_Explode");
        }
    }
}