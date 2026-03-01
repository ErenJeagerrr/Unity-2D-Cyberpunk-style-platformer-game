using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpBase : MonoBehaviour
{
    private Rigidbody2D rig;
    public float Radius;
    private bool IsInit;
    private float timer;
    public void Init()
    {
        rig = GetComponent<Rigidbody2D>();
        float RamdomForce = Random.Range(-20, 20);
        rig.AddForce(new Vector2(RamdomForce, 100), ForceMode2D.Impulse);
        timer = 0;
        IsInit = true;
    }
    private void Update()
    {
        if (!IsInit)
            return;
        timer += Time.deltaTime;
        if (timer <= 0.2f)
            return;
        Collider2D collider = Physics2D.OverlapCircle(transform.position, Radius, LayerMask.GetMask("Player"));
        if (collider != null)
        {
            Trigger();
        }
    }
    public virtual void Trigger()
    {
        Destroy(gameObject);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
}
