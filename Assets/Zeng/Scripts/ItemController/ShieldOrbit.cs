using UnityEngine;

/// <summary>
/// 护盾独立旋转跟随脚本（挂载在护盾实例上）
/// </summary>
public class ShieldOrbit : MonoBehaviour
{
    public Transform target; // 跟随的目标（角色）
    public float orbitRadius = 0.5f; // 旋转半径
    public float orbitSpeed = 240f; // 旋转速度（度/秒）
    public float initialAngle; // 初始角度偏移（区分三个护盾）
    private float currentAngle;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("未指定护盾跟随的角色目标！");
            Destroy(gameObject);
            return;
        }

        // 初始化角度（转为弧度）
        currentAngle = initialAngle * Mathf.Deg2Rad;
        // 设置初始位置
        UpdatePosition();
    }

    void Update()
    {
        if (target == null) return;

        // 每帧更新旋转角度
        currentAngle += orbitSpeed * Time.deltaTime * Mathf.Deg2Rad;
        // 循环角度（避免数值过大）
        if (currentAngle > Mathf.PI * 2) currentAngle -= Mathf.PI * 2;

        // 更新位置（跟随角色+旋转）
        UpdatePosition();
    }

    /// <summary>
    /// 计算并更新护盾位置
    /// </summary>
    private void UpdatePosition()
    {
        float x = Mathf.Cos(currentAngle) * orbitRadius;
        float z = Mathf.Sin(currentAngle) * orbitRadius;
        // 跟随角色位置，保持高度1f
        transform.position = target.position + new Vector3(x, 1f, z);
    }
}