using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartPanel : BasePanel
{
    // 按钮
    public Button startButton;
    public Button loadButton;
    public Button exitButton;
    public Button escButton;
    public Button shopButton;
    public Button detailsButton;
    
    public TextMeshProUGUI startText;
    public TextMeshProUGUI exitText;
    public TextMeshProUGUI loadText;
    
    // 过场背景动画
    private float timer;
    [HideInInspector] public MainCameraController mainCamera;
    // 用静态变量记录是否在当前场景播放过，切换场景后重置
    private static bool isSoundPlayedInCurrentScene;
    
    public override void Awake()
    {
        // 注册场景切换事件（关键：全局监听场景变化）
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    // 场景卸载时自动重置标记（全局事件，百分百触发）
    private void OnSceneUnloaded(Scene scene)
    {
        isSoundPlayedInCurrentScene = false;
        Debug.Log($"场景[{scene.name}]卸载，重置播放标记为：{isSoundPlayedInCurrentScene}");
    }
    
    private void PlayStartSound()
    {
        // 避免同一场景内重复播放（比如面板多次启用）
        if (isSoundPlayedInCurrentScene) return;
        // 增加空值检查，避免空引用
        if (SoundAudioPool.Instance == null)
        {
            Debug.LogError("StartPanel: SoundAudioPool.Instance为空！");
            return;
        }
        AudioClip startClip = SoundManager.Instance?.startSound;
        if (startClip == null)
        {
            Debug.LogError("StartPanel: startSound音频片段为空！");
            return;
        }
        SoundAudioPool.Instance.PlaySound(startClip, transform.position);
        isSoundPlayedInCurrentScene = true;
    }

    private void OnEnable()
    {
        //Debug.Log("StartPanel OnEnable 执行，当前场景播放标记：" + isSoundPlayedInCurrentScene);
        // 延迟加长到0.3秒，确保所有全局单例都初始化完成
        Invoke(nameof(PlayStartSound), 0.3f);
        mainCamera = GameObject.Find("MainCamera").GetComponent<MainCameraController>();
        shopButton.onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.menuBtnClick, transform.position);
            UIManager.Instance.closePanel<StartPanel>();
            ShopPanel shopPanel = UIManager.Instance.openPanel<ShopPanel>();
            shopPanel.UpdateData();
            shopPanel.transform.localPosition = new Vector2(2000, 136);
            shopPanel.transform.DOLocalMove(new Vector2(0, 136), 0.5f);
        });
        startButton.onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.menuBtnClick, transform.position);
            UIManager.Instance.closePanel<StartPanel>();
            mainCamera.MoveToLookAtPanel();
        });
        loadButton.onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.menuBtnClick, transform.position);
            UIManager.Instance.closePanel<StartPanel>();
            UIManager.Instance.openPanel<LoadFilePanel>().UpdateData();
        });
        exitButton.onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.menuBtnClick, transform.position);
            UIManager.Instance.openPanel<SaveFilePanel>();
        });
        escButton.onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.menuBtnClick, transform.position);
            PausePanel pausePanel = UIManager.Instance.openPanel<PausePanel>();
            pausePanel.backBtn.gameObject.SetActive(false);
            pausePanel.transform.localPosition = new Vector2(2000, 0);
            pausePanel.transform.DOLocalMove(new Vector2(0, 0), 0.5f);
        });
        detailsButton.onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.menuBtnClick, transform.position);
            Vector3 startPos = new Vector3(2050, 1110, 0);
            Vector3 targetPos = new Vector3(45, -39, 0);
            GhostDetailPanel ghostDetailPanel = UIManager.Instance.openPanel<GhostDetailPanel>();
            ghostDetailPanel.transform.localPosition = startPos;
            ghostDetailPanel.transform.DOLocalMove(targetPos, 0.5f);
            UIManager.Instance.closePanel<StartPanel>();
        });
    }

    void Update()
    {
        // 设置文字动画效果
        timer += Time.deltaTime * 2;
        startText.fontSize = Mathf.Lerp(40, 60, (Mathf.Sin(timer) + 1) * 0.5f);
        exitText.fontSize = Mathf.Lerp(40, 60, (Mathf.Sin(timer) + 1) * 0.5f);
        loadText.fontSize = Mathf.Lerp(40, 60, (Mathf.Sin(timer) + 1) * 0.5f);
    }

    void OnDisable()
    {
        startButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();
        escButton.onClick.RemoveAllListeners();
        detailsButton.onClick.RemoveAllListeners();
        shopButton.onClick.RemoveAllListeners();
        loadButton.onClick.RemoveAllListeners();
    }
    
    // 场景卸载时重置标记（关键：确保返回场景时能重新播放）
    private void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
}
