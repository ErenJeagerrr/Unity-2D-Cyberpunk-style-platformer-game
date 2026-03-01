using UnityEngine;
using System.Collections;

public class BossSpike : MonoBehaviour
{
    public int Damage = 10;
    public float DelayTime = 1.0f;

    private SpriteRenderer sr;
    private BoxCollider2D col;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        col.enabled = false;

        // Red semi-transparent as warning
        sr.color = new Color(1, 0, 0, 0.5f);

        StartCoroutine(SpikeProcess());
    }

    IEnumerator SpikeProcess()
    {
        yield return new WaitForSeconds(DelayTime);

        sr.color = Color.white;
        col.enabled = true;

        if (AudioService.Instance != null)
        {
            AudioService.Instance.PlayEffect("Boss_Explode");
        }

        yield return new WaitForSeconds(0.5f);

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<IHurt>()?.Hurt(transform, Damage);
        }
    }
}