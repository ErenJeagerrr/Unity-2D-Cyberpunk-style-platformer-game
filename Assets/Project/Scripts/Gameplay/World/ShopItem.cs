using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    [Header("商品设置")]
    public ShopItemType ItemType; 
    
    public int Price = 100;      
    public int Value = 20;       
    public string ItemName = "Item"; 

    [Header("提示信息")]
    public GameObject TipUI;      
    public Text InfoText;         

    private bool canBuy = false;  

    private void Start()
    {
        if (TipUI != null) TipUI.SetActive(false);
        
        if (InfoText != null)
        {
            InfoText.text = $"{ItemName}\n${Price}\n+{Value} Stat";
        }
    }

    private void Update()
    {
        if (canBuy && Input.GetKeyDown(KeyCode.E))
        {
            bool success = PlayerSystem.Instance.BuyItem(ItemType, Value, Price);
            
            if (success)
            {
                AudioService.Instance.PlayEffect("Button"); 
                if (TipUI != null) TipUI.SetActive(false);
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canBuy = true;
            if (TipUI != null) TipUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canBuy = false;
            if (TipUI != null) TipUI.SetActive(false);
        }
    }
}