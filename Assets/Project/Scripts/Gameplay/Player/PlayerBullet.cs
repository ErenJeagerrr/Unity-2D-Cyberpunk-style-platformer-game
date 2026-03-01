using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    private int damage;

    public void Init(Vector2 dir, int damage, float speed = 10f, float lifetime = 1f)
    {
        this.damage = damage;
        transform.right = dir;
        GetComponent<Rigidbody2D>().velocity = dir.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        IHurt hurt = other.GetComponent<IHurt>();
        if (hurt != null && other.tag == "Enemy")
        {
            hurt.Hurt(transform, damage);
            Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}