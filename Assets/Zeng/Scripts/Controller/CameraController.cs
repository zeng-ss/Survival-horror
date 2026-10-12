using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    private PostProcessVolume volume;
    [HideInInspector] public Vignette vignette;
    [Header("第一人称设置")]
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    private float xRotation;
    private bool isOpenUI;
    // 缓存变量避免重复计算
    private Transform cameraTransform;
    private const float SENSITIVITY_MULTIPLIER = 0.01f; // 预计算时间因子
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        volume = GetComponent<PostProcessVolume>();
        vignette = volume.profile.GetSetting<Vignette>();
        vignette.active = false;
        EventCenter.Instance.AddEventListener(GameEvent.光标出现,HasOpenUI);
        EventCenter.Instance.AddEventListener(GameEvent.光标消失,HasCloseUI);
        cameraTransform = transform; // 缓存transform
    }

    #region 开启后处理方法

    /// <summary>
    /// 开启看日记后处理效果方法
    /// </summary>
    public void StartLookDiaryPost()
    {
        vignette.active = true;
        vignette.color.value = Color.black;
        vignette.intensity.value = 1f;
        vignette.smoothness.value = 1f;
        vignette.roundness.value = 1f;
        vignette.rounded.value = true;
    }
    
    /// <summary>
    /// 开启死亡后处理效果方法
    /// </summary>
    public void StartDeadPost()
    {
        vignette.active = true;
        vignette.color.value = Color.red;
        vignette.smoothness.value = 1f;
        vignette.roundness.value = 1f;
        DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 1f, 3.5f);
    }
    
    /// <summary>
    /// 开启受伤后处理效果方法
    /// </summary>
    public void StartHurtPost()
    {
        vignette.active = true;
        vignette.color.value = Color.red;
        vignette.intensity.value = 1f;
        vignette.smoothness.value = 1f;
        vignette.roundness.value = 1f;
        vignette.rounded.value = true;
    }

    #endregion
    
    private void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener(GameEvent.光标出现,HasOpenUI);
        EventCenter.Instance.RemoveEventListener(GameEvent.光标消失,HasCloseUI);
    }
    private void HasOpenUI() { isOpenUI = true; }
    private void HasCloseUI() { vignette.active = false; isOpenUI = false; }
    void Update()
    {
        HandleCursorState();
        if (!isOpenUI) { RotateCamera(); }
        UIManager.Instance.TogglePanel<PausePanel>(KeyCode.Escape);
        if (UIManager.Instance.getPanel<PausePanel>() != null)
        {
            UIManager.Instance.getPanel<PausePanel>().backBtn.gameObject.SetActive(true);
        }
        UIManager.Instance.TogglePanel<CheckGhostPanel>(KeyCode.T);
    }

    private void HandleCursorState()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        { 
            Cursor.lockState = CursorLockMode.None; 
        }
        // 简化光标状态逻辑
        Cursor.lockState = isOpenUI ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void RotateCamera()
    {
        // 只在第一人称时处理鼠标视角
        if (gameObject.activeInHierarchy)
        {
            // 使用预计算的灵敏度，减少乘法运算
            float sensitivity = mouseSensitivity * SENSITIVITY_MULTIPLIER;
            float mouseX = Input.GetAxis("Mouse X") * sensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

            // 上下视角 - 使用本地缓存减少属性访问
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
            // 左右旋转玩家身体 - 使用缓存的transform
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
    
}
