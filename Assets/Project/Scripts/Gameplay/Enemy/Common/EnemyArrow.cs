using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArrow : MonoBehaviour
{
    private int Attack;//攻击力
    public void Init(Vector2 Dir, int Attack)
    {
        this.Attack = Attack;
        transform.right = Dir;
        GetComponent<Rigidbody2D>().velocity = Dir.normalized * 10;
        Destroy(gameObject, 3);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        IHurt hurt = other.GetComponent<IHurt>();
        if (hurt != null && other.tag != "Enemy")
        {
            //如果攻击到敌人，造成伤害，销毁自身
            hurt.Hurt(transform, Attack);
            Destroy(gameObject);
        }
    }
}
