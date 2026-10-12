using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ShopPanel : BasePanel
{
    [Header("主面板相关")]
    public GameObject content;
    public Button closeBtn;
    
    [Header("购买面板相关")]
    #region 购买面板相关

    public GameObject buyPanel;
    public GameObject mask;
    public Text itemName;
    public Button backBtn;
    public Button confirmBtn;
    public TMP_InputField buyInputField; // 玩家输入的购买数量 InputField
    private int currentBuyQuantity;      // 当前选择的道具购买数量
    public Text currentHasCoin;          // 当前角色拥有的金币数量
    public Text currentTotalCoin;        // 当前所需支付的全部金币
    private Vector2 buyPanelStartPos;
    private Vector2 buyPanelEndPos;

    #endregion
    
    [Header("描述面板相关")]
    #region 描述面板相关

    public GameObject desPanel;
    public Text desText;
    private Vector2 desPanelStartPos;
    private Vector2 desPanelEndPos;

    #endregion
    
    private ItemData currentItem; // 当前的选中的道具信息
    private Tween currentTween;   // 用于杀死当前的动画防止动画冲突
    void OnEnable()
    {
        closeBtn.onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
            UIManager.Instance.openPanel<StartPanel>();
            UIManager.Instance.closePanel<ShopPanel>();
        });
        currentHasCoin.text = GameManager.Instance.Coin.ToString();
        mask.SetActive(false);
        backBtn.onClick.AddListener(DeShowBuyPanel);
        // 绑定输入框事件
        buyInputField.onEndEdit.AddListener(OnInputEndEdit);
        buyInputField.onValueChanged.AddListener(OnInputValueChanged);
        confirmBtn.onClick.AddListener(ConfirmBuy);
        buyPanelStartPos = buyPanel.transform.localPosition;
        buyPanelEndPos = new Vector2(0, 268);
        desPanelStartPos = desPanel.transform.localPosition;
        desPanelEndPos = new Vector2(0, -545);
    }

    public void UpdateData()
    {
        // 将未解锁的道具排在下面，已经解锁的排在上面
        // List<ItemData> newList = new List<ItemData>(GameManager.Instance.ItemDataDict.Values);
        List<ItemData> newList = GameManager.Instance.ItemDataDict.Values.ToList();
        newList = newList
            .Where(item => !GameManager.Instance.ItemLockDict[item.id])  // 先选已解锁的
            .Concat(newList.Where(item => GameManager.Instance.ItemLockDict[item.id]))  // 再连未解锁的
            .ToList();
        for (int i = 0; i < content.transform.childCount; i++) { Destroy(content.transform.GetChild(i).gameObject); }
        foreach (ItemData item in newList)
        {
            GameObject obj = ResMgr.Instance.load<GameObject>("UI/buyItem", content.transform);
            obj.GetComponent<ShopItem>().UpdateShopItemData(item);
        }
    }
    

    #region 购买面板相关
    public void ShowBuyPanel(ItemData item)
    {
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.menuBtnClick, transform.position);
        currentItem = item;
        itemName.text = $"购买{item.name}:";
        currentTotalCoin.text = "0";
        mask.SetActive(true);
        buyPanel.transform.DOLocalMove(buyPanelEndPos, 0.5f);
    }
    public void DeShowBuyPanel()
    {
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
        mask.SetActive(false);
        buyInputField.text = "";
        buyPanel.transform.DOLocalMove(buyPanelStartPos, 0.5f);
    }

    /// <summary>
    /// 实时响应输入变化
    /// </summary>
    private void OnInputValueChanged(string inputValue)
    {
        // 实时响应输入变化
        if (int.TryParse(inputValue, out var quantity))
        {
            currentBuyQuantity = Mathf.Clamp(quantity, 1, 9999);
            currentTotalCoin.text = (currentItem.price * currentBuyQuantity).ToString();
        }
        else if (string.IsNullOrEmpty(inputValue))
        {
            currentBuyQuantity = 0;
            currentTotalCoin.text = "0";
        }
    }

    /// <summary>
    /// 输入结束时进行验证和处理
    /// </summary>
    private void OnInputEndEdit(string inputValue)
    {
        // 输入结束时进行验证和处理
        if (string.IsNullOrEmpty(inputValue))
        {
            buyInputField.text = "0";
            currentBuyQuantity = 0;
            currentBuyQuantity = 0;
            currentTotalCoin.text = "0";
        }
        else if (!int.TryParse(inputValue, out var quantity) || quantity <= 0)
        {
            buyInputField.text = "0";
            currentBuyQuantity = 0;
            currentBuyQuantity = 0;
            currentTotalCoin.text = "0";
            TipPanel tipPanel = UIManager.Instance.openPanel<TipPanel>();
            tipPanel.ShowTip("请输入有效数字");
            tipPanel.transform.localPosition = new Vector2(0, -380);
        }
        
    }
    
    /// <summary>
    /// 确认购买，减钱
    /// </summary>
    private void ConfirmBuy()
    {
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
        if (currentBuyQuantity * currentItem.price > GameManager.Instance.Coin)
        {
            TipPanel tipPanel = UIManager.Instance.openPanel<TipPanel>();
            tipPanel.ShowTip("金币不足");
            tipPanel.transform.localPosition = new Vector2(0, -380);
        }
        else
        {
            currentHasCoin.text = (GameManager.Instance.Coin -= currentBuyQuantity * currentItem.price).ToString();
            GameManager.Instance.ItemHasNumDict[currentItem.id] += currentBuyQuantity;
            DeShowBuyPanel();
        }
    }

    #endregion

    #region 描述面板相关

    public void ShowDesPanel(ItemData item)
    {
        currentTween.Kill();
        if (GameManager.Instance.ItemLockDict[item.id])
        { desText.text = "抓鬼完成委托解锁道具"; }
        else
        { desText.text = item.des; }
        currentTween = desPanel.transform.DOLocalMove(desPanelEndPos, 0.5f);
    }

    public void DeShowDesPanel()
    {
        currentTween.Kill();
        currentTween = desPanel.transform.DOLocalMove(desPanelStartPos, 0.1f);
    }

    #endregion
    
    
    void OnDisable()
    {
        currentTween.Kill();
        closeBtn.onClick.RemoveAllListeners();
        backBtn.onClick.RemoveAllListeners();
        confirmBtn.onClick.RemoveAllListeners();
        buyInputField.onEndEdit.RemoveAllListeners();
    }
}
