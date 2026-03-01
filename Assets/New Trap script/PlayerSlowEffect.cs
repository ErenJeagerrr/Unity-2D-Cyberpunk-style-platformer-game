using UnityEngine;

// 这个脚本负责管理减速逻辑 (修正版)
public class PlayerSlowEffect : MonoBehaviour
{
    private Player targetPlayer; // 仅用来检测玩家是否还活着
    private float timer;

    // 初始化方法
    public void Init(Player player, float duration, float slowPercent)
    {
        this.targetPlayer = player;
        this.timer = duration;

        // 1. 获取当前速度 (修改点：从 PlayerSystem 获取，而不是 Player)
        // 你的项目中，数据都存在 PlayerSystem 这个单例里
        if (PlayerSystem.Instance == null) return;

        int currentSpeed = PlayerSystem.Instance.MoveSpeed;
        
        // 2. 计算减速后的数值
        int slowedSpeed = (int)(currentSpeed * (1.0f - slowPercent));
        
        // 保证速度最少为 1，防止定身
        if (slowedSpeed < 1) slowedSpeed = 1;

        // 3. 修改 PlayerSystem 的属性
        PlayerSystem.Instance.MoveSpeed = slowedSpeed;
        
        Debug.Log($"<color=cyan>玩家中箭！速度从 {currentSpeed} 降低至: {slowedSpeed}</color>");
    }

    // 用于刷新时间（如果玩家还没恢复又中了一箭）
    public void RefreshDuration(float newDuration)
    {
        timer = newDuration;
        Debug.Log("减速时间刷新！");
    }

    private void Update()
    {
        // 如果玩家对象没了（比如死了销毁了），这个脚本也该停了
        if (targetPlayer == null) 
        {
            Destroy(this);
            return;
        }

        // 计时
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            RemoveDebuff();
        }
    }

    private void RemoveDebuff()
    {
        // 4. 恢复数据 (修改点：调用 PlayerSystem 的 ReCalculateData)
        // 这个方法会根据装备重新计算出正确的满状态速度，正好用来清除减速效果
        if (PlayerSystem.Instance != null)
        {
            PlayerSystem.Instance.ReCalculateData();
            Debug.Log("<color=green>减速效果结束，速度已恢复。</color>");
        }

        // 5. 销毁这个临时组件
        Destroy(this);
    }
}