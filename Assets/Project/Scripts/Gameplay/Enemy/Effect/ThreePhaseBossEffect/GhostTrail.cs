using UnityEngine;


public class GhostTrail : MonoBehaviour
{
    public float LifeTime = 0.35f;

    private SpriteRenderer sr;
    private Color startColor;
    private float timer;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            startColor = sr.color;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        float progress = timer / LifeTime;

        if (sr != null)
        {
            Color c = startColor;
            c.a = Mathf.Lerp(startColor.a, 0, progress);
            sr.color = c;
        }

        if (timer >= LifeTime)
        {
            Destroy(gameObject);
        }
    }
}