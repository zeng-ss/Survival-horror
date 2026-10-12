using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CheckGhostPanel : BasePanel
{
    public GameObject content;
    public GameObject content2;
    public Button confirmBtn; // 确认定鬼 Btn
    
    // 面板锁定相关
    private bool isPanelLocked; // 是否被锁定
    private float lockEndTime;  // 锁定结束时间
    private int errorCount;     // 错误次数
    private readonly float baseLockTime = 60f; // 基础锁定时间（秒）

    void Start()
    {
        // 更新
        UpdateEvidenceAndGhostNameData();
        confirmBtn.onClick.AddListener(CheckGhost);
    }

    void Update()
    {
        if (isPanelLocked)
        {
            float remainingTime = lockEndTime - Time.time;
            if (remainingTime <= 0)
            {
                // 解锁面板
                isPanelLocked = false;
                confirmBtn.interactable = true;
            }
            else
            {
                // 锁定中：禁用按钮
                confirmBtn.interactable = false;
            }
        }
    }
    
    /// <summary>
    /// 核心：确认定鬼判断逻辑
    /// </summary>
    private void CheckGhost()
    {
        TipPanel tipPanel = UIManager.Instance.openPanel<TipPanel>();
        tipPanel.transform.localPosition = new Vector2(0, 430);
        // 检查面板是否被锁定
        if (isPanelLocked)
        {
            tipPanel.ShowTip("面板已锁定，暂时无法判断！");
            return;
        }
        // 获取所有未划横线的鬼（lineImage未激活）
        List<GhostData> validGhostList = new List<GhostData>();
        foreach (Transform child in content2.transform)
        {
            ghostNameItem item = child.GetComponent<ghostNameItem>();
            if (item != null && !item.lineImage.gameObject.activeSelf)
            {
                validGhostList.Add(item.currentGhostData);
            }
        }
        
        // 校验条件：未划横线的鬼数量为 1，且匹配 GameManager 的目标鬼
        bool isCorrect = false;
        if (validGhostList.Count == 1)
        {
            // 替换为 GameManager 中实际保存单个鬼的字段名（比如 CurrentGhost/TargetGhost 等）
            GhostData targetGhost = GameManager.Instance.GhostDataDict[GameManager.Instance.currentChooseGhostId]; 
            if (targetGhost != null && validGhostList[0].id == targetGhost.id) // 用ID匹配更可靠
            {
                isCorrect = true;
            }
        }
        // 处理结果
        if (isCorrect)
        {
            // 正确逻辑，显示成功提示、关闭面板等
            tipPanel.showTime = 3;
            tipPanel.ShowTip("定鬼成功，日记已出现，里面有抓鬼方法，请前往寻找");
            // 创建日记在场景中
            GameObject diarySpawnManager = GameObject.Find("DiarySpawnManager");
            diarySpawnManager.GetComponent<DiarySpawnManager>().enabled = true;
        }
        else
        {
            // 错误逻辑：锁定面板
            LockPanel();
            tipPanel.showTime = 3;
            tipPanel.ShowTip($"定鬼错误！面板将锁定{baseLockTime + errorCount * 30f}秒！");
        }
        UIManager.Instance.closePanel<CheckGhostPanel>(); 
        EventCenter.Instance.EventTrigger(GameEvent.光标消失);
    }

    /// <summary>
    /// 锁定面板方法
    /// </summary>
    private void LockPanel()
    {
        errorCount++;
        // 惩罚时长：基础 60秒 + 错误次数 * 30秒
        float lockTime = baseLockTime + (errorCount - 1) * 30f;
        lockEndTime = Time.time + lockTime;
        isPanelLocked = true;
        confirmBtn.interactable = false; // 禁用确认按钮
    }

    private void UpdateEvidenceAndGhostNameData()
    {
        // 先清理
        for (int i = 0; i < content.transform.childCount; i++)
        {
            Destroy(content.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < content2.transform.childCount; i++)
        {
            Destroy(content2.transform.GetChild(i).gameObject);
        }

        // !!! List<EvidenceData> = GameManager.Instance.EvidenceDataDict.Values.ToList()
        // List<EvidenceData> randomEvidenceList = new List<EvidenceData>(GameManager.Instance.EvidenceDataDict.Values);
        List<EvidenceData> randomEvidenceList = GameManager.Instance.EvidenceDataDict.Values.ToList();
        List<GhostData> randomGhostList = GameManager.Instance.GhostDataDict.Values.ToList();
        ShuffleList(randomGhostList); // 打乱列表顺序
        ShuffleList(randomEvidenceList); // 打乱列表顺序

        // 更新数据
        foreach (EvidenceData item in randomEvidenceList)
        {
            GameObject obj = ResMgr.Instance.load<GameObject>("UI/EvidenceItem",content.transform);
            EvidenceItem evidenceItem = obj.GetComponent<EvidenceItem>();
            evidenceItem.UpdateEvidenceData(item);
            // 新增：重置证据勾选状态（关闭面板后再打开，勾选状态清空）
            evidenceItem.tickImage.gameObject.SetActive(false);
            evidenceItem.currentSaveEvidenceItem = null;
        }
        foreach (GhostData item in randomGhostList)
        {
            GameObject obj = ResMgr.Instance.load<GameObject>("UI/ghostNameItem",content2.transform);
            ghostNameItem ghostItem = obj.GetComponent<ghostNameItem>();
            ghostItem.UpdateGhostData(item);
            // 强制刷新划线状态
            ghostItem.UpdateLineImage();
        }
        // 清空保存的证据列表（关闭面板后重置勾选）
        if (GameManager.Instance.SaveEvidenceList != null)
        {
            GameManager.Instance.SaveEvidenceList.Clear();
        }
    }

    /// <summary>
    /// 打乱列表顺序（Fisher-Yates洗牌算法）
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0 ; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
    
}
