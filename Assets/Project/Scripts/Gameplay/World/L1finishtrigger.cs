using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1FinishTrigger : MonoBehaviour
{
    private bool hasTriggered = false; 

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (LevelSystem.Instance.CurrentLevel != 1)
        {
            return;
        }

        if (collision.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;

            LevelSystem.Instance.WinLevel();
        }
    }

    private void OnEnable()
    {
        hasTriggered = false;
    }
}