using UnityEngine;
using System;
using System.Collections.Generic;

public class BossDashProjectile : MonoBehaviour
{
    private float speed;
    private int damage;
    private Vector3 direction;
    private float maxDistance;
    private Vector3 startPos;
    private Action onComplete;
    private bool isFinished = false;

    private float checkWidth = 1.0f;

    // Preventing repeated injuries
    private List<GameObject> hitList = new List<GameObject>();

    // --- Trailing Effect Parameters ---
    private SpriteRenderer sr;
    private float ghostTimer = 0;
    private float ghostInterval = 0.03f; 
    private float ghostLifeTime = 0.35f; 

    public void Init(Vector3 dir, float spd, int dmg, float dist, float width, Action onFinish)
    {
        this.direction = dir;
        this.speed = spd;
        this.damage = dmg;
        this.maxDistance = dist;
        this.checkWidth = width;
        this.onComplete = onFinish;
        this.startPos = transform.position;

        this.sr = GetComponent<SpriteRenderer>();

        if (this.sr != null)
        {
            Color shadowBodyColor = this.sr.color;
            shadowBodyColor.a = 0.7f;
            this.sr.color = shadowBodyColor;
        }

        // --- Orientation Correction ---
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Fixing the inverted display issue
        Vector3 scale = transform.localScale;
        if (Mathf.Abs(angle) > 90) scale.y = -Mathf.Abs(scale.y);
        else scale.y = Mathf.Abs(scale.y);
        transform.localScale = scale;

        Destroy(gameObject, 3f);
    }

    void Update()
    {
        if (isFinished) return;

        HandleGhostEffect();

        float moveStep = speed * Time.deltaTime;

        //  物理检测 (射线检测防穿模)
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            transform.position,
            new Vector2(0.1f, checkWidth),
            transform.eulerAngles.z,
            direction,
            moveStep,
            LayerMask.GetMask("Player")
        );

        foreach (var hit in hits)
        {
            if (hit.collider != null && !hitList.Contains(hit.collider.gameObject))
            {
                IHurt hurt = hit.collider.GetComponent<IHurt>();
                if (hurt != null)
                {
                    hurt.Hurt(transform, damage);
                    hitList.Add(hit.collider.gameObject);

                    if (AttackEffect.Instance != null)
                    {
                        AttackEffect.Instance.Shake();
                        AttackEffect.Instance.StopFrame(9);
                    }
                }
            }
        }

        transform.position += direction * moveStep;

        if (Vector3.Distance(startPos, transform.position) >= maxDistance)
        {
            Finish();
        }
    }

    private void HandleGhostEffect()
    {
        if (sr == null) return;

        ghostTimer += Time.deltaTime;
        if (ghostTimer >= ghostInterval)
        {
            CreateGhost();
            ghostTimer = 0;
        }
    }

    private void CreateGhost()
    {
        GameObject ghost = new GameObject("DashGhost");

        ghost.transform.position = transform.position;
        ghost.transform.rotation = transform.rotation;
        ghost.transform.localScale = transform.localScale;

        SpriteRenderer ghostSr = ghost.AddComponent<SpriteRenderer>();
        ghostSr.sprite = sr.sprite;
        ghostSr.color = sr.color;

        ghostSr.sortingLayerName = sr.sortingLayerName;
        ghostSr.sortingOrder = sr.sortingOrder - 1;

        GhostTrail trail = ghost.AddComponent<GhostTrail>();
        trail.LifeTime = ghostLifeTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isFinished) return;

        if (other.CompareTag("Player") && !hitList.Contains(other.gameObject))
        {
            other.GetComponent<IHurt>()?.Hurt(transform, damage);
            hitList.Add(other.gameObject);

            if (AttackEffect.Instance != null)
            {
                AttackEffect.Instance.Shake();
                AttackEffect.Instance.StopFrame(9);
            }
        }
    }

    private void Finish()
    {
        if (isFinished) return;
        isFinished = true;
        onComplete?.Invoke();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (!isFinished) onComplete?.Invoke();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, checkWidth / 2);
    }
}