using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 通灵玉控制器 
/// </summary>
public class JadeController : ItemController
{
    [Header("玉器设置")]
    private bool isWork;
    private bool isGhostInArea;
    private bool isCreatSubtitles;
    private SphereCollider sphereCollider;
    private float backHoldTimer;
    private GameObject effect;
    private Text plotText;
    private GameObject subtitleObj;

    [Header("字幕设置")]
    private bool isTyping;       // 是否在打字
    private float subtitleTimer; // 字幕计时器
    private float audioTimer;    // 基础音效计时器
    private int lastPlotIndex = -1;
    
    private List<string> plots = new ()
    {
        "快给我回来", "不要走", "我需要你", "我恨你", 
        "为什么要害我", "我发誓要你死", "我要你加倍偿还", 
        "我们一直存在下去吧", "我要养人死", "你的血很美味"
    };

    private new void Start()
    {
        base.Start();
        sphereCollider = GetComponent<SphereCollider>();
        effect = ResMgr.Instance.load<GameObject>("Model/effects/AuraChargeRed", transform);
        if (effect == null)
        {
            print("特效为空！");
            return;
        }
        effect.SetActive(false);
    }

    private new void Update()
    {
        base.Update();
        if (player == null || player.cameraOne == null || ghost == null)
        {
            return;
        }
        // 装备状态检测
        if (transform.parent == player.cameraOne.transform && gameObject.activeInHierarchy && !isOpen)
        {
            if (KeyBindingManager.Instance.GetAction(GameAction.UsingItem)) OpenIncreaseFillAmount();
            else OpenDecreaseFillAmount();
        }
        // 工作状态处理
        if (isWork)
        {
            if (KeyBindingManager.Instance.GetAction(GameAction.UsingItem)) BackIncreaseFillAmount();
            else BackDecreaseFillAmount();
            JadeWork();
        }
        // 效果缩放
        if (effect != null) effect.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        // 字幕计时器更新
        if (plotText != null && !isTyping)
        {
            subtitleTimer += Time.deltaTime;
            if (subtitleTimer >= 7f) ShowNextPlot();
        }
    }

    /// <summary>
    /// 玉器工作逻辑
    /// </summary>
    private void JadeWork()
    {
        effect.gameObject.SetActive(true);
        // 鬼魂在区域内
        if (isGhostInArea)
        {
            // 鬼魂在区域内：播放特效，创建/更新字幕
            if (ghost.name == "ShuiGui"|| ghost.name == "MuGui"||
                ghost.name == "HuaPi"|| ghost.name == "ZhaiGui")
            {
                audioTimer = 0;
                if (!isCreatSubtitles) CreateSubtitles();
                return;
            }
            // 清理字幕，播放基础音效
            CleanSubtitles();
            PlayBaseAudio();
        }
    }
    
    /// <summary>
    /// 清理字幕
    /// </summary>
    private void CleanSubtitles()
    {
        if (subtitleObj != null) 
        {
            Destroy(subtitleObj);
            subtitleObj = null;
            plotText = null;
            isCreatSubtitles = false;
        }
        subtitleTimer = 0;
    }
    
    /// <summary>
    /// 播放基础音效
    /// </summary>
    private void PlayBaseAudio()
    {
        audioTimer += Time.deltaTime;
        if (audioTimer >= 7f)
        {
            int index = Random.Range(0, 4);
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseGhostClips[index], transform.position);
            audioTimer = 0f;
        }
    }
    
    /// <summary>
    /// 创建字幕
    /// </summary>
    private void CreateSubtitles()
    {
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.mesBtnClick, transform.position);
        isCreatSubtitles = true;
        // 创建字幕容器
        subtitleObj = new GameObject("Subtitles");
        subtitleObj.transform.SetParent(GameObject.Find("currentShowPanel").transform, false);
        
        // 添加背景
        Image bg = subtitleObj.AddComponent<Image>();
        bg.sprite = ResMgr.Instance.load<Sprite>("Image/渐变bg褪色");
        bg.rectTransform.anchoredPosition = new Vector2(0, -400);
        bg.rectTransform.sizeDelta = new Vector2(1500, 200);
        bg.color = new Color(1, 1, 1, 0.8f);
        
        // 创建文本对象
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(subtitleObj.transform, false);
        
        plotText = textObj.AddComponent<Text>();
        plotText.font = ResMgr.Instance.load<Font>("Material&Animation&Fonts/字体管家棉花糖") ?? 
                       Resources.GetBuiltinResource<Font>("Arial.ttf");
        
        // 文本布局
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(30, 20);
        textRect.offsetMax = new Vector2(-30, -20);
        
        // 文本样式
        plotText.fontSize = 60;
        plotText.color = Color.red;
        plotText.alignment = TextAnchor.MiddleCenter;
        
        // 显示第一个文本
        ShowNextPlot();
    }
    
    /// <summary>
    /// 显示下一个文本（打字机效果）
    /// </summary>
    private void ShowNextPlot()
    {
        if (plots.Count == 0 || plotText == null) return;
        // 随机选择不重复的文本
        int randomIndex;
        do { randomIndex = Random.Range(0, plots.Count); } 
        while (randomIndex == lastPlotIndex && plots.Count > 1);
        lastPlotIndex = randomIndex;
        
        string randomPlot = plots[randomIndex];
        if (string.IsNullOrEmpty(randomPlot)) return;
        // 播放对应音效
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.subtitlesGhostClips[randomIndex], transform.position);
        
        // 打字机效果
        isTyping = true;
        plotText.text = "";
        plotText.DOText(randomPlot, randomPlot.Length * 0.4f)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                isTyping = false;
                subtitleTimer = 0f;
            });
    }
    
    
     #region 开启和收回玉石的方法

        /// <summary>
    /// 开启玉石的增加进度方法
    /// </summary>
    public override void OpenIncreaseFillAmount()
    {
        base.OpenIncreaseFillAmount();
        if (holdTimer >= duration) // 进度完成，开启道具功能
        {
            isOpen = true; holdTimer = 0;
            UIManager.Instance.closePanel<HorProgressPanel>();
            // 开启道具功能
            sphereCollider.enabled = true; isWork = true; // 切换工作状态
        }
    }
    
    
    /// <summary>
    /// 收回玉石的增加进度方法
    /// </summary>
    private void BackIncreaseFillAmount()
    {
        backHoldTimer += Time.deltaTime;
        HorProgressPanel horProgressPanel = GetProgressPanel();
        horProgressPanel.tipText.text = "道具收回中.....";
        horProgressPanel.IncreaseFillAmount(duration, backHoldTimer);
        if (backHoldTimer >= duration)
        {
            GameObject sbtitles = GameObject.Find("Subtitles");
            if (sbtitles != null) { Destroy(sbtitles); }
            // 如果有效果就隐藏
            effect.SetActive(false);
            isWork = false; backHoldTimer = 0;
            UIManager.Instance.closePanel<HorProgressPanel>();
            // 开启道具功能
            sphereCollider.enabled = false; isOpen = false; // 回到初始装备状态
            player.isStartOpen = false; isGhostInArea = false; isCreatSubtitles = false;
        }
    }
    
    /// <summary>
    /// 收回玉石减少进度方法
    /// </summary>
    private void BackDecreaseFillAmount()
    {
        // 如果没有面板则返回
        if (UIManager.Instance.getPanel<HorProgressPanel>() == null) { return;}
        HorProgressPanel horProgressPanel = UIManager.Instance.getPanel<HorProgressPanel>();
        // 如果还有进度则加速减少
        if (horProgressPanel.currentFillAmount > 0)
        {
            backHoldTimer -= Time.deltaTime * 2;
            horProgressPanel.DecreaseFillAmount(duration, backHoldTimer);
        }
        else if(horProgressPanel.currentFillAmount <= 0)
        {
            isWork = true; backHoldTimer = 0;
            UIManager.Instance.closePanel<HorProgressPanel>();
        }
    }

    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ghost"))
        {
            isGhostInArea = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ghost"))
        {
            isGhostInArea = false;
        }
    }
    
}
