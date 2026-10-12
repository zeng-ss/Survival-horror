using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
public class CircleMarkerCreator : MonoBehaviour
{
    [MenuItem("Tools/创建红色圆圈")]
    static void CreateCircleMarker()
    {
        // 创建圆形标记游戏对象
        GameObject marker = new GameObject("CircleMarker");
        
        // 添加精灵渲染器
        SpriteRenderer spriteRenderer = marker.AddComponent<SpriteRenderer>();
        
        // 创建圆形精灵（或者使用现有的圆形图片）
        Texture2D circleTexture = CreateCircleTexture(256, Color.red);
        Sprite circleSprite = Sprite.Create(circleTexture, 
            new Rect(0, 0, circleTexture.width, circleTexture.height), 
            new Vector2(0.5f, 0.5f));
        
        spriteRenderer.sprite = circleSprite;
        spriteRenderer.sortingOrder = 1000;
        
        // 确保始终面向相机
        marker.AddComponent<Billboard>();
        
        // 保存为预制体
        PrefabUtility.SaveAsPrefabAsset(marker, "Assets/Prefabs/Effects/CircleMarker.prefab");
        DestroyImmediate(marker);
    }

    /// <summary>
    /// 创建圆形纹理
    /// </summary>
    static Texture2D CreateCircleTexture(int size, Color color)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = distance <= radius ? 1f - (distance / radius) : 0f;
                alpha = Mathf.Pow(alpha, 0.5f); // 让边缘更柔和
                
                pixels[y * size + x] = new Color(color.r, color.g, color.b, alpha);
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
}
#endif
/// <summary>
/// 轨迹线
/// </summary>
public class TrajectoryDrawer : MonoBehaviour
{
    [Header("基础配置")]
    private GameObject circleMarker;      // 圆形标记
    public LineRenderer trajectoryLine;   // 拖入LineRenderer组件
    public float trajectoryLength = 10f;  // 轨迹最大长度
    public int lineSegmentCount = 100;    // 分段数（曲线越高，更平滑）

    [Header("抛物线配置")]
    public float gravity = 9.8f;    // 重力系数（控制曲线弯曲程度）
    public float launchForce = 5f;  // 发射力度（影响曲线跨度）

    // 轨迹类型枚举（外部调用时指定）
    public enum TrajectoryType { Straight, Parabola }
    private TrajectoryType currentType;
    private bool isDrawing;

    void Awake()
    {
        circleMarker = GameObject.Find("CircleMarker");
        // 初始化隐藏
        if (trajectoryLine != null) trajectoryLine.enabled = false;
        if (circleMarker != null) circleMarker.SetActive(false);
    }

    /// <summary>
    /// 外部调用：开启轨迹绘制（指定类型）
    /// </summary>
    /// <param name="type">轨迹类型：直线/抛物线</param>
    public void StartDraw(TrajectoryType type)
    {
        if (trajectoryLine == null || circleMarker == null)
        {
            Debug.LogError("轨迹线或圆形标记未赋值！");
            return;
        }

        currentType = type;
        isDrawing = true;
        trajectoryLine.enabled = true;
        trajectoryLine.positionCount = lineSegmentCount;
    }

    /// <summary>
    /// 外部调用：关闭轨迹绘制
    /// </summary>
    public void StopDraw()
    {
        isDrawing = false;
        if (trajectoryLine != null) trajectoryLine.enabled = false;
        if (circleMarker != null) circleMarker.SetActive(false);
    }

    void Update()
    {
        if (isDrawing)
        {
            // 根据当前类型更新轨迹
            if (currentType == TrajectoryType.Straight)
                UpdateStraightTrajectory();
            else
                UpdateParabolaTrajectory();
        }
    }

    /// <summary>
    /// 直线轨迹更新（原逻辑优化）
    /// </summary>
    private void UpdateStraightTrajectory()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        // 分段绘制直线
        for (int i = 0; i < lineSegmentCount; i++)
        {
            float distance = (float)i / (lineSegmentCount - 1) * trajectoryLength;
            Vector3 point = ray.origin + ray.direction * distance;
            trajectoryLine.SetPosition(i, point);
        }

        // 碰撞标记
        UpdateCircleMarker(ray);
    }

    /// <summary>
    /// 抛物线轨迹更新（核心新增）
    /// </summary>
    private void UpdateParabolaTrajectory()
    {
        Vector3 launchDirection = transform.forward;
        RaycastHit hit = new RaycastHit(); // 关键修复：初始化RaycastHit
        bool hasHit = false;

        // 分段计算抛物线每个点的位置（考虑重力）
        for (int i = 0; i < lineSegmentCount; i++)
        {
            // 计算当前时间占比（0到1）
            float time = (float)i / (lineSegmentCount - 1) * trajectoryLength / launchForce;
            // 抛物线公式：位置 = 初始位置 + 方向*力度*时间 - 0.5*重力*时间²（Y轴向下）
            Vector3 point = transform.position 
                            + launchDirection * launchForce * time 
                            - new Vector3(0, 0.5f * gravity * time * time, 0);

            // 检测当前点到上一个点是否碰撞（避免穿透）
            if (i > 0 && !hasHit)
            {
                Vector3 prevPoint = trajectoryLine.GetPosition(i - 1);
                if (Physics.Linecast(prevPoint, point, out hit))
                {
                    point = hit.point;
                    hasHit = true;
                    // 后续点都设为碰撞点（避免超出碰撞体）
                    for (int j = i; j < lineSegmentCount; j++)
                        trajectoryLine.SetPosition(j, point);
                    break;
                }
            }

            trajectoryLine.SetPosition(i, point);
        }

        // 碰撞标记
        if (hasHit)
        {
            circleMarker.transform.position = hit.point;
            circleMarker.transform.LookAt(transform);
            circleMarker.SetActive(true);
        }
        else
        {
            circleMarker.SetActive(false);
        }
    }

    /// <summary>
    /// 通用：更新圆形碰撞标记
    /// </summary>
    private void UpdateCircleMarker(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, trajectoryLength))
        {
            circleMarker.transform.position = hit.point;
            circleMarker.transform.LookAt(transform);
            circleMarker.SetActive(true);
        }
        else { circleMarker.SetActive(false); }
    }
}

/// <summary>
/// 让标记始终面向相机
/// </summary>
public class Billboard : MonoBehaviour
{
    private Camera cam;
    void Start() { cam = GameObject.Find("CameraOne").GetComponent<Camera>();}
    void Update()
    {
        if (cam != null)
        {
            transform.rotation = cam.transform.rotation;
        }
    }
}
