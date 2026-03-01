using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnArea : MonoBehaviour
{
    private Player player;
    private bool isActive = false;
    private HashSet<Enemy> burnedEnemies = new HashSet<Enemy>();
    private Coroutine burnCoroutine;
    private CircleCollider2D areaCollider;

    private void Awake()
    {
        player = GetComponentInParent<Player>();

        areaCollider = GetComponent<CircleCollider2D>();
        if (areaCollider == null)
        {
            areaCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        areaCollider.isTrigger = true;
    }

    private void Start()
    {
        if (player != null)
        {
            areaCollider.radius = player.burnAreaRadius;
        }
    }

    public void Activate()
    {
        if (isActive || player == null)
            return;

        isActive = true;
        burnedEnemies.Clear();

        if (burnCoroutine != null)
        {
            StopCoroutine(burnCoroutine);
        }
        burnCoroutine = StartCoroutine(DelayedActivation());
    }

    private IEnumerator DelayedActivation()
    {
        yield return new WaitForSeconds(player.burnActivationDelay);

        CheckAndApplyBurn();

        Deactivate();
    }

    private void CheckAndApplyBurn()
    {
        if (player == null || !isActive) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, player.burnAreaRadius);

        int newIgnitions = 0;
        foreach (var hit in hits)
        {
            IHurt hurt = hit.GetComponent<IHurt>();
            if (hurt != null && hit.tag == "Enemy")
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null && !enemy.IsDie && !burnedEnemies.Contains(enemy))
                {
                    ApplyBurnDebuff(enemy);
                    burnedEnemies.Add(enemy);
                    newIgnitions++;
                }
            }
        }
    }

    private void ApplyBurnDebuff(Enemy enemy)
    {
        BurnDebuff existingBurn = enemy.GetComponent<BurnDebuff>();
        if (existingBurn != null)
        {
            return;
        }

        BurnDebuff burnDebuff = enemy.gameObject.AddComponent<BurnDebuff>();
        burnDebuff.Init(
            enemy,
            player.burnInitialDamage,
            player.burnDOTDamage,
            player.burnTickInterval,
            player.burnDebuffDuration,
            transform
        );
    }

    private void Deactivate()
    {
        isActive = false;
        burnedEnemies.Clear();
    }

    private void OnDestroy()
    {
        if (burnCoroutine != null)
        {
            StopCoroutine(burnCoroutine);
        }
        burnedEnemies.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, player.burnAreaRadius);
        }
        else
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, 2f);
        }
    }
}