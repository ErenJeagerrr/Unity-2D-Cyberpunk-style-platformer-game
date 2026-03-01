using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ShadeGround : MonoBehaviour
{
    [Header("时间设置")]
    public float ExistTime = 3f;       
    public float DisappearTime = 2f;   
    public float FadeDuration = 1f;    

    private Tilemap tilemap;
    private TilemapCollider2D tileCol; 

    private void Start()
    {
        tilemap = GetComponent<Tilemap>();
        
        tileCol = GetComponent<TilemapCollider2D>(); 

        if (tilemap == null || tileCol == null)
        {
            Debug.LogError("报错：物体 " + gameObject.name + " 缺少 Tilemap 或 TilemapCollider2D 组件！");
            return; 
        }

        StartCoroutine(CycleRoutine());
    }

    IEnumerator CycleRoutine()
    {
        while (true)
        {
            SetTilemapAlpha(1f);
            tileCol.enabled = true; 
            yield return new WaitForSeconds(ExistTime);

            float timer = 0;
            while (timer < FadeDuration)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, timer / FadeDuration);
                SetTilemapAlpha(alpha);
                yield return null;
            }

            SetTilemapAlpha(0f);
            tileCol.enabled = false;
            yield return new WaitForSeconds(DisappearTime);

            timer = 0;
            while (timer < FadeDuration)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, timer / FadeDuration);
                SetTilemapAlpha(alpha);
                yield return null;
            }
        }
    }

    private void SetTilemapAlpha(float alpha)
    {
        if (tilemap != null)
        {
            Color c = tilemap.color;
            c.a = alpha;
            tilemap.color = c;
        }
    }
}