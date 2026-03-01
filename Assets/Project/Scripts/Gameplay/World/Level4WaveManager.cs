using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level4WaveManager : MonoBehaviour
{
    [Header("生成设置")]
    public List<Transform> Portals;      
    public List<GameObject> EnemyPrefabs; 
    public int TotalCount = 10;          
    public float SpawnInterval = 5.0f; 
    [Header("特效(可选)")]
    public GameObject SpawnEffect;

    private IEnumerator Start()
    {
        // 等待 LevelSystem 初始化
        while (LevelSystem.Instance == null)
        {
            yield return null;
        }

        yield return null;

        // 告诉系统开始刷怪了，把门锁上
        LevelSystem.Instance.StartSpawning(); // 使用新加的方法更安全
        // 或者保留你原来的: LevelSystem.Instance.IsSpawning = true; 

        StartCoroutine(IESpawnEnemies());
    }

    private IEnumerator IESpawnEnemies()
    {
        for (int i = 0; i < TotalCount; i++)
        {
            if (LevelSystem.Instance == null)
            {
                Debug.LogWarning("LevelSystem.Instance 已被销毁，停止刷怪");
                yield break;
            }

            // 【重点修改】删掉了原本检查 "Level3" 的代码
            // 只要场景没切换，就继续刷
            if (!ResService.Instance.CurrentSceneName.Contains("Level")) 
            {
                Debug.Log("不在战斗场景中，停止刷怪");
                yield break; 
            }

            // 确保状态一直是刷怪中
            LevelSystem.Instance.IsSpawning = true;

            SpawnOneEnemy();

            yield return new WaitForSeconds(SpawnInterval);
        }

        // 所有怪生成完毕
        if (LevelSystem.Instance != null)
        {
            
            LevelSystem.Instance.FinishSpawning(); 
        }
    }

    private void SpawnOneEnemy()
    {
        if (Portals.Count == 0 || EnemyPrefabs.Count == 0)
        {
            Debug.LogWarning("请检查 Portals (生成点) 或 EnemyPrefabs (怪物预制体) 是否为空！");
            return;
        }

        // 随机选一个门
        int portalIndex = Random.Range(0, Portals.Count);
        Transform spawnPos = Portals[portalIndex];

        // 随机选一种怪
        int enemyIndex = Random.Range(0, EnemyPrefabs.Count);
        GameObject prefab = EnemyPrefabs[enemyIndex];

        Instantiate(prefab, spawnPos.position, Quaternion.identity);

        if (SpawnEffect != null)
        {
            Instantiate(SpawnEffect, spawnPos.position, Quaternion.identity);
        }

        // 强制刷新UI计数
        if (LevelSystem.Instance != null)
        {
            LevelSystem.Instance.CheckIsHaveEnemy();
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}