using UnityEngine;

public class TrapBow : MonoBehaviour
{
    // 定义方向枚举
    public enum ShootDirection
    {
        Right,  // 向右
        Left,   // 向左
        Up,     // 向上
        Down    // 向下
    }

    [Header("方向设置")]
    public ShootDirection direction = ShootDirection.Right; // 默认向右，你可以改

    [Header("组件设置")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float interval = 2.5f;

    private void Start()
    {
        InvokeRepeating(nameof(Shoot), 1f, interval);
    }

    void Shoot()
    {
        if (arrowPrefab == null || firePoint == null) return;

        // 生成箭矢
        GameObject go = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
        
        IceArrow arrow = go.GetComponent<IceArrow>();
        if (arrow != null)
        {
            // 根据下拉菜单的选择，决定发射方向向量
            Vector2 dirVector = Vector2.right;

            switch (direction)
            {
                case ShootDirection.Right: dirVector = Vector2.right; break;
                case ShootDirection.Left:  dirVector = Vector2.left;  break;
                case ShootDirection.Up:    dirVector = Vector2.up;    break;
                case ShootDirection.Down:  dirVector = Vector2.down;  break;
            }

            // 把方向传给箭矢
            arrow.Init(dirVector);
        }
    }
}