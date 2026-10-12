using LeTai.TrueShadow;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChooseItemItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 道具图标
    public Image itemIcon;
    
    // 按钮和文字
    public Button addBtn;
    public Button deBtn;
    public Text itemName;
    public Text currentHasItemNum;
    
    public ItemData currentItem;
    private LookSelectPanel lookSelectPanel;
    [HideInInspector] public TrueShadow trueShadow;

    void Start()
    {
        trueShadow = GetComponent<TrueShadow>();
        lookSelectPanel = UIManager.Instance.getPanel<LookSelectPanel>();
        addBtn.onClick.AddListener(OnAddItemClick);
        deBtn.onClick.AddListener(OnDeItemClick);
    }

    /// <summary>
    /// 增加选择道具数量
    /// </summary>
    private void OnAddItemClick()
    {
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
        if (GameManager.Instance.ItemHasNumDict[currentItem.id] > 0)
        {
            GameManager.Instance.ItemSelectNumDict[currentItem.id]++;
            GameManager.Instance.ItemHasNumDict[currentItem.id]--;
            lookSelectPanel.AddItem(currentItem);
            currentHasItemNum.text = GameManager.Instance.ItemHasNumDict[currentItem.id].ToString();
        }
        else
        {
            TipPanel tipPanel = UIManager.Instance.openPanel<TipPanel>();
            tipPanel.ShowTip($"{currentItem.name}已经没有库存了");
            tipPanel.transform.localPosition = new Vector2(0, 430);
        }
    }
    
    /// <summary>
    /// 减少选择道具数量
    /// </summary>
    private void OnDeItemClick()
    {
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
        if (GameManager.Instance.ItemSelectNumDict[currentItem.id] > 0)
        {
            GameManager.Instance.ItemSelectNumDict[currentItem.id]--;
            GameManager.Instance.ItemHasNumDict[currentItem.id]++;
            lookSelectPanel.DeItem(currentItem);
            currentHasItemNum.text = GameManager.Instance.ItemHasNumDict[currentItem.id].ToString();
        }
        else
        {
            TipPanel tipPanel = UIManager.Instance.openPanel<TipPanel>();
            tipPanel.ShowTip($"{currentItem.name}已经减完了");
            tipPanel.transform.localPosition = new Vector2(0, 430);
        }
    }
    
    void OnDestroy() { addBtn.onClick.RemoveAllListeners(); deBtn.onClick.RemoveAllListeners(); }

    public void UpdateData(ItemData item)
    {
        currentItem = item;
        // 强制读取最新的库存数量（关键）
        if (GameManager.Instance.ItemHasNumDict.ContainsKey(item.id))
        {
            currentHasItemNum.text = GameManager.Instance.ItemHasNumDict[item.id].ToString();
        }
        else
        {
            currentHasItemNum.text = "0"; // 兜底
        }
        itemIcon.sprite = ResMgr.Instance.load<Sprite>($"Image/{item.icon}");
        itemName.text = item.name;
        UIManager.Instance.getPanel<ChooseItemPanel>().desDict[item.id] = item.des;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        trueShadow.Spread = 0.7f; // 开启特效
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        trueShadow.Spread = 0f; // 关闭特效
    }
}
