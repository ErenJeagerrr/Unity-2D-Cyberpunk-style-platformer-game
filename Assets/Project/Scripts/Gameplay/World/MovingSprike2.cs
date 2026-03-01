using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSpike2 : MonoBehaviour
{
    public enum MoveType
    {
        Horizontal, // 水平往返
        Vertical,   // 垂直往返
        Rectangle,  // 长方形循环
        Diamond     // 菱形循环
    }

    [Header("外观设置")]
    public float RotateSpeed = 300f;

    [Header("移动设置")]
    public MoveType Type = MoveType.Horizontal;
    
    [Tooltip("水平/垂直模式的移动距离")]
    public float MoveDistance = 5f;

    [Tooltip("长方形或菱形模式的大小 (X=宽, Y=高)")]
    public Vector2 ShapeSize = new Vector2(4f, 4f);

    public float MoveSpeed = 3f;
    public float WaitTime = 0.5f;

    [Header("伤害设置")]
    public int Damage = 20;

    // 内部变量
    private List<Vector3> waypoints; // 存储所有路径点
    private int currentTargetIndex = 0; // 当前目标点的索引
    private bool isWaiting;
    private bool movingForward = true; // 用于水平/垂直往返判断方向

    private void Start()
    {
        // 初始化路径点
        CalculateWaypoints();
        
        // 如果有路径点，设置初始目标为下一个点（索引1）
        if (waypoints.Count > 1)
        {
            currentTargetIndex = 1;
            transform.position = waypoints[0]; // 强制让物体归位到起点
        }
    }

    private void Update()
    {
        transform.Rotate(0, 0, RotateSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (isWaiting || waypoints.Count < 2) return;

        // 获取当前目标位置
        Vector3 targetPos = waypoints[currentTargetIndex];

        // 移动
        transform.position = Vector3.MoveTowards(transform.position, targetPos, MoveSpeed * Time.fixedDeltaTime);

        // 检查是否到达目标
        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            StartCoroutine(WaitAndSwitch());
        }
    }

    // 计算路径点的方法（核心逻辑）
    void CalculateWaypoints()
    {
        waypoints = new List<Vector3>();
        Vector3 startPos = Application.isPlaying ? transform.position : transform.position; 
        // 注意：在Gizmos绘制时，transform.position是实时的；运行时以Start时的位置为准

        waypoints.Add(startPos); // 第一个点总是当前位置

        switch (Type)
        {
            case MoveType.Horizontal:
                waypoints.Add(startPos + Vector3.right * MoveDistance);
                break;

            case MoveType.Vertical:
                waypoints.Add(startPos + Vector3.up * MoveDistance);
                break;

            case MoveType.Rectangle:
                // 顺时针走长方形：起点 -> 右 -> 下 -> 左 -> 回到起点
                // 注意：这里假设起点是长方形的"左上角"
                waypoints.Add(startPos + Vector3.right * ShapeSize.x); // 右上
                waypoints.Add(startPos + Vector3.right * ShapeSize.x + Vector3.down * ShapeSize.y); // 右下
                waypoints.Add(startPos + Vector3.down * ShapeSize.y); // 左下
                // 最后会闭环回到 index 0
                break;

            case MoveType.Diamond:
                // 菱形走位：起点(顶) -> 右中 -> 底 -> 左中 -> 回到起点(顶)
                // 这里假设起点是菱形的"顶点"
                float halfW = ShapeSize.x / 2f;
                float halfH = ShapeSize.y / 2f;

                waypoints.Add(startPos + new Vector3(halfW, -halfH, 0)); // 右边中间
                waypoints.Add(startPos + new Vector3(0, -ShapeSize.y, 0)); // 底部顶点
                waypoints.Add(startPos + new Vector3(-halfW, -halfH, 0)); // 左边中间
                break;
        }
    }

    IEnumerator WaitAndSwitch()
    {
        isWaiting = true;
        yield return new WaitForSeconds(WaitTime);

        // 切换目标点逻辑
        if (Type == MoveType.Horizontal || Type == MoveType.Vertical)
        {
            // 往返模式 (Ping Pong)
            if (movingForward)
            {
                currentTargetIndex++;
                if (currentTargetIndex >= waypoints.Count)
                {
                    currentTargetIndex = waypoints.Count - 2; // 退回到前一个点
                    movingForward = false;
                }
            }
            else
            {
                currentTargetIndex--;
                if (currentTargetIndex < 0)
                {
                    currentTargetIndex = 1; // 前进到下一个点
                    movingForward = true;
                }
            }
        }
        else // Rectangle 或 Diamond
        {
            // 循环模式 (Loop)
            currentTargetIndex++;
            if (currentTargetIndex >= waypoints.Count)
            {
                currentTargetIndex = 0; // 回到起点
            }
        }

        isWaiting = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IHurt hurt = other.GetComponent<IHurt>();
            if (hurt != null)
            {
                hurt.Hurt(transform, Damage);
            }
        }
    }

    // 辅助线绘制，让你在不运行游戏时也能看到轨迹
    private void OnDrawGizmos()
    {
        // 只有在非运行时才实时计算，避免浪费性能，但能保证编辑器里拖动变量实时看到效果
        if (!Application.isPlaying)
        {
            CalculateWaypoints();
        }

        if (waypoints == null || waypoints.Count < 2) return;

        Gizmos.color = Color.yellow;

        // 绘制所有路径点之间的连线
        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 current = waypoints[i];
            Vector3 next = Vector3.zero;

            // 如果是循环模式（长方形/菱形），最后一点连回第一点
            if ((Type == MoveType.Rectangle || Type == MoveType.Diamond) && i == waypoints.Count - 1)
            {
                next = waypoints[0];
            }
            // 如果是往返模式，只画到终点
            else if (i < waypoints.Count - 1)
            {
                next = waypoints[i + 1];
            }
            else
            {
                continue; 
            }

            Gizmos.DrawLine(current, next);
            Gizmos.DrawWireSphere(current, 0.2f);
        }
    }
}