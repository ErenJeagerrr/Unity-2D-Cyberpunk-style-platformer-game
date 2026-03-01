using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float fireInterval = 5f;

    void Start()
    {
        InvokeRepeating(nameof(Fire), 0f, fireInterval);
    }

    void Fire()
    {
        Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
    }
}
