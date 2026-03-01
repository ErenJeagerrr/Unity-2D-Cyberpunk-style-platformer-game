using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : TriggerBase
{
    [Header("Chest Settings")]
    public GameObject CoinPrefab;
    public int CoinCount = 5;
    public float DropRadius = 0.5f;

    [Header("Money Reward")]
    public int MoneyAmount = 50;

    [Header("UI")]
    public GameObject TipObject;  

    private bool isOpened = false;

    private void Start()
    {
        if (CheckCenterPos == null)
        {
            CheckCenterPos = transform;
        }

        if (TipObject != null)
            TipObject.SetActive(false);
    }

    public override void EnterRandius()
    {
        if (!isOpened && TipObject != null)
        {
            TipObject.SetActive(true);
        }
    }
    public override void ExitRadius()
    {
        if (TipObject != null)
        {
            TipObject.SetActive(false);
        }
    }

    public override void TriggerEvent()
    {
        if (isOpened)
            return;

        base.TriggerEvent();
        OpenChest();
    }

    private void OpenChest()
    {
        isOpened = true;

        if (TipObject != null)
            TipObject.SetActive(false);

        PlayerSystem.Instance.CoinCount += MoneyAmount;

        DropCoins();

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject);
    }

    private void DropCoins()
    {
        if (CoinPrefab == null)
        {
            Debug.LogWarning("Coin Prefab not assigned!");
            return;
        }

        Coin coinComponent = CoinPrefab.GetComponent<Coin>();
        if (coinComponent == null)
        {
            Debug.LogError("CoinPrefab must have a Coin component! Current prefab: " + CoinPrefab.name);
            return;
        }

        for (int i = 0; i < CoinCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * DropRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);

            GameObject coin = Instantiate(CoinPrefab, spawnPos, Quaternion.identity);

            Coin coinScript = coin.GetComponent<Coin>();
            if (coinScript != null)
            {
                coinScript.Init();
            }
        }
    }
}
