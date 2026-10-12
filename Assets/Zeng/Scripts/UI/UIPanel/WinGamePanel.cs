using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WinGamePanel : BasePanel
{
    public Button closeButton;
    public Text ghostText;
    public Text coinText;
    public Text itemText;
    private List<Text> texts = new ();
    void OnEnable()
    {
        if (Cursor.lockState == CursorLockMode.Locked) Cursor.lockState = CursorLockMode.None;
        closeButton.onClick.AddListener(() =>
        {
            UIManager.Instance.closePanel<WinGamePanel>();
            UIManager.Instance.openPanel<StartPanel>();
        });
    }

    void OnDisable()
    {
        closeButton.onClick.RemoveAllListeners();
    }

    
    /// <summary>
    /// 更新鬼的上锁状态，更新面板数据  
    /// </summary>
    /// <param name="currentGhostData">当前解锁的鬼</param>
    public void UpdateData(GhostData currentGhostData,ItemData currentItemData)
    {
        ghostText.text = $"解锁鬼魂:{GameManager.Instance.GhostDataDict[currentGhostData.id].name}";
        coinText.text = $"获得金币:{GameManager.Instance.GhostDataDict[currentGhostData.id].coin}";
        itemText.text = $"解锁道具:{GameManager.Instance.ItemDataDict[currentItemData.id].name}";
        texts.Add(ghostText);
        texts.Add(coinText);
        texts.Add(itemText);
        GameManager.Instance.Coin += GameManager.Instance.GhostDataDict[currentGhostData.id].coin;
        // 设置逐步出现动画
        for (int i = 0; i < texts.Count; i++)
        {
            Text optionItem = texts[i];
            optionItem.transform.localScale = Vector3.zero;
            optionItem.transform.DOScale(Vector3.one, 1f)
                .SetDelay(i * 0.2f)
                .SetEase(Ease.OutBack);
        }
        // 设置鬼的上锁状态为解锁
        GameManager.Instance.GhostLockDict[currentGhostData.id] = false;
    }
}
