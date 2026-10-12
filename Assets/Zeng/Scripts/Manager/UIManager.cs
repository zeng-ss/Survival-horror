using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : UnitySingleTonMono<UIManager>
{
    // 原有面板管理字典
    public Dictionary<string, BasePanel> UIPanelDict = new();
    // 新增：追踪需要后处理的面板
    private HashSet<string> postProcessingPanels = new();
    // Canvas相关
    [HideInInspector] public RectTransform Canvas;
    private Canvas canvasComponent;

    // 面板容器
    private GameObject currentPanel;

    // 相机引用
    private Camera mainCamera;
    private Camera cameraOne;

    /// <summary>
    /// 打开面板
    /// </summary>
    public T openPanel<T>(bool needPostProcessing = false) where T : BasePanel
    {
        // 更新相机引用并设置Canvas模式
        UpdateCameraReferences();
        string panelName = typeof(T).Name;
        
        // 记录后处理面板
        if (needPostProcessing)
        {
            postProcessingPanels.Add(panelName);
            SetCanvasMode(true);
        }
        else
        {
            // 如果不是后处理面板，检查是否还有其他后处理面板打开
            if (postProcessingPanels.Count == 0) { SetCanvasMode(false); }
        }

        // 初始化当前面板容器
        if (currentPanel == null)
        {
            currentPanel = new GameObject("currentShowPanel");
            currentPanel.transform.SetParent(Canvas);
            RectTransform rect = currentPanel.AddComponent<RectTransform>();
            
            // 设置currentPanel为全屏，但不干扰子面板的布局
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
        }

        //string panelName = typeof(T).Name;
        if (UIPanelDict.ContainsKey(panelName))
        {
            UIPanelDict[panelName].Show();
            UIPanelDict[panelName].transform.SetParent(currentPanel.transform);
            
            // 确保面板位置正确
            RectTransform panelRect = UIPanelDict[panelName].GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.localPosition = Vector3.zero;
                panelRect.localRotation = Quaternion.identity;
            }
            
            return UIPanelDict[panelName] as T;
        }
        else
        {
            GameObject panelObj = Resources.Load<GameObject>("UI/UIPanel/" + panelName);
            if (panelObj == null)
            {
                Debug.LogError($"无法加载面板预制体: UI/UIPanel/{panelName}");
                return null;
            }

            // 保存预制体的原始设置
            RectTransform prefabRect = panelObj.GetComponent<RectTransform>();
            Vector2 originalAnchorMin = prefabRect.anchorMin;
            Vector2 originalAnchorMax = prefabRect.anchorMax;
            Vector2 originalSizeDelta = prefabRect.sizeDelta;
            Vector2 originalPivot = prefabRect.pivot;
            Vector3 originalAnchoredPos = prefabRect.anchoredPosition3D;

            panelObj = Instantiate(panelObj, currentPanel.transform);
            
            // 恢复预制体的原始设置
            RectTransform newRect = panelObj.GetComponent<RectTransform>();
            if (newRect != null)
            {
                newRect.anchorMin = originalAnchorMin;
                newRect.anchorMax = originalAnchorMax;
                newRect.sizeDelta = originalSizeDelta;
                newRect.pivot = originalPivot;
                newRect.anchoredPosition3D = originalAnchoredPos;
                newRect.localRotation = Quaternion.identity;
                newRect.localScale = Vector3.one;
            }

            BasePanel panel = panelObj.GetComponent<T>();
            if (panel == null)
            {
                Debug.LogError($"面板 {panelName} 没有对应的BasePanel组件");
                Destroy(panelObj);
                return null;
            }
            panelObj.name = panelName;
            UIPanelDict.Add(panelName, panel);
            return panel as T;
        }
    }

    /// <summary>
    /// 更新相机引用
    /// </summary>
    private void UpdateCameraReferences()
    {
        if (mainCamera == null || !mainCamera.gameObject.activeInHierarchy)
        {
            mainCamera = GameObject.Find("MainCamera")?.GetComponent<Camera>();
        }

        if (cameraOne == null || !cameraOne.gameObject.activeInHierarchy)
        {
            cameraOne = GameObject.Find("CameraOne")?.GetComponent<Camera>();
        }
    }

    /// <summary>
    /// 切换Canvas模式（修复版）
    /// </summary>
    private void SetCanvasMode(bool needPostProcessing)
    {
        if (canvasComponent == null) return;
        if (needPostProcessing && cameraOne != null)
        {
            // 后处理模式：ScreenSpaceCamera
            canvasComponent.renderMode = RenderMode.ScreenSpaceCamera;
            canvasComponent.worldCamera = cameraOne;
            canvasComponent.planeDistance = 0.32f;
            
            // 关键：不要手动设置Canvas的transform，Unity会自动处理
            //Debug.Log($"切换到后处理模式，使用相机: {cameraOne.name}");
        }
        else
        {
            // 普通模式：ScreenSpaceOverlay
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasComponent.worldCamera = null;
            
            // 确保Canvas在Overlay模式下位置正确
            if (Canvas != null)
            {
                Canvas.transform.position = Vector3.zero;
                Canvas.transform.rotation = Quaternion.identity;
                Canvas.transform.localScale = Vector3.one;
            }
            //Debug.Log("切换到普通Overlay模式");
        }
    }

    // 场景加载事件
    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }

    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log($"场景 {scene.name} 加载完成，重置相机引用");
        mainCamera = null;
        cameraOne = null;
        UpdateCameraReferences();
        
        // 场景切换后默认使用普通模式
        SetCanvasMode(false);
    }

    public override void Awake()
    {
        base.Awake();

        GameObject canvasObj = Resources.Load<GameObject>("Canvas");
        canvasObj = Instantiate(canvasObj);
        canvasObj.name = "Canvas";
        Canvas = canvasObj.transform as RectTransform;
        canvasComponent = canvasObj.GetComponent<Canvas>();

        // 设置Canvas为全屏
        Canvas.anchorMin = Vector2.zero;
        Canvas.anchorMax = Vector2.one;
        Canvas.offsetMin = Vector2.zero;
        Canvas.offsetMax = Vector2.zero;

        DontDestroyOnLoad(canvasObj);

        // 初始化EventSystem
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = Resources.Load<GameObject>("EventSystem");
            if (eventSystemObj != null)
            {
                eventSystemObj = Instantiate(eventSystemObj);
                eventSystemObj.name = "EventSystem";
                DontDestroyOnLoad(eventSystemObj);
            }
        }
    }

    public void closePanel<T>()
    {
        string panelName = typeof(T).Name;
        if (UIPanelDict.ContainsKey(panelName))
        {
            UIPanelDict[panelName].Hide();
            UIPanelDict[panelName].transform.SetParent(Canvas.transform);
            
            // 从后处理面板集合中移除
            if (postProcessingPanels.Contains(panelName))
            {
                postProcessingPanels.Remove(panelName);
                
                // 如果没有后处理面板打开，切换回Overlay模式
                if (postProcessingPanels.Count == 0)
                {
                    SetCanvasMode(false);
                    //Debug.Log("所有后处理面板已关闭，自动切换回Overlay模式");
                }
            }
            
            if (currentPanel != null && currentPanel.transform.childCount <= 0)
            {
                Destroy(currentPanel);
                currentPanel = null;
            }
        }
    }

    public T getPanel<T>() where T : BasePanel
    {
        string panelName = typeof(T).Name;
        return UIPanelDict.ContainsKey(panelName) ? UIPanelDict[panelName] as T : null;
    }
    
    /// <summary>
    /// 强制切换到Overlay模式（外部调用）
    /// </summary>
    public void ForceOverlayMode() { postProcessingPanels.Clear(); SetCanvasMode(false); }
    
    /// <summary>
    /// 检查当前是否有后处理面板打开
    /// </summary>
    public bool HasPostProcessingPanels() { return postProcessingPanels.Count > 0; }
    
    /// <summary>
    /// 清空所有面板 
    /// </summary>
    public void ClearAllPanel()
    {
        foreach (var panel in UIPanelDict.Values) { panel.Hide(); }
    }

    /// <summary>
    /// 切换面板的显示/隐藏状态
    /// </summary>
    public void TogglePanel<T>(KeyCode keyCode) where T : BasePanel
    {
        if (Input.GetKeyDown(keyCode))
        {
            T panel = getPanel<T>();
            if (panel == null || !panel.gameObject.activeInHierarchy)
            {
                SoundAudioPool.Instance.PlaySound(SoundManager.Instance.menuBtnClick,transform.localPosition);
                // 面板不存在或未激活，打开面板
                openPanel<T>();
                EventCenter.Instance.EventTrigger(GameEvent.光标出现);
            }
            else
            {
                SoundAudioPool.Instance.PlaySound(SoundManager.Instance.menuBtnClick,transform.localPosition);
                // 面板已激活，关闭面板
                closePanel<T>();
                EventCenter.Instance.EventTrigger(GameEvent.光标消失);
            }
        }
    }


    #region 鬼魂跳脸
    [SerializeField] private RawImage jumpScareImage; // 全屏UI元素
    public void ShowJumpScareImage(Texture2D image, float duration)
    {
        jumpScareImage.texture = image;
        jumpScareImage.gameObject.SetActive(true);
        StartCoroutine(HideAfterDelay(jumpScareImage.gameObject, duration));
    }

    private IEnumerator HideAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }
    #endregion
}