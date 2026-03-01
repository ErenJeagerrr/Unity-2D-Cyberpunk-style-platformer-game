using UnityEngine;

public class WarningFade : MonoBehaviour
{
    public float Duration = 0.5f;
    public float TargetAlpha = 0.5f;

    private SpriteRenderer sr;
    private float timer;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        Color c = sr.color;
        c.a = 0;
        sr.color = c;
    }

    void Update()
    {
        if (timer < Duration)
        {
            timer += Time.deltaTime;

            float progress = timer / Duration;

            float currentAlpha = Mathf.Lerp(0f, TargetAlpha, progress);

            Color c = sr.color;
            c.a = currentAlpha;
            sr.color = c;
        }
    }
}