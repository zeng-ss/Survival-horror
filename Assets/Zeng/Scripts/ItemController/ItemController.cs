using System.Collections;
using UnityEngine;
using DG.Tweening;
/// <summary>
/// 道具脚本，继承了道具接口，实现了接口的公共的基本方法
/// </summary>
public class ItemController : MonoBehaviour,I_Item
{
    #region 数据

    protected GameObject catchArea; // 抓鬼区域
    
    // 所有有开启时间的道具的状态
    protected float holdTimer;
    protected float duration = 3;
    protected bool isOpen; // 道具是否打开
    
    // 道具状态
    private float timer; // 计时器（用来实现正弦函数的边缘光）
    private bool hasReset; // 添加一个重置的标志位
    [HideInInspector] public bool isEqupt;

    // 玩家 相机
    protected CameraController cameraController;
    protected GhostController ghost;
    protected PlayerController player;

    // 组件
    private Outline outline;
    private Rigidbody itemRb;
    private MeshCollider itemMeshCollider; // MeshCollider字段
    private BoxCollider itemBoxCollider;   // BoxCollider字段

    // 模型
    private GameObject flashlight; //手电筒模型
    private GameObject blackHole;  //黑板的模型

    // 书相关
    protected bool isStartPost;      //面板是否开始后处理
    protected Vector3 startPosition; //书本（或者其他道具）开始的位置

    [Header("碰撞体设置")]
    [Tooltip("选择使用的碰撞体类型")]
    public ColliderType colliderType = ColliderType.BoxCollider; // 默认使用 BoxCollider（性能更优）
    
    // 位置设置
    [Header("道具在手中位置偏移")] private Vector3 itemHandOffset = new (0.69f, -0.43f, 0.82f);
    [Header("手电筒在手中位置偏移")] private Vector3 flashHandOffset = new (0, -0.3f, 0.5f);
    [Header("手电筒在手中的旋转值")] private Vector3 flashHandRotate =  new (-83, -180f, 0.4f);
    [Header("道具开始作用时的旋转")] public Vector3 openRotation;
    
    // 碰撞体类型枚举（供Inspector选择）
    public enum ColliderType
    {
        BoxCollider,    // 盒子碰撞体（推荐，性能优）
        MeshCollider    // 网格碰撞体（原有类型）
    }

    #endregion

    public void Start()
    {
        StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        yield return null;
        ghost = GameObject.FindGameObjectWithTag("Ghost").GetComponent<GhostController>();
        outline = GetComponent<Outline>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        cameraController = GameObject.FindGameObjectWithTag("CameraOne").GetComponent<CameraController>();
        startPosition = transform.position;
        blackHole = GameObject.Find("Mesh2");
        //拿到手电筒
        flashlight = GameObject.Find("Flashlight");
        itemRb = GetComponent<Rigidbody>();
        // 初始化碰撞体（根据选择的类型创建/激活）
        InitCollider();

        // 物理设置（保持原有逻辑）
        if (transform.parent != blackHole.transform && transform.parent != player.cameraOne.transform)
        {
            itemRb.isKinematic = false;
            itemRb.useGravity = true;
            // 只给启用的碰撞体设置convex（仅MeshCollider需要）
            if (colliderType == ColliderType.MeshCollider && itemMeshCollider != null)
                itemMeshCollider.convex = true;
        }
        else { itemRb.isKinematic = true; itemRb.useGravity = false; }
        //装备手电筒
        FlashlightEquip(cameraController.transform);
    }
    
    /// <summary>
    /// 初始化碰撞体：根据选择的类型激活/创建对应的碰撞体
    /// </summary>
    private void InitCollider()
    {
        // 获取现有碰撞体（如果已手动添加）
        itemMeshCollider = GetComponent<MeshCollider>();
        itemBoxCollider = GetComponent<BoxCollider>();
        switch (colliderType)
        {
            case ColliderType.BoxCollider:
                // 启用BoxCollider，禁用MeshCollider
                if (itemBoxCollider == null)
                    itemBoxCollider = gameObject.AddComponent<BoxCollider>(); // 没有则创建
                itemBoxCollider.enabled = true;

                if (itemMeshCollider != null)
                    itemMeshCollider.enabled = false; // 禁用MeshCollider
                break;

            case ColliderType.MeshCollider:
                // 启用MeshCollider，禁用BoxCollider
                if (itemMeshCollider == null)
                    itemMeshCollider = gameObject.AddComponent<MeshCollider>(); // 没有则创建
                itemMeshCollider.convex = true; // 确保勾选convex
                itemMeshCollider.enabled = true;

                if (itemBoxCollider != null)
                    itemBoxCollider.enabled = false; // 禁用BoxCollider
                break;
        }
    }

    #region 道具基础方法
    
    /// <summary>
    /// 装备手电筒的方法
    /// </summary>
    private void FlashlightEquip(Transform parentTransform)
    {
        flashlight.SetActive(true);
        flashlight.GetComponent<Rigidbody>().isKinematic = true;
        flashlight.GetComponent<Rigidbody>().useGravity = false;
        flashlight.transform.SetParent(parentTransform);
        flashlight.transform.localPosition = flashHandOffset;
        flashlight.transform.localRotation = Quaternion.Euler(flashHandRotate); // 直接设置正确的旋转
        DOVirtual.DelayedCall(0.5f, () => { flashlight.GetComponentInChildren<Light>().enabled = true; });
    }
    
    /// <summary>
    /// 拾起道具
    /// </summary>
    /// <param name="parentTransform">拾起的道具是parentTransform的孩子</param>
    public virtual void OnPickUp(Transform parentTransform)
    {
        gameObject.transform.SetParent(parentTransform);
        itemRb.isKinematic = true;
        itemRb.useGravity = false;
        gameObject.SetActive(false);
        //播放音效
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.pickUpClip,transform.position);
    }

    /// <summary>
    /// 丢弃道具
    /// </summary>
    public virtual void OnDrop()
    {
        // 恢复物理设置
        transform.SetParent(null);
        itemRb.isKinematic = false;
        itemRb.useGravity = true;
        isEqupt = false;
        // 添加丢弃力度
        itemRb.AddForce(transform.forward * 5f, ForceMode.Impulse);
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 装备道具
    /// </summary>
    /// <param name="handTransform">装备的道具的父物体（手）</param>
    public virtual void OnEquip(Transform handTransform)
    {
        // 切换到手中位置
        transform.SetParent(handTransform);
        transform.localPosition = itemHandOffset;
        transform.localRotation = Quaternion.identity;
        gameObject.SetActive(true);
        isEqupt = true;
    }

    /// <summary>
    /// 卸下道具
    /// </summary>
    public virtual void OnUnequip() { isEqupt = false; gameObject.SetActive(false); }

    /// <summary>
    /// 查看书（日记）的方法
    /// </summary>
    /// <param name="targetTransform">目标位置</param>
    public void OnLookDiary(Transform targetTransform)
    {
        //播放音效
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.pickUpClip,transform.position);
        itemRb.isKinematic = true;
        itemRb.useGravity = false;
        //设置位置
        Vector3 target = targetTransform.position + new Vector3(0.8f, -0.6f, 0);
        Sequence s = DOTween.Sequence();
        //移动到指定位置
        s.Append(transform.DOMove(target, 1f));
        //看向玩家
        s.Join(transform.DOLookAt(target, 1f)).OnComplete(() =>
        {
            isStartPost = true;
            player.isLookDiary = true;
            player.guideLine.isStart = true;
            player.guideLine.gameObject.SetActive(true);
            EventCenter.Instance.EventTrigger(GameEvent.光标出现);
            cameraController.StartLookDiaryPost();
            UIManager.Instance.openPanel<DiaryPanel>(true);
            if (catchArea == null)
            {
                // 创建抓鬼区域
                catchArea = player.areas[Random.Range(0, player.areas.Length)];
                var effect  = ResMgr.Instance.load<GameObject>("Model/effects/SpinPortalBlue",catchArea.transform);
                if (effect == null)
                {
                    print("抓鬼区域为空！");
                    return;
                }
                player.guideLine.endTarget = effect.transform;
            }
        });
        
    }
    
    /// <summary>
    /// 书（日记）的放回方法
    /// </summary>
    public void OnPutBackDiary()
    {
        if (catchArea != null)
        {
            TipPanel tipPanel = UIManager.Instance.openPanel<TipPanel>();
            tipPanel.transform.localPosition = new Vector2(0, 430);
            tipPanel.showTime = 3;
            tipPanel.ShowTip("抓鬼区域已打开，请把鬼引导至区域");
        }
        itemRb.isKinematic = false;
        itemRb.useGravity = true;
        UIManager.Instance.closePanel<DiaryPanel>();
        EventCenter.Instance.EventTrigger(GameEvent.光标消失);
        transform.position = startPosition;
        transform.rotation = Quaternion.Euler(-90, 0, 0);
    }

    #endregion
    

    public void Update()
    {
        if (ghost == null)
        {
            return;
        }
        if (isEqupt && gameObject.name != "Flashlight")
        {
            hasReset = false;
            // 装备之后开启循环发光效果
            timer += Time.deltaTime * 2;
            outline.OutlineWidth = Mathf.Lerp(0, 5, (Mathf.Sin(timer) + 1) * 0.5f);
        }
        else
        {
            if (!hasReset)
            {
                hasReset = true; // 标记已经重置
                timer = 0;
                if (outline == null)
                {
                    print("kong!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    return;
                }
                outline.OutlineWidth = 0;
                isStartPost = false;
            }
        }
    }
    
    #region 进度方法

    /// <summary>
    /// 增加进度方法
    /// </summary>
    public virtual void OpenIncreaseFillAmount()
    {
        if (isOpen) return;
        holdTimer += Time.deltaTime;
        player.isStartOpen = true;
        HorProgressPanel horProgressPanel = GetProgressPanel();
        horProgressPanel.tipText.text = "道具打开中.....";
        horProgressPanel.IncreaseFillAmount(duration,holdTimer);
    }
    
    /// <summary>
    /// 减少进度方法
    /// </summary>
    public virtual void OpenDecreaseFillAmount()
    {
        if (isOpen) return;
        // 检测是否有进度面板存在（相当于检测是否还有进度）
        if (UIManager.Instance.getPanel<HorProgressPanel>() == null) { return; }
        // 还有进度
        HorProgressPanel horProgressPanel = UIManager.Instance.getPanel<HorProgressPanel>();
        if (horProgressPanel.currentFillAmount > 0)
        {
            player.isStartOpen = true;
            holdTimer -= Time.deltaTime * 2;
            horProgressPanel.DecreaseFillAmount(duration, holdTimer);
        }
        else if(horProgressPanel.currentFillAmount <= 0)
        {
            player.isStartOpen = false;
            isOpen = false; holdTimer = 0;
            UIManager.Instance.closePanel<HorProgressPanel>();
        }
    }
    
    /// <summary>
    /// 拿到进度面板
    /// </summary>
    public HorProgressPanel GetProgressPanel()
    {
        UIManager.Instance.openPanel<HorProgressPanel>().transform.position = new Vector3(0, 350, 0);
        return UIManager.Instance.getPanel<HorProgressPanel>();
    }

    #endregion
}
