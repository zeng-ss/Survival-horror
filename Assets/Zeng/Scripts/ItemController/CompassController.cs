using UnityEngine;
using DG.Tweening;

/// <summary>
/// 罗盘控制器
/// </summary>
public class CompassController : ItemController
{
    // 指针
    public GameObject pointer; 
    
    //道具时间
    private float backHoldTimer; // 收回道具计时器

    // 道具的位置
    private Vector3 equipPos;
    private Vector3 equipRot;
    [Header("道具开始作用时的位置")] public Vector3 openPosition;
    
    // 道具状态
    private SphereCollider sphereCollider;
    private GameObject effect ; // 有鬼反应的效果
    [HideInInspector] public bool isWork; // 道具是否在工作
    private float coolTimer; // 道具开始作用时起作用的间隔时间计时器
    [Header("道具开始作用时起作用的间隔时间（间隔多久指向一次）")] public float coolTime;
    
    // 鬼是否进入工作范围
    private bool isGhostInArea;
    
    private new void Start()
    {
        base.Start();
        equipRot = pointer.transform.localEulerAngles;
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.enabled = false;
    }

    private new void Update()
    {
        base.Update();
        if (player == null || player.cameraOne == null || ghost == null)
        {
            return;
        }
        // 只有捡起来且已经装备了才可以开始道具功能
        if (transform.parent == player.cameraOne.transform && gameObject.activeInHierarchy && !isOpen)
        {
            if (KeyBindingManager.Instance.GetAction(GameAction.UsingItem))
            { OpenIncreaseFillAmount(); }
            else { OpenDecreaseFillAmount(); }
        }
        if (isWork) // 判断是否正在工作
        {
            // 收回道具
            if (KeyBindingManager.Instance.GetAction(GameAction.UsingItem))
            { BackIncreaseFillAmount(); }
            else
            { BackDecreaseFillAmount(); }
            // 罗盘开启工作效果
            StartWork();
        }
        
    }

    private void StartWork()
    {
        if (isGhostInArea)
        {
            // 判断是否已经创建了效果
            if (effect == null &&
                (ghost.name == "ShuiGui"|| ghost.name == "DiaoSiGui"||
                 ghost.name == "HuaPi"|| ghost.name == "NanChanGui"))
            {
                effect = ResMgr.Instance.load<GameObject>(
                    "Model/effects/AuraChargeRed",transform);
                effect.transform.localScale = Vector3.one;
            }
            coolTimer += Time.deltaTime;
            if (coolTimer >= coolTime) // 每间隔 coolTime 秒指向一次
            {
                // 计算方向向量
                Vector3 direction = (ghost.transform.position - pointer.transform.position).normalized;
                Quaternion targetRotation = Quaternion.FromToRotation(Vector3.right, direction);
                // 应用旋转（保持 Y 轴向上，避免倾斜）
                pointer.transform.rotation = targetRotation;
                coolTimer = 0;
            }
        }
        else
        {
            // 如果有效果就销毁
            if (effect != null) { Destroy(effect); }
        }
    }

    #region 开启和收回罗盘的方法

        /// <summary>
    /// 开启罗盘的增加进度方法
    /// </summary>
    public override void OpenIncreaseFillAmount()
    {
        if (isOpen) return;
        holdTimer += Time.deltaTime;
        player.isStartOpen = true;
        HorProgressPanel horProgressPanel = GetProgressPanel();
        horProgressPanel.tipText.text = "道具打开中.....";
        horProgressPanel.IncreaseFillAmount(duration,holdTimer);
        if (holdTimer >= duration) // 进度完成，开启道具功能
        {
            isOpen = true; holdTimer = 0;
            equipPos = transform.localPosition;
            UIManager.Instance.closePanel<HorProgressPanel>();
            // 动画平滑移动到指定位置
            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOLocalMove(openPosition, 1));
            seq.Join(transform.DOLocalRotate(openRotation, 1));
            // 动画完成之后再开启功能
            seq.OnComplete(() =>
            {
                // 开启道具功能
                sphereCollider.enabled = true; isWork = true; // 切换工作状态
            });
        }
    }
    
    /// <summary>
    /// 开启罗盘减少进度方法
    /// </summary>
    public override void OpenDecreaseFillAmount()
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
    /// 收回罗盘的增加进度方法
    /// </summary>
    private void BackIncreaseFillAmount()
    {
        backHoldTimer += Time.deltaTime;
        HorProgressPanel horProgressPanel = GetProgressPanel();
        horProgressPanel.tipText.text = "道具收回中.....";
        horProgressPanel.IncreaseFillAmount(duration, backHoldTimer);
        if (backHoldTimer >= duration)
        {
            // 如果有效果就销毁
            if (effect != null) { Destroy(effect); }
            isWork = false; backHoldTimer = 0;
            UIManager.Instance.closePanel<HorProgressPanel>();
            // 动画平滑移动到指定位置
            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOLocalMove(equipPos, 1));
            seq.Join(pointer.transform.DOLocalRotate(equipRot, 1));
            // 动画完成之后再开启功能
            seq.OnComplete(() =>
            {
                // 开启道具功能
                sphereCollider.enabled = false; isOpen = false; // 回到初始装备状态
                player.isStartOpen = false; isGhostInArea = false;
            });
        }
    }
    
    /// <summary>
    /// 收回罗盘减少进度方法
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
    
    private void OnTriggerEnter(Collider other) { if (other.CompareTag("Ghost")) { isGhostInArea = true; } }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("Ghost")) { isGhostInArea = false; } }
    
    
}
