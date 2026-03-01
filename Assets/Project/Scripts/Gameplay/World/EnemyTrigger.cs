using UnityEngine;

public class EnemyTrigger : MonoBehaviour
{
    [Header("把需要激活的怪物/刷怪笼拖到这里")]
    public GameObject[] enemiesToEnable;

    [Header("设置")]
    public bool isOneTimeUse = true; // 是否是一次性的（通常是打完就不刷了）

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 只有玩家才能触发
        if (other.CompareTag("Player")) 
        {
            // 防止重复触发
            if (isOneTimeUse && hasTriggered) return;

            // 2. 激活列表里的所有东西
            foreach (GameObject obj in enemiesToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(true); // 这一步就像在Inspector里把那个勾勾打上
                }
            }

            hasTriggered = true;
            Debug.Log("战斗开始！怪物已激活！");
            
            // 可选：如果你希望触发后这个触发器就没用了，可以把下面这行解注
            // Destroy(gameObject); 
        }
    }
}
