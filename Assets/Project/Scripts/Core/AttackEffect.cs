using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class AttackEffect : MonoBehaviour
{
    public static AttackEffect Instance;
    private CinemachineImpulseSource ShakeSource;
    private void Awake()
    {
        if (AttackEffect.Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        ShakeSource = GetComponent<CinemachineImpulseSource>();
    }
    public void Shake()
    {
        ShakeSource.GenerateImpulse();
    }
    public void StopFrame(int delay)
    {
        StartCoroutine(IEStopTime(delay));
    }
    private IEnumerator IEStopTime(int delay)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(delay / 60.0f);
        Time.timeScale = 1;
    }
}
