using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendulumTrap : MonoBehaviour
{
    [Header("摆动设置")]
    [Tooltip("摆动的最大角度")]
    public float SwingAngle = 45f; // 左右各摆动多少度
    
    [Tooltip("摆动速度")]
    public float Speed = 2f;

    [Tooltip("初始偏移(让不同的陷阱不同步)")]
    public float TimeOffset = 0f;

    [Header("伤害设置")]
    public int Damage = 20;

    // 初始的角度（通常是0，即垂直向下或向上）
    private Quaternion startRotation;

    private void Start()
    {
        // 记录游戏开始时陷阱的初始朝向
        startRotation = transform.rotation;
    }

    private void Update()
    {
        Swing();
    }

    void Swing()
    {
        // 核心公式：利用 Sin 函数产生 -1 到 1 之间的数值
        // Time.time * Speed 决定摆动频率
        // + TimeOffset 决定起始相位（用于错开多个陷阱）
        float angle = Mathf.Sin((Time.time + TimeOffset) * Speed) * SwingAngle;

        // 将计算出的角度应用到 Z 轴旋转上
        // startRotation.eulerAngles.z 保证我们可以把陷阱斜着放，它依然会以当前角度为中心摆动
        transform.rotation = Quaternion.Euler(0, 0, startRotation.eulerAngles.z + angle);
    }

    // 碰撞检测逻辑
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 获取之前定义的扣血接口
            IHurt hurt = other.GetComponent<IHurt>();
            if (hurt != null)
            {
                // 传入当前物体的位置 transform，方便计算击退方向
                hurt.Hurt(transform, Damage);
            }
        }
    }

    // (可选) 在编辑器里画出摆动轨迹，方便摆放
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            // 模拟画出左右极限位置
            Gizmos.color = Color.yellow;
            
            // 计算当前基准角度
            float baseAngle = transform.eulerAngles.z;
            
            // 画左极限
            Vector3 leftDir = Quaternion.Euler(0, 0, baseAngle + SwingAngle) * Vector3.up; // 假设矛是朝上的
            // 注意：如果你的图片默认是朝上的，用 Vector3.up；如果是朝下的，用 Vector3.down
            // 根据你的图，看起来是竖直的，默认高度算它长度为3
            Gizmos.DrawLine(transform.position, transform.position + leftDir * 3f);

            // 画右极限
            Vector3 rightDir = Quaternion.Euler(0, 0, baseAngle - SwingAngle) * Vector3.up;
            Gizmos.DrawLine(transform.position, transform.position + rightDir * 3f);
        }
    }
}