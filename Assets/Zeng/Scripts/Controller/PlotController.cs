using System.Collections;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
// 过场动画控制器
public class PlotController : MonoBehaviour
{
    public Button jumpBtn;
    public Image backImage;
    public Camera plotCamera;
    public Camera cameraStart;
    public GameObject plotPanel;
    public Text plotText;
    private float timer;      // 显示独白计时器
    private int currentPlot;  // 当前的独白
    private bool isStartPlot; // 判断是否开始剧情
    public CinemachineFreeLook freeLookCamera;
    public PlayableDirector timeLinePlayer;  // TimeLine导演
    private CanvasGroup canvasGroup;
    
    // 存储延迟调用的句柄，用于取消
    private Tween delayedShowGhost;
    private Tween delayedChangeScene;
    // 存储协程句柄，用于终止
    private Coroutine fadeCoroutine;
    private Coroutine waitCoroutine;

    // 每条剧情的出现时间字典
    private Dictionary<int, float> plotsTimeDict = new()
    {
        { 0, 0 },
        { 1, 3.1f },
        { 2, 4 },
        { 3, 2.8f },
        { 4, 2.3f },
        { 5, 2 },
        { 6, 1.25f },
        { 7, 2.8f },
        { 8, 8.27f },
    };
    private List<string> plots = new()
    {
        "见鬼，房子的主人给我送到哪来了...",
        "我看看地图，见鬼地图怎么看不清了",
        "空气中弥漫着一股腐木和...铁锈的味道",
        "我打个电话问问",
        "（手机屏幕闪烁）'无信号...怎么可能？'",
        "（远处传来脚步声）有人吗？",
        "脚步声突然停止，取而代之的是...婴儿的哭声",
        "哭声好像来自走廊尽头，我去看看",
        "啊！那是什么东西..."
    };

    void OnEnable()
    {
        plotPanel.SetActive(false);
        if (GameManager.Instance.isFirstPlayGame) { jumpBtn.gameObject.SetActive(false); }
        else { jumpBtn.gameObject.SetActive(true); }
        jumpBtn.onClick.AddListener(OnJumpPlot); // 抽离跳过逻辑到单独方法
        timeLinePlayer.Stop();
        canvasGroup = backImage.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        waitCoroutine = StartCoroutine(Wait());
    }

    private void OnDisable() 
    {
        jumpBtn.onClick.RemoveAllListeners();
        // 销毁时清理所有延迟调用和协程，防止内存泄漏
        CancelAllDelayedCalls();
        StopAllCoroutines();
    }

    /// <summary>
    /// 跳过剧情按钮点击逻辑
    /// </summary>
    private void OnJumpPlot()
    {
        AudioSource audioSource = FindObjectOfType<AudioSource>();
        audioSource.Stop();
        // 终止所有延迟调用
        CancelAllDelayedCalls();
        // 终止协程
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        // 终止协程
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }
        // 停止 Timeline 和重置状态
        timeLinePlayer.Stop();
        isStartPlot = false;
        plotPanel.SetActive(false);
        // 重置相机输入（可选，恢复玩家控制）
        freeLookCamera.m_XAxis.m_InputAxisName = "Mouse X";
        freeLookCamera.m_YAxis.m_InputAxisName = "Mouse Y";
        
        // 单次执行场景切换，避免重复
        SceneMgr.Instance.LoadSceneAsync("Show", () =>
        {
            UIManager.Instance.closePanel<LoadPanel>();
            SpawnGhost();
        });
    }

    private void SpawnGhost()
    {
        // 先获取目标位置
        GameObject obj = ResMgr.Instance.load<GameObject>(
            $"Model/ghostModel/{GameManager.Instance.GhostDataDict[GameManager.Instance.currentChooseGhostId].model}");
        obj.name = GameManager.Instance.GhostDataDict[GameManager.Instance.currentChooseGhostId].model;
    }

    /// <summary>
    /// 动画切换到鬼的方法 
    /// </summary>
    private void OnShowGhost()
    {
        // 保存目标位置和旋转
        Vector3 targetPos = new Vector3(-11.5f, 3.145f, 31.349f);
        Quaternion targetRot = Quaternion.Euler(4.29f, 0, 0);
        cameraStart.transform.localPosition = plotCamera.transform.localPosition;
        cameraStart.transform.localRotation = plotCamera.transform.localRotation;
        plotCamera.gameObject.SetActive(false);
        cameraStart.gameObject.SetActive(true);
        // 开始移动
        Sequence seq = DOTween.Sequence();
        seq.Append(cameraStart.transform.DOLocalMove(targetPos, 0.5f));
        seq.Join(cameraStart.transform.DORotate(targetRot.eulerAngles, 0.5f));
    }

    /// <summary>
    /// 剧情结束 切换场景
    /// </summary>
    private void ChangeScene()
    {
        canvasGroup.alpha = 1;
        // 存储协程句柄，方便跳过时终止
        fadeCoroutine = StartCoroutine(FadeIn());
    }
    
    /// <summary>
    /// 淡入
    /// </summary>
    /// <returns></returns>
    IEnumerator FadeIn()
    {
        // 执行淡入动画
        float fadeInDuration = 2f;
        float timer1 = 0f;
        
        while (timer1 < fadeInDuration && isStartPlot) // 增加 isStartPlot判断，防止跳过后继续执行
        {
            timer1 += Time.deltaTime;
            float t = timer1 / fadeInDuration;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t);
            yield return null; // 关键：每帧暂停，让出主线程控制权
        }
        if (!isStartPlot) yield break; // 已跳过，直接终止协程
        // 确保最终值
        backImage.GetComponent<CanvasGroup>().alpha = 1;
        // 完成后，执行等待
        yield return new WaitForSeconds(2);
        plotPanel.SetActive(false);
        // 执行淡出动画
        float fadeDuration = 1f;
        float timer2 = 0f;
        SceneMgr.Instance.LoadSceneAsync("Show", () =>
        {
            UIManager.Instance.closePanel<LoadPanel>();
            SpawnGhost();
        });
        plotPanel.SetActive(false);
        while (timer2 < fadeDuration)
        {
            timer2 += Time.deltaTime;
            float t2 = timer2 / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t2);
            yield return null; // 关键：每帧暂停，让出主线程控制权
        }
        canvasGroup.alpha = 0;
    }
    
    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(2f); // 2 秒后开始防止卡顿
        // 移动相机
        cameraStart.transform.DOMove(new Vector3(-12.63f, 0.3f, -54.86f), 3f).OnComplete(() =>
        {
            plotCamera.gameObject.SetActive(true);
            cameraStart.gameObject.SetActive(false);
            isStartPlot = true;
            timeLinePlayer.Play();
            if (!GameManager.Instance.isFirstPlayGame) jumpBtn.gameObject.SetActive(true);
            // TImeLine 执行 26.6 s 之后开始移动到鬼,存储延迟调用句柄，方便取消
            delayedShowGhost = DOVirtual.DelayedCall(26.6f, OnShowGhost);
            // TImeLine 执行 30 s 之后开始切换场景
            delayedChangeScene = DOVirtual.DelayedCall(30f, ChangeScene);
        });
        freeLookCamera.m_XAxis.m_InputAxisName = "";  // 清空水平轴输入
        freeLookCamera.m_YAxis.m_InputAxisName = "";  // 清空垂直轴输入
        plotPanel.SetActive(false);
    }
    
    /// <summary>
    /// 取消所有延迟调用
    /// </summary>
    private void CancelAllDelayedCalls()
    {
        if (delayedShowGhost != null) delayedShowGhost.Kill();
        if (delayedChangeScene != null) delayedChangeScene.Kill();
    }
    
    void Update()
    {
        if (currentPlot >= plots.Count || !isStartPlot) return;
        timer += Time.deltaTime;
        // 开始剧情文字
        if (timer >= plotsTimeDict[currentPlot] && currentPlot <= plots.Count - 1 && isStartPlot)
        {
            plotPanel.SetActive(true);
            plotText.text = plots[currentPlot];
            if (SoundManager.Instance.plotGhostClips[currentPlot] != null)
            {
                SoundAudioPool.Instance.PlaySound(SoundManager.Instance.plotGhostClips[currentPlot],transform.position);
            }
            currentPlot++;
            timer = 0;
        }
    }
}
