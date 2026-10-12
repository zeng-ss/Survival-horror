using UnityEngine;

public class DiaryController : ItemController
{
    new void Start()
    {
        base.Start();
    }

    new void Update()
    {
        UpdateVignetteCenter();
    }
    
    /// <summary>
    /// 更新后处理随着鼠标位置变化
    /// </summary>
    private void UpdateVignetteCenter()
    {
        if (cameraController.vignette == null || !isStartPost) return;
        // 将鼠标位置转换为标准化坐标 (0-1)
        Vector2 mouseViewportPos = new Vector2(
            Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height
        );
        // 设置 Vignette 中心跟随鼠标
        cameraController.vignette.center.Override(mouseViewportPos);
    }
    
}
