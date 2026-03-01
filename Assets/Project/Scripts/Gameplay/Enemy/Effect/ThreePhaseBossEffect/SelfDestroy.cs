using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    [Header("Destroy after x seconds")]
    public float Delay = 1f;

    void Start()
    {
        Destroy(gameObject, Delay);
    }
}