using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    private ShopPanel shopPanel;
    public Image itemImage;
    public Text itemName;
    public Text itemPrice;
    public Button buyBtn;
    private ItemData currentItem;
    void Start()
    {
        shopPanel = GameObject.Find("ShopPanel").GetComponent<ShopPanel>();
        buyBtn.onClick.AddListener(() =>
        {
            shopPanel.ShowBuyPanel(currentItem);
        });
    }
    
    public void UpdateShopItemData(ItemData item)
    {
        currentItem = item;
        if (item.id == 1 || item.id == 2)
        {
            buyBtn.gameObject.SetActive(false);
            itemPrice.text = "免费";
            itemName.text = item.name;
        }
        else if (!GameManager.Instance.ItemLockDict[item.id])
        {
            buyBtn.gameObject.SetActive(true);
            itemPrice.text = $"¥{item.price}";
            itemName.text = item.name;
        }
        else if (GameManager.Instance.ItemLockDict[item.id])
        {
            buyBtn.gameObject.SetActive(false);
            itemName.text = "xxx";
            itemPrice.text = "xxx";
        }
        itemImage.sprite = ResMgr.Instance.load<Sprite>($"Image/{item.icon}");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        shopPanel.ShowDesPanel(currentItem);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        shopPanel.DeShowDesPanel();
    }
}
