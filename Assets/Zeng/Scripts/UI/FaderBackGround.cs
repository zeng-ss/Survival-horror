using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
/// <summary>
/// 过渡背景效果 
/// </summary>
public class FaderBackGround : MonoBehaviour
{
    private MainCameraController mainCamera;
    private bool isFading;
    private float fadeDuration = 1.5f;
    private float waitDuration = 14f;
    private CanvasGroup canvasGroup;
    public Button jumpBtn;
    public bool isNeedCreatText; // 是否需要创建背景文字
    private List<string> lastGhostList = new (); // 已经选中的鬼的集合
    
    private Tween delayTween;
    private Tween delayTextTween;
    private Coroutine delayCoroutine;

    // 鬼的信息字典
    private Dictionary<string, List<string>> backgroundDict = new();

    #region 背景信息

    private List<string> shuiBackGround = new ()
    {
        "这栋优雅的乔治亚风格宅邸，曾是著名外科医生“阿尔弗雷德·温斯顿”的住宅\n",
        "二十年前，他的独女艾玛患上绝症。绝望中，他向某种古老的存在祈求\n",
        "他用一场血腥的仪式，以自己的生命和医术为代价，将女儿“留”了下来\n",
        "然而，艾玛并未真正复活\n",
        "而是成了一个依附于宅邸、依靠汲取访客生命力存在的非生非死之物\n",
        "她依然保持着八岁女孩的天真与残忍，将每个闯入者都视为可以一起“永远玩耍”的新朋友\n",
        "你受雇于最后一位侥幸逃脱的访客，前来解开这延续了二十年的不死诅咒"
    };
    private List<string> diaoBackGround = new ()
    {
        "“黑木公寓”403室，表面上是一起残忍的灭门惨案现场。但你知道，事情没这么简单\n",
        "这里曾住着一位痴迷于留声机的手语教师一家。真正的悲剧始于男主人获得了一卷来历不明的胶片\n",
        "影片内容无法言说，却能让观者在绝对的寂静中产生致命的幻觉\n",
        "如今，这家人以扭曲的“默片幽灵”形式存在着：所有恐怖的袭击都发生在绝对安静之中\n",
        "随后才会爆发出扭曲的、来自留声机的尖锐音乐\n",
        "你需要在这部循环播放的死亡默片中，找到那卷原始胶片\n",
        "并面对那个试图将所有人为其“无声剧场”增添角色的导演之魂"
    };
    private List<string> muBackGround = new ()
    {
        "这栋位于郊外的三层汽车旅馆并非建筑上的异类，而是时间与空间的“锚点”\n",
        "老板在建造时，无意中将地基打在了数个平行世界灾难事件的交汇点上\n",
        "每当其他世界发生死亡悲剧（如火灾、谋杀、瘟疫）\n",
        "其回响与亡灵便会如“幽灵信号”般在此处短暂显形\n",
        "因此，旅馆内的鬼魂现象总在毫无规律地变化——昨夜304房还是溺亡者的湿漉脚印\n",
        "今夜可能就变成了烧焦的呼救声。你需要找出这些“信号”的来源规律\n",
        "并面对那个被所有世界悲剧反复冲刷、已彻底扭曲、想将旅舍变成所有不幸者永恒避难所的老板亡灵\n",
    };
    private List<string> liBackGround = new ()
    {
        "湖边那座粗野主义的混凝土别墅，是已故先锋艺术家“陈羲”的故居与最后一件作品\n",
        "他坚信情绪是实体，并发明了能够抽取、储存人类强烈情绪的装置——“情绪熔炉”\n",
        "他去世后，熔炉失控，长期积累的恐惧、嫉妒、狂喜等情绪凝结成各种基于情绪的抽象怪物\n",
        "整栋房子成了一个活着的、情绪化的生态系统\n",
        "愤怒会让管道发烫流血，而扭曲的爱意则会让人产生被拥抱至骨骼碎裂的幻觉\n",
        "你并非来驱鬼，而是受艺术基金会委托，来关闭这个失控的装置\n",
        "并直面这位艺术家最终极的一件“作品”——他自己投入炉中的纯粹痴迷。"
    };
    private List<string> huaBackGround = new ()
    {
        "知名人偶大师“格里姆”的宅邸。他一生未婚，将全部情感倾注于制作栩栩如生的人偶\n",
        "传闻他掌握了让造物“活”过来的禁忌技艺\n",
        "真相是，他将自己的灵魂碎片与记忆，分批注入了他最满意的十二个作品之中\n",
        "他死后，这些人偶“活”了过来，各自继承了他性格的一部分\n",
        "并坚信自己才是格里姆的“唯一正统”\n",
        "它们将宅邸视为自己的舞台与战场，用丝线、陷阱夺控制权，并将闯入者视为新的“原材料”\n",
        "你需要在这群危险的“家人”中周旋，找到并拼合大师真正的灵魂核心"
    };
    private List<string> nanBackGround = new ()
    {
        "表面看，这是一户温馨的中产家庭住宅。但所有灵异现象都遵循一个诡异法则\n",
        "只发生在物体的“倒影”或“对称位置”\n",
        "例如，真实世界的水杯放在桌子左边，其鬼魂倒影可能出现在右边；墙上的影子做出与你完全不同的动作\n",
        "调查发现，这家人曾痴迷于一个神秘仪式，试图创造一个“完美的镜像家庭”来替代自己承担厄运\n",
        "仪式成功了，却也失败了\n",
        "现在，那个充满怨念的“倒影家庭”正试图与现实家庭交换位置\n",
        "将活人拉入永恒的镜像囚笼。你必须利用这个“对称法则”在虚实间切换，同时阻止交换的完成"
    };
    private List<string> zhaiBackGround = new ()
    {
        "这栋不起眼的旧公寓里，住着一个并非以生命，而是以“记忆”为食的古老存在\n",
        "它没有固定形态，更像一种弥漫性的氛围\n",
        "它会悄然抽取住户的珍贵记忆作为养分，被抽取者不会立刻死亡，只会变得麻木空洞，最终成为一具“空壳”在屋内游荡\n",
        "这座公寓因此积累了数十年的记忆碎片\n",
        "这些碎片会随机重现，形成无法分辨虚实的记忆回廊\n",
        "鬼魂正是那些被彻底吃空、只剩下记忆外壳的昔日住户\n",
        "你需要在记忆迷宫中找到那个存在的核心——它可能伪装成一段最温暖、让你最不愿离开的记忆"
    };
    private List<string> jingBackGround = new ()
    {
        "这座百年老宅的每一任屋主都死于非命，且死因都与前任惊人地相似\n",
        "这不是诅咒的简单重复，而是一个更加黑暗的“传承”\n",
        "初代屋主为了家族永恒繁荣，与邪灵立约，将屋主之死设计为一场献给邪灵的、仪式性的“戏剧”\n",
        "每一任新屋主在入住时，其实都无形中签署了这份契约，他们的命运被编织进固定的“剧本”\n",
        "你踏入时，便已成为最新一任“主角”\n",
        "之前的屋主亡魂并非都是恶灵，有些是试图警告你的悲剧演员，有些则是入戏太深、想将你推入预定结局以完成自己表演的偏执狂\n",
        "你需要识破“剧本”的结构，在无数轮回的死亡演出中，找到改写结局的方法"
    };

    #endregion
    
    void OnEnable()
    {
        backgroundDict.Add("ShuiGui", shuiBackGround);
        backgroundDict.Add("DiaoSiGui", diaoBackGround);
        backgroundDict.Add("MuGui", muBackGround);
        backgroundDict.Add("LiGui", liBackGround);
        backgroundDict.Add("HuaPi", huaBackGround);
        backgroundDict.Add("NanChanGui", nanBackGround);
        backgroundDict.Add("ZhaiGui", zhaiBackGround);
        backgroundDict.Add("JingZhongHun", jingBackGround);
        mainCamera = GameObject.Find("MainCamera")?.GetComponent<MainCameraController>();
        if (jumpBtn != null)
        {
            jumpBtn.gameObject.SetActive(true);
            jumpBtn.onClick.AddListener(() =>
            {
                SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
                mainCamera.KillDelayCall();
                SceneMgr.Instance.LoadSceneAsync("FactoryNight",() => {UIManager.Instance.closePanel<LoadPanel>();});
                gameObject.SetActive(false);
            });
        }
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
    }

    private void OnDisable()
    {
        if (jumpBtn != null) { jumpBtn.onClick.RemoveAllListeners(); }
        if (delayTween != null) { delayTween.Kill(); delayTween = null; }
        if (delayCoroutine != null) StopCoroutine(delayCoroutine);
        if (delayTextTween != null) { delayTextTween.Kill(); delayTextTween = null; }
    }


    #region 效果相关

    public void StartFade()
    {
        if (!isFading)
        {
            isFading = true;
            if (isNeedCreatText) CreateText();
            delayCoroutine = StartCoroutine(FadeCoroutine());
        }
    }
    private IEnumerator FadeCoroutine()
    {
        float timer = 0;
        // 减少曝光（渐黑）
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float value = timer / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(0, 1, value);
            yield return null;
        }
        canvasGroup.alpha = 1;
        // 等待
        yield return new WaitForSeconds(waitDuration);
        // 恢复曝光（渐亮）
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t);
            yield return null;
        }
        canvasGroup.alpha = 0;
        GetComponent<Image>().enabled = false;
        isFading = false;
    }

    #endregion
    

    /// <summary>
    /// 动态创建背景文字信息
    /// </summary>
    private void CreateText()
    {
        KeyValuePair<string, List<string>> currentBackGround;
        if (GameManager.Instance.isFirstPlayGame)
        {
            var curBackGround = backgroundDict.ToList();
            currentBackGround = curBackGround[0]; // 默认第一关是水鬼
            lastGhostList.Add(currentBackGround.Key);
        }
        else
        {
            /*jumpBtn.gameObject.SetActive(true);
            currentBackGround = RandomChooseGhost();*/
            var curBackGround = backgroundDict.ToList();
            currentBackGround = curBackGround[1]; // 默认第一关是水鬼
            lastGhostList.Add(currentBackGround.Key);
        }
        // 保存选中的鬼信息到 GameManager
        GameManager.Instance.currentChooseGhostId = GetGhostId(currentBackGround.Key);
        print(GameManager.Instance.currentChooseGhostId);
        // 随机选一个鬼的信息
        for (int i = 0; i < currentBackGround.Value.Count; i++)
        {
            GameObject text = ResMgr.Instance.load<GameObject>("BackGroundDes", transform);
            text.transform.localPosition = new Vector2(0, 400 - i * 100);
    
            // 创建局部变量，为每一轮循环创建独立的副本
            int currentIndex = i;
            string currentText = currentBackGround.Value[currentIndex];
    
            delayTextTween = DOVirtual.DelayedCall(currentIndex * 2f, () => 
            {
                // 现在使用的是局部变量的副本，不会被后续循环修改
                Text textComponent = text.GetComponent<Text>();
                textComponent.text = currentText;
                textComponent.DOFade(0, 0);
                textComponent.DOFade(1, 2);
            });
            
            delayTween = DOVirtual.DelayedCall(13f, () =>
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.transform.GetComponent<Text>().DOFade(0, 2);
                }
            });
        }
    }
    
    /// <summary>
    /// 随机选鬼
    /// </summary>
    private KeyValuePair<string, List<string>> RandomChooseGhost()
    {
        // 如果所有鬼都选过了，重置列表
        if (lastGhostList.Count >= backgroundDict.Count) { lastGhostList.Clear(); }
    
        // 获取所有可选的鬼键（排除已选过的）
        List<string> availableKeys = backgroundDict.Keys
            .Where(key => !lastGhostList.Contains(key))
            .ToList();
        if (availableKeys.Count == 0)
        {
            lastGhostList.Clear();
            availableKeys = backgroundDict.Keys.ToList();
        }
        // 随机选择一个键
        string selectedKey = availableKeys[Random.Range(0, availableKeys.Count)];
        // 记录已选的鬼
        lastGhostList.Add(selectedKey);
        // 返回对应的键值对
        return new KeyValuePair<string, List<string>>(selectedKey, backgroundDict[selectedKey]);
    }

    private int GetGhostId(string key)
    {
        switch (key)
        {
            case "ShuiGui": return 1;
            case "DiaoSiGui": return 2;
            case "MuGui": return 3;
            case "LiGui": return 4;
            case "HuaPi": return 5;
            case "NanChanGui": return 6;
            case "ZhaiGui": return 7;
            case "JingZhongHun": return 8;
        }
        return 1;
    }
    
}
