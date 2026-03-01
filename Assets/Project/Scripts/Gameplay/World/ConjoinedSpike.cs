using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConjoinedSpike : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private int lifeCost = 20;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            other.GetComponent<IHurt>()?.Hurt(transform, lifeCost);
        }
    }
}