using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LookSelectPanel : BasePanel
{
    public GameObject content;
    public RectTransform rectTransform;
    // 存储道具 id 对应的游戏对象
    public Dictionary<int, GameObject> itemIconDic = new ();

    void OnEnable()
    {
        rectTransform.transform.localPosition = new Vector3(0, -540, 0);
        // 初始时清空content
        ClearContent();
        // 从GameManager加载已保存的选择
        LoadSavedSelections();
    }   

    /// <summary>
    /// 从GameManager加载已保存的选择
    /// </summary>
    private void LoadSavedSelections()
    {
        foreach (var kvp in GameManager.Instance.ItemSelectNumDict)
        {
            int itemId = kvp.Key;
            int count = kvp.Value;
            
            if (GameManager.Instance.ItemDataDict.TryGetValue(itemId, out ItemData item) && count > 0)
            {
                CreateOrUpdateIcon(item, count);
            }
        }
    }
    
    /// <summary>
    /// 添加选择道具（增加数量或创建新图标）
    /// </summary>
    public void AddItem(ItemData item)
    {
        int currentCount = GameManager.Instance.ItemSelectNumDict[item.id];
        if (currentCount == 1)
        {
            CreateNewIcon(item, 1); // 第一次添加，创建新图标
        }
        else
        {
            UpdateIconCount(item.id, currentCount); // 增加现有图标的数量
        }
    }
    
    /// <summary>
    /// 减少选择道具数量
    /// </summary>
    public void DeItem(ItemData item)
    {
        int currentCount = GameManager.Instance.ItemSelectNumDict[item.id];
        if (currentCount > 0)
        {
            // 减少现有图标的数量
            UpdateIconCount(item.id, currentCount);
        }
        else if (currentCount == 0)
        {
            // 数量为 1 时，减少到 0，移除图标 如果已经是0，什么都不做
            RemoveIcon(item.id);
        }
    }
    
    /// <summary>
    /// 创建或更新图标（带数量显示）
    /// </summary>
    private void CreateOrUpdateIcon(ItemData item, int count)
    {
        if (itemIconDic.ContainsKey(item.id))
        {
            UpdateIconCount(item.id, count); // 更新现有图标数量
        }
        else
        {
            CreateNewIcon(item, count); // 创建新图标
        }
    }
    
    /// <summary>
    /// 更新图标数量显示
    /// </summary>
    private void UpdateIconCount(int itemId, int count)
    {
        if (itemIconDic.TryGetValue(itemId, out GameObject iconObject))
        {
            Transform countTextObj = iconObject.transform.Find("CountText");
            if (countTextObj != null)
            {
                TextMeshProUGUI countText = countTextObj.GetComponent<TextMeshProUGUI>();
                countText.text = count.ToString();
            }
        }
    }
    
    /// <summary>
    /// 创建新图标
    /// </summary>
    private void CreateNewIcon(ItemData item,int count)
    {
        GameObject iconObject = new GameObject("ItemIcon");
        iconObject.transform.SetParent(content.transform);
        
        // 添加Image组件
        Image image = iconObject.AddComponent<Image>();
        Sprite sprite = ResMgr.Instance.load<Sprite>($"Image/{item.icon}");
        image.sprite = sprite;
        
        // 设置RectTransform
        RectTransform rectTransform = iconObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(80, 80);
        rectTransform.localScale = Vector3.one;
        
        // 创建数量文本
        GameObject countTextObject = new GameObject("CountText");
        countTextObject.transform.SetParent(iconObject.transform);
        
        // 添加TextMeshPro组件显示数量
        TextMeshProUGUI countText = countTextObject.AddComponent<TextMeshProUGUI>();
        countText.text = count.ToString();
        countText.fontSize = 20;
        countText.color = Color.white;
        countText.alignment = TextAlignmentOptions.TopRight;
        
        // 设置数量文本的位置（右上角）
        RectTransform countRect = countTextObject.GetComponent<RectTransform>();
        countRect.anchorMin = new Vector2(1, 1);
        countRect.anchorMax = new Vector2(1, 1);
        countRect.pivot = new Vector2(1, 1);
        countRect.anchoredPosition = new Vector2(-5, -5);
        countRect.sizeDelta = new Vector2(30, 30);
        
        // 存储到字典
        itemIconDic[item.id] = iconObject;
    }
    
    /// <summary>
    /// 移除图标
    /// </summary>
    private void RemoveIcon(int itemId)
    {
        if (itemIconDic.TryGetValue(itemId, out GameObject iconObject))
        {
            Destroy(iconObject);
            itemIconDic.Remove(itemId);
        }
    }
    
    /// <summary>
    /// 清空content下的所有子对象
    /// </summary>
    private void ClearContent()
    {
        foreach (Transform child in content.transform) { Destroy(child.gameObject); }
        itemIconDic.Clear();
    }

    
}
