using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnDebuff : MonoBehaviour
{
    private Enemy enemy;
    private float initialDamage;
    private float dotDamage;
    private float tickInterval;
    private float duration;
    private float elapsed = 0f;
    private float tickTimer = 0f;
    private Transform damageSource;
    private bool hasDealtInitialDamage = false;

    public void Init(Enemy target, float initial, float dot, float interval, float totalDuration, Transform source)
    {
        enemy = target;
        initialDamage = initial;
        dotDamage = dot;
        tickInterval = interval;
        duration = totalDuration;
        damageSource = source;

        Debug.Log($"<color=orange>{enemy.gameObject.name} is ignited! Initial: {initialDamage}, DOT: {dotDamage} every {tickInterval}s for {duration}s</color>");

        DealInitialDamage();
    }

    private void Update()
    {
        if (enemy == null || enemy.IsDie)
        {
            Destroy(this);
            return;
        }

        elapsed += Time.deltaTime;
        tickTimer += Time.deltaTime;

        if (tickTimer >= tickInterval)
        {
            DealDOTDamage();
            tickTimer = 0f;
        }

        if (elapsed >= duration)
        {
            Debug.Log($"<color=cyan>{enemy.gameObject.name} burn effect ended</color>");
            Destroy(this);
        }
    }

    private void DealInitialDamage()
    {
        if (enemy != null && !enemy.IsDie && !hasDealtInitialDamage)
        {
            enemy.Hurt(damageSource, initialDamage);
            hasDealtInitialDamage = true;
            Debug.Log($"<color=red>{enemy.gameObject.name} takes {initialDamage} initial burn damage!</color>");
        }
    }

    private void DealDOTDamage()
    {
        if (enemy != null && !enemy.IsDie)
        {
            enemy.Hurt(damageSource, dotDamage);
            Debug.Log($"<color=orange>{enemy.gameObject.name} takes {dotDamage} DOT damage</color>");
        }
    }
}