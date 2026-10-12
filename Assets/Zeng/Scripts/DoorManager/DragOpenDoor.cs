using UnityEngine;
using DG.Tweening;

public class DragOpenDoor : MonoBehaviour
{
    private PlayerController player;
    [Header("门配置")]
    public Transform doorTransform;         // 门的旋转根节点（可直接拖自身）
    public Vector3 rotateAxis = Vector3.up; // 门的旋转轴（默认绕Y轴转）
    public float maxOpenAngle = 90f;        // 最大开门角度（如90度）
    private readonly float dragSensitivity = 100f;    // 拖动灵敏度（值越大，拖动越灵敏）

    public bool isDragging;    // 是否正在拖动
    private float currentAngle; // 当前门的旋转角度
    private float lastMouseX;   // 上一帧鼠标X坐标
    private Tween rotateTween;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        // 初始化：如果没指定doorTransform，默认用自身
        if (doorTransform == null) doorTransform = transform;
        // 记录初始角度（避免门初始旋转影响）
        currentAngle = doorTransform.localEulerAngles.y;
    }
    void Update()
    {
        // 开始拖动：面对门且按下鼠标
        if (!isDragging && player.isToDoor && Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMouseX = Input.mousePosition.x;
        }

        if (isDragging && player.currentDragDoor == this) { DragDoor(); }
        
        // 停止拖动：鼠标抬起
        if (isDragging && Input.GetMouseButtonUp(0)) { isDragging = false; }
    }
    
    /// <summary>
    /// 拖门的方法 
    /// </summary>
    private void DragDoor()
    {
        if (!isDragging || player.currentDragDoor != this) return;
        // 计算鼠标水平偏移（X轴）
        float mouseDeltaX = Input.mousePosition.x - lastMouseX;
        // 更新上一帧鼠标位置
        lastMouseX = Input.mousePosition.x;

        // 偏移量转换为旋转角度（右拖→正角度，左拖→负角度）
        currentAngle += mouseDeltaX * dragSensitivity;
        // 限制旋转角度在 0 ~ maxOpenAngle 之间（避免开门过度/反向）
        currentAngle = Mathf.Clamp(currentAngle, 0, maxOpenAngle);
        
        // 使用DOTween平滑旋转 ,先停止之前的动画
        if (rotateTween != null && rotateTween.IsActive()) { rotateTween.Kill(); }
        rotateTween = doorTransform.DOLocalRotate(
            new Vector3(doorTransform.localEulerAngles.x, currentAngle, doorTransform.localEulerAngles.z), 0.7f).SetEase(Ease.OutQuad);
    }
    
}
