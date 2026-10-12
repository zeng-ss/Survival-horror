using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 道具选择面板
/// </summary>
public class ChooseItemPanel : BasePanel
{
    // 容器，组件
    public GameObject content;
    private GridLayoutGroup gridLayout; // 添加GridLayoutGroup组件
    
    // 按钮，文字
    public Button saveBtn;
    public Button backBtn;
    public Text description;

    // 存储每个道具的描述信息
    public Dictionary<int, string> desDict = new ();

    // 过场背景动画
    private MainCameraController mainCamera;

    private void OnEnable()
    {
        mainCamera = GameObject.Find("MainCamera").GetComponent<MainCameraController>();
        // 配置网格布局
        ConfigureGridLayout();
        
        // 保存选择的道具信息字典 GameManager.Instance.ItemIconDic
        saveBtn.onClick.AddListener(() =>
        {
            mainCamera = GameObject.Find("MainCamera").GetComponent<MainCameraController>();
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
            UIManager.Instance.closePanel<ChooseItemPanel>();
            UIManager.Instance.closePanel<LookSelectPanel>();
            mainCamera.MoveToLookAtPerson();
        });
        // 退出不保存
        backBtn.onClick.AddListener(() =>
        {
            mainCamera = GameObject.Find("MainCamera").GetComponent<MainCameraController>();
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
            UIManager.Instance.closePanel<ChooseItemPanel>();
            UIManager.Instance.closePanel<LookSelectPanel>();
            mainCamera.MoveBackToStart();
        });
    }

    private void OnDisable()
    {
        saveBtn.onClick.RemoveAllListeners();
        backBtn.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// 配置网格布局为2列
    /// </summary>
    private void ConfigureGridLayout()
    {
        // 如果content上没有GridLayoutGroup，就添加一个
        gridLayout = content.GetComponent<GridLayoutGroup>();
        // 配置为2列
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 2;
        // 设置间距和大小（根据你的UI调整这些值）
        gridLayout.cellSize = new Vector2(350f, 200f);    // 每个item的大小
        gridLayout.spacing = new Vector2(10f, 50f);       // 间距
        gridLayout.childAlignment = TextAnchor.UpperLeft; // 对齐方式
        
    }

    /// <summary>
    /// 更新道具信息
    /// </summary>
    public void UpdateItemItem()
    {
        // 先销毁
        foreach (Transform child in content.transform) { Destroy(child.gameObject); }
        foreach (ItemData item in GameManager.Instance.ItemDataDict.Values)
        {
            if (!GameManager.Instance.ItemLockDict[item.id]) 
            {
                GameObject obj = ResMgr.Instance.load<GameObject>("UI/ChooseItemItem",content.transform);
                obj.GetComponent<ChooseItemItem>().UpdateData(item);
            }
        }
    }

    /// <summary>
    /// 更新信息的方法
    /// </summary>
    private void UpdateDescription()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            foreach (var result in results)
            {
                if (result.gameObject.CompareTag("ItemUI"))
                {
                    ChooseItemItem item = result.gameObject.GetComponent<ChooseItemItem>();
                    int id = item.currentItem.id;
                    if (!GameManager.Instance.ItemLockDict[id])
                    {
                        description.text =
                            $"<b>道具名字：</b>{item.currentItem.name}\n" +
                            $"<b>基础描述：</b>{desDict[id]}\n<b>功能：</b>{item.currentItem.function}";
                    }
                    break;
                }
                description.text = " ";
            }
        }
    }
    void Update()
    {
        // 更新信息的方法
        UpdateDescription();
    }
}
