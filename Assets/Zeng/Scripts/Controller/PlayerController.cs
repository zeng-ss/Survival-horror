using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour,IState_MachineOwner
{
    #region 数据

    [HideInInspector] public AutoGenerateNavPath guideLine; // 导航线
    // ===== 修复 PlayerModel 属性的 get 方法（报错核心行）=====
    [HideInInspector] public PlayerModel PlayerModel;
    [HideInInspector] public GameObject exitDoor;
    [HideInInspector] public GameObject[] areas;
    public PlayerModel playerModel
    {
        get
        {
            // 第一步：先检查自身是否存活！
            if (!IsSelfValid()) return null;

            // 第二步：再检查子组件
            if (PlayerModel == null || (PlayerModel != null && PlayerModel.gameObject == null))
            {
                // 加 try-catch 防止销毁过程中调用报错
                try
                {
                    PlayerModel = GetComponentInChildren<PlayerModel>(true);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"获取PlayerModel失败：{e.Message}", this);
                    PlayerModel = null;
                }
                if (PlayerModel == null)
                    Debug.LogError("找不到PlayerModel组件（自身未销毁）", this);
            }
            return PlayerModel;
        }
    }
    private State_Machine state_machine;
    [HideInInspector] public CharacterController characterController;
    public CharacterController CharacterController
    {
        get
        {
            if (!IsSelfValid()) return null; // 先检查自身

            if (characterController == null || (characterController != null && characterController.gameObject == null))
            {
                try
                {
                    characterController = GetComponent<CharacterController>();
                    if (characterController == null)
                    {
                        characterController = gameObject.AddComponent<CharacterController>();
                        Debug.LogWarning("自动添加了CharacterController组件", this);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"获取/添加CharacterController失败：{e.Message}", this);
                    characterController = null;
                }
            }
            return characterController;
        }
        set => characterController = value;
    }
    //角色控制器

       #endregion
    
    #region 配置参数

    #region 理智值，体力值

       public float CurrentSanity
       { 
           get => currentSanity;
           set { currentSanity = value; EventCenter.Instance.EventTrigger(GameEvent.理智值变化,currentSanity); }
       }
    private Light[] allLights;         // 拿到所有光源
    private float deSanityTimer;       // 减少理智值计时器
    private float inSanityTimer;       // 增加理智值计时器
    private float currentSanity = 100; // 角色当前的理智值 （默认100）
    public float CurrentBodyPower
    {
        get => currentBodyPower;
        set { currentBodyPower = value; EventCenter.Instance.EventTrigger(GameEvent.体力值变化,currentBodyPower); }
    }
    private float currentBodyPower = 100;             // 角色当前的体力值 （默认100）
    [HideInInspector] public bool isPowerRecovering;  // 是否在恢复期
    [HideInInspector] public float powerRecoverTimer; // 体力恢复计时器

    #endregion

    #region 角色动作参数

    [Header("重力")] private float gravity = -9.8f;
    [Header("跳跃幅度")] public float jumpPower;
    [Header("跑步的速度")] public float RunSpeed;
    [Header("走路的速度")] public float WalkSpeed;
    [Header("旋转速度")] public float RotateSpeed;
    [Header("走切换到跑的速度")] public float WalkToRunSpeed;
    [HideInInspector] [Header("角色的垂直速度")] public Vector3 volecity;
    [HideInInspector] [Header("当前的移动速度")] public float currentMoveSpeed;

    #endregion

    #region 角色状态

    [HideInInspector] [Header("是否在地面")] public bool isOnGround;
    [HideInInspector] [Header("是否开启重力")] public bool hasGravity;
    [HideInInspector] [Header("上一个状态")] public Player_StateType lastState;
    [HideInInspector] [Header("当前的状态")] public Player_StateType currentState;

    #endregion

    #region 脚步，相机

    private float lastFootStepTime; 
    private readonly float footStepCoolDown = 0.3f; // 脚步声最小间隔（根据动画节奏调整，例如 0.3秒）

    [HideInInspector] public Camera cameraOne;   // 第一人称相机
    [HideInInspector] public Camera cameraThird; // 第三人称相机
    [HideInInspector] [Header("当前的相机")] public Camera currentCamera;
    public Camera CurrentCamera
    {
        get
        {
            if (!IsSelfValid()) return null; // 先检查自身

            if (currentCamera == null || (currentCamera != null && currentCamera.gameObject == null))
            {
                try
                {
                    var camOne = GameObject.FindGameObjectWithTag("CameraOne");
                    var camThird = GameObject.FindGameObjectWithTag("CameraThird");
                    if (camOne != null)
                    {
                        currentCamera = camOne.GetComponent<Camera>();
                        cameraOne = currentCamera;
                    }
                    else if (camThird != null)
                    {
                        currentCamera = camThird.GetComponent<Camera>();
                        cameraThird = currentCamera;
                    }
                    Debug.Log($"自动重新获取相机: {currentCamera?.name}", this);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"重新获取相机失败：{e.Message}", this);
                    currentCamera = null;
                }
            }
            return currentCamera;
        }
        set => currentCamera = value;
    }

    #endregion

    #region 门和电梯

    [HideInInspector] [Header("判断角色是否面对门")] public bool isToDoor;
    [HideInInspector] [Header("当前角色所在楼层")] public int currentFloor;
    [HideInInspector] [Header("是否在电梯的区域内")] public bool isInElevatorArea;
    [HideInInspector] [Header("当前交互的门")] public DoorRotation currentDoor;
    [HideInInspector] [Header("当前交互的电梯")] public ElevatorRotate currentElevator;
    [HideInInspector] [Header("当前交互的小门")] public DragOpenDoor currentDragDoor;

    #endregion

    #region 道具

    private I_Item currentBookItem;            // 当前的书（日记）的道具
    [HideInInspector] public bool isStartOpen; // 道具是否在开启中或者在工作中
    [Header("拾起的道具的列表")] private List<I_Item> items = new ();
    [HideInInspector] [Header("当前选中的道具索引")] public int currentItemIndex = -1;
    [HideInInspector] [Header("判断是否在看日记")] public bool isLookDiary;

    // 基础效果
    [Header("描边")] private Outline outline;
    [SerializeField] [Header("振动源")] private CinemachineImpulseSource  impulseSource;

    #endregion

    #endregion
    
    // 鬼魂跳脸参数
    private Quaternion initialLookRotation;
    private bool isStunned;
    private float stunnedTimer;
    void OnEnable() // 启用时先检查自身 初始化信息
    {
        if (IsSelfValid()) Init();
    } 
    
    private void Init()
    {
        if (!IsSelfValid()) return; // 双重保险
        // 先清空已销毁的引用
        ClearDestroyedReferences();
        try
        {
            areas = GameObject.FindGameObjectsWithTag("CatchArea");
            
            var camOneObj = GameObject.FindGameObjectWithTag("CameraOne");
            cameraOne = camOneObj?.GetComponent<Camera>();
            
            var camThirdObj = GameObject.FindGameObjectWithTag("CameraThird");
            cameraThird = camThirdObj?.GetComponent<Camera>();

            var lineObj = GameObject.Find("Line");
            guideLine = lineObj?.GetComponent<AutoGenerateNavPath>();

            exitDoor = GameObject.Find("ExitDoor");
            if (exitDoor != null) exitDoor.gameObject.SetActive(false);

            characterController = GetComponent<CharacterController>();
            PlayerModel = GetComponentInChildren<PlayerModel>();

            allLights = FindObjectsOfType<Light>();
            if (UIManager.Instance != null) // 加UIManager判空
                UIManager.Instance.openPanel<PlayerProPanel>();

            if (cameraThird != null) cameraThird.gameObject.SetActive(false);
            currentCamera = cameraOne;
            if (currentCamera != null)
                originalCameraLocalPos = currentCamera.transform.localPosition;

            if (guideLine != null) guideLine.gameObject.SetActive(false);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PlayerController.Init 失败：{e.Message}", this);
            return;
        }
        isOnGround = true;
        hasGravity = true;
        // 初始化模型前先判空
        if (playerModel != null)
            playerModel.Init(OnFootSetp);
        else
            Debug.LogError("PlayerModel为空，无法初始化", this);

        // 初始化状态机
        state_machine = new State_Machine();
        state_machine.Init(this);
        ChangeState(Player_StateType.Idle);
    }

    #region 确保不为空

    /// <summary>
    /// 检查当前PlayerController是否有效（未被销毁）
    /// </summary>
    private bool IsSelfValid()
    {
        // Unity特有的销毁检查：gameObject==null 表示对象已销毁
        if (this == null || gameObject == null)
        {
            //Debug.LogWarning("PlayerController已被销毁，跳过当前操作", this);
            return false;
        }
        return true;
    }
    
    /// <summary>
    /// 清空已被销毁的Unity对象引用
    /// </summary>
    private void ClearDestroyedReferences()
    {
        if (!IsSelfValid()) return; // 自身已销毁则无需清理
        cameraOne = (cameraOne != null && cameraOne.gameObject != null) ? cameraOne : null;
        cameraThird = (cameraThird != null && cameraThird.gameObject != null) ? cameraThird : null;
        currentCamera = (currentCamera != null && currentCamera.gameObject != null) ? currentCamera : null;
        PlayerModel = (PlayerModel != null && PlayerModel.gameObject != null) ? PlayerModel : null;
        guideLine = (guideLine != null && guideLine.gameObject != null) ? guideLine : null;
        exitDoor = (exitDoor != null && exitDoor.gameObject != null) ? exitDoor : null;
        characterController = (characterController != null && characterController.gameObject != null) ? characterController : null;
    }
    void Update()
    {
        // 处理被困状态计时
        if (isStunned && stunnedTimer > 0)
        {
            stunnedTimer -= Time.deltaTime;
            if (stunnedTimer <= 0)
            {
                SetStunned(false); // 时间到后恢复移动能力
            }
        }
        SwitchCamera();
        if(!hasGravity) return; // 是否打开重力
        CharacterController.Move(volecity * Time.deltaTime); //先移动
        // 判断是否在地面
        isOnGround = characterController.isGrounded;
        if (isOnGround) { volecity.y = -2f; }
        else { volecity.y += gravity * Time.deltaTime; }
        
        UpdateBodyPowerRecovery();
        UpdateSanity();
        if (KeyBindingManager.Instance.isEscKey) return; // 如果正在设置按键（打开按键设置面板）则返回
        // 检测门的交互按键
        if (KeyBindingManager.Instance.GetActionDown(GameAction.Interact))
        {
            // 触发门交互事件
            if (currentDoor != null && isToDoor) { EventCenter.Instance.EventTrigger(GameEvent.开门事件, currentDoor); }
            if (currentElevator != null && isInElevatorArea)
            {
                //打开选择目的地电梯面板
                SoundAudioPool.Instance.PlaySound(SoundManager.Instance.mesBtnClick,transform.position);
                UIManager.Instance.openPanel<SelectFloorPanel>();
                EventCenter.Instance.EventTrigger(GameEvent.光标出现);
            }
        }
        CrosshairRaycaster(); // 准心射线和拾取道具方法
        SwitchItem();    // 切换道具方法
        // 检测丢弃
        if (KeyBindingManager.Instance.GetActionDown(GameAction.Drop))
        {
            if (isLookDiary)
            {
                // 添加null检查
                if (currentBookItem != null)
                {
                    currentBookItem.OnPutBackDiary(); isLookDiary = false;
                    currentBookItem = null; // 操作完成后才设为 null
                }
            }
            else { DropItem(); } 
        }
    }

    #endregion

    #region 体力值和理智值相关

    /// <summary>
    /// 体力恢复逻辑（全局Update调用）
    /// </summary>
    public void UpdateBodyPowerRecovery()
    {
        if (isPowerRecovering)
        {
            powerRecoverTimer += Time.deltaTime;
            // 5 秒内恢复到 30 点体力
            if (powerRecoverTimer >= 0.1f) { CurrentBodyPower += 0.6f; powerRecoverTimer = 0; }

            if (CurrentBodyPower >= 30)
            {
                isPowerRecovering = false; powerRecoverTimer = 0;
            }
        }
        else  // 非恢复阶段但体力不足：缓慢恢复到 100
        {
            if (CurrentBodyPower < 100)
            {
                powerRecoverTimer += Time.deltaTime;
                if (powerRecoverTimer >= 0.1f) { CurrentBodyPower += 0.2f; powerRecoverTimer = 0; }
                CurrentBodyPower = Mathf.Min(CurrentBodyPower, 100);
            }
        }
    }
    
    private Vector3 originalCameraLocalPos; // 记录相机初始本地位置
    private Quaternion originalCameraLocalRot; // 记录相机初始本地旋转

    /// <summary>
    /// 获取角色当前位置的综合光照强度
    /// </summary>
    /// <returns>0（完全黑暗）~ 1（强光）</returns>
    private float GetCurrentLightIntensity()
    {
        float totalIntensity = 0f;
        // 查找场景中所有激活的灯光
        foreach (Light l in allLights)
        {
            // 拿到激活的灯光，排除手电筒
            if (l != null && l.isActiveAndEnabled && l.intensity > 0 && l.name != "WhiteLight")
            {
                float distance = Vector3.Distance(transform.position, l.transform.position);
            
                // 检查是否在灯光范围内
                if (distance <= l.range)
                {
                    // 对于聚光灯，检查是否在锥形范围内
                    if (l.type == LightType.Spot)
                    {
                        Vector3 directionToPlayer = (transform.position - l.transform.position).normalized;
                        float angle = Vector3.Angle(l.transform.forward, directionToPlayer);
                        if (angle > l.spotAngle * 0.5f) continue; // 不在锥形范围内
                    }
                    // 计算光照衰减
                    float lightAttenuation = 1 - Mathf.Clamp01(distance / l.range);
                    float normalizedIntensity = Mathf.Clamp01(l.intensity / 1);
                    totalIntensity += normalizedIntensity * lightAttenuation;
                }
            }
        }
        float finalIntensity = Mathf.Clamp(totalIntensity, 0f, 1f);
        return finalIntensity;
    }

    /// <summary>
    /// 更新理智值（黑暗降、光照升）
    /// </summary>
    private void UpdateSanity()
    {
        float currentLightIntensity = GetCurrentLightIntensity();
        // 分支1：黑暗/弱光 → 减理智
        if (currentLightIntensity <= 0f)
        {
            deSanityTimer += Time.deltaTime;
            // 重置恢复计时器（避免切换状态后立刻恢复）
            inSanityTimer = 0;
            if (deSanityTimer >= 0.1f)
            {
                CurrentSanity -= 0.2f; deSanityTimer = 0;
                CurrentSanity = Mathf.Max(CurrentSanity, 0);
            }
        }
        // 分支2：光照充足 → 增理智（但理智值未满时）
        else if (CurrentSanity < 100)
        {
            inSanityTimer += Time.deltaTime;
            deSanityTimer = 0; // 重置消耗计时器
            if (inSanityTimer >= 0.1f)
            { 
                CurrentSanity += 0.2f; inSanityTimer = 0;
                CurrentSanity = Mathf.Min(CurrentSanity, 100);
            }
        }
        // 分支3：光照充足且理智值已满 → 只重置计时器，不增加
        else
        {
            // 理智值已经是100，不需要再增加
            // 但需要重置两个计时器，确保离开光照时能立即开始减少
            deSanityTimer = 0;
            inSanityTimer = 0;
        }
    }

    #endregion
    
    #region 脚步声音动画相关

    /// <summary>
    /// 播放脚步声方法
    /// </summary>
    /// <param name="stateType"></param>
    public void OnFootSetp()
    {
        // 检查冷却时间，避免短时间内重复播放
        if (Time.time - lastFootStepTime < footStepCoolDown) { return; }

        if (currentState == Player_StateType.CrouchWalk)
        {
            SoundAudioPool.Instance.PlaySound(FloorManager.Instance.currentAudioClip,transform.position,0.5f);
        }
        else { SoundAudioPool.Instance.PlaySound(FloorManager.Instance.currentAudioClip,transform.position); }
        // 更新最后一次播放时间
        lastFootStepTime = Time.time;
    }
    
    /// <summary>
    /// 切换状态
    /// </summary>
    /// <param name="stateType"></param>
    public void ChangeState(Player_StateType stateType)
    {
        if (!IsSelfValid()) return; // 自身已销毁则不切换状态
        if (state_machine == null)
        {
            Debug.LogError("状态机未初始化，无法切换状态", this);
            return;
        }
        // 记录上一个状态（排除重复切换相同状态的情况）
        if (currentState != stateType) { lastState = currentState; }
        currentState = stateType;
        switch (stateType)
        {
            case Player_StateType.Idle:
                //调用状态机身上的切换状态方法
                state_machine.ChangeState<Player_IdleState>();
                break;
            case Player_StateType.Move:
                state_machine.ChangeState<Player_MoveState>();
                break;
            case Player_StateType.Jump:
                state_machine.ChangeState<Player_JumpState>();
                break;
            case Player_StateType.Dead:
                state_machine.ChangeState<Player_DeadState>();
                break;
            case Player_StateType.Hurt:
                state_machine.ChangeState<Player_HurtState>();
                break;
            case Player_StateType.Crouch:
                state_machine.ChangeState<Player_CrouchState>();
                break;
            case Player_StateType.CrouchWalk:
                state_machine.ChangeState<Player_CrouchWalkState>();
                break;
            case Player_StateType.CrouchToStand:
                state_machine.ChangeState<Player_CrouchToStandState>();
                break;
        }
    }
    //播放动画
    public void PlayerAnimation(string animationName,float fixedTransitionTime = 0.25f)
    {
        // 添加 null 检查
        if (playerModel == null || playerModel.gameObject == null)
        {
            // 重新获取 PlayerModel
            PlayerModel = FindObjectOfType<PlayerModel>();
        
            if (playerModel == null)
            {
                Debug.LogError("找不到 PlayerModel，无法播放动画");
                return;
            }
        }
        // 检查 Animator
        if (playerModel.Animator == null)
        {
            Debug.LogError("Animator 为 null，无法播放动画");
            return;
        }
        // 播放动画
        playerModel.Animator.Play(animationName, 0, 0);
    }

    #endregion
    
    #region 电梯和相机相关

    private void SwitchCamera()
    {
        if (cameraOne.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.V))
        {
            // 切换到第三人称
            cameraOne.gameObject.SetActive(false);
            cameraThird.gameObject.SetActive(true);
            currentCamera = cameraThird;
        }
        else if (cameraThird.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.V))
        {
            // 切换到第一人称
            cameraThird.gameObject.SetActive(false);
            cameraOne.gameObject.SetActive(true);
            currentCamera = cameraOne;
        }

        // 切换相机后，更新初始位置（防止晃动时用错相机的位置）
        if (currentCamera != null)
        {
            originalCameraLocalPos = currentCamera.transform.localPosition;
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DoorArea"))
        {
            isInElevatorArea = true;
            // 获取当前触发区域对应的脚本
            DoorAreaTrigger doorTrigger = other.GetComponent<DoorAreaTrigger>();
            // 获取区域对应的电梯
            currentElevator = doorTrigger.linkedElevator;
            if (doorTrigger != null && doorTrigger.linkedElevator != null) 
            { currentElevator = doorTrigger.linkedElevator; }
            else { currentElevator = null; }
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("DoorArea"))
        {
            isInElevatorArea = false;
            currentElevator = null;
            currentFloor = 0;
        }
    }

    #endregion
    
    #region 道具和门相关

        /// <summary>
/// 准心射线方法
/// </summary>
    private void CrosshairRaycaster()
    {
        // 判断屏幕中心点是否在UI上，我把 PlayerProPanel 的射线检测关了，所以检测不到
        if (IsPointerOverUIAtScreenCenter()) { return; }
        // 获取屏幕中心位置
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);
        // 重置之前的outline
        if (outline != null) { outline.OutlineWidth = 0; outline = null; }
        // 从屏幕中心发射射线
        Ray ray = currentCamera.ScreenPointToRay(screenCenter);
        // 画射线
        Debug.DrawRay(ray.origin, ray.direction * 1.5f, Color.yellow);
        // 创建层掩码，排除触发器所在的层 排除"Trigger"层
        //int layerMask = ~(1 << LayerMask.NameToLayer("Trigger")); 
        // 获取UI面板引用
        PlayerProPanel playerPanel = UIManager.Instance.getPanel<PlayerProPanel>();
        bool showPickup = false; bool showCrosshair = true; isToDoor = false;
        if (Physics.Raycast(ray, out RaycastHit hit, 1.5f,Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject.layer == 9)// 判断是否为电闸
            {
                outline = hit.collider.gameObject.GetComponent<Outline>();// 设置 outline 效果
                outline.OutlineWidth = 10; isToDoor = false; outline.OutlineColor = Color.white;
            }
            else if (hit.collider.CompareTag("Item"))
            {
                outline = hit.collider.gameObject.GetComponent<Outline>();// 设置outline效果
                outline.OutlineWidth = 10;  outline.OutlineColor = Color.magenta;
                showPickup = true; showCrosshair = false; isToDoor = false;
                // 拾取道具逻辑
                if (KeyBindingManager.Instance.GetActionDown(GameAction.PickUp))
                {
                    I_Item currentItem = hit.collider.gameObject.GetComponent<I_Item>();
                    //判断是不是日记
                    if (hit.collider.gameObject.name != "Diary") //不是日记，执行普通道具拾取方法
                    {
                        if (items.Count < 3) { PickUpItem(currentItem);}
                        else // 弹出提示
                        {
                            TipPanel tipPanel = UIManager.Instance.openPanel<TipPanel>();
                            tipPanel.ShowTip("你已经拾取最大数量道具");
                            tipPanel.transform.localPosition = new Vector2(0, 430);
                        }
                    }
                    else //是日记，执行日记的观看方法
                    {
                        currentBookItem = currentItem; //保存日记的信息
                        currentBookItem.OnLookDiary(currentCamera.transform);
                        ChangeState(Player_StateType.Idle);
                    }
                }
            }
            else if (hit.collider.CompareTag("Switch"))
            {
                showPickup = true; showCrosshair = false; isToDoor = false;
            }
            else if (hit.collider.CompareTag("Door"))
            {
                outline = hit.collider.gameObject.GetComponent<Outline>();// 设置 outline 效果
                outline.OutlineWidth = 10; isToDoor = true; outline.OutlineColor = Color.white;
                // 拿到门
                DoorRotation door = hit.collider.GetComponent<DoorRotation>();
                DragOpenDoor dragDoor = hit.collider.GetComponent<DragOpenDoor>();
                
                currentDoor = null;     // 重置当前门交互对象
                if (door != null) { currentDoor = door; }
                else { currentDoor = null; }
                
                if (dragDoor == null || dragDoor.isDragging) return; // 如果有正在交互的小门则返回
                if (dragDoor != null) { currentDragDoor = dragDoor; }
                else { currentDragDoor = null; }
            }
        }
        // 统一设置UI显示状态
        playerPanel.pickUpImage.gameObject.SetActive(showPickup);
        playerPanel.crosshairImage.gameObject.SetActive(showCrosshair);
    }
      
        /// <summary>
        /// 判断屏幕中心点是否在UI上的方法
        /// </summary>
        /// <returns></returns>
    private bool IsPointerOverUIAtScreenCenter()
    {
        // 创建指向屏幕中心的事件数据
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        // 如果有UI元素在屏幕中心，返回true
        return results.Count > 0;
    }

/// <summary>
/// 拾取道具方法
/// </summary>
/// <param name="item">当前的道具</param>
    private void PickUpItem(I_Item item)
    {
        if (items.Contains(item)) return; // 避免重复拾取
        item.OnPickUp(transform);
        items.Add(item);
        if (items.Count == 1)//如果是第一个道具
        {
            item.OnEquip(currentCamera.transform);
            currentItemIndex = 0; //设置为当前道具索引
        }
    }

/// <summary>
/// 处理道具切换
/// </summary>
    private void SwitchItem()
    {
        if(items.Count == 0 || isStartOpen) return;
        float scroll = Input.GetAxis("Mouse ScrollWheel");// 获取鼠标滚轮输入
        if (scroll == 0) return; // 没有滚轮输入时直接返回
        // 确定方向：1向上，-1向下
        int direction = scroll > 0 ? 1 : -1; 
        // 卸下当前道具
        if (currentItemIndex >= 0 && currentItemIndex < items.Count)
        {
            items[currentItemIndex].OnUnequip();
        }
        // 计算新索引（循环）
        currentItemIndex = (currentItemIndex + direction + items.Count) % items.Count;
        // 装备新道具
        items[currentItemIndex].OnEquip(currentCamera.transform);
    }

/// <summary>
/// 丢弃当前的道具
/// </summary>
    public void DropItem()
    {
        if (items.Count == 0 || isStartOpen) return;
        I_Item itemToDrop = items[currentItemIndex];
        // 从列表中移除
        items.RemoveAt(currentItemIndex);
        itemToDrop.OnDrop();
        // 切换到下一个道具或重置
        if (items.Count > 0)
        {
            //设置 currentItemIndex
            currentItemIndex = Mathf.Clamp(currentItemIndex, 0, items.Count - 1);
            items[currentItemIndex].OnEquip(currentCamera.transform);
        }
        else { currentItemIndex = -1; }
    }

    #endregion
   
    #region 鬼魂跳脸
    
    // 保存原始移动速度
    private float originalRunSpeed;
    private float originalWalkSpeed;

// 新增：设置玩家被困状态
    public void SetStunned(bool stunned, float duration = 0)
    {
        isStunned = stunned;
    
        if (stunned)
        {
            // 保存原始速度
            originalRunSpeed = RunSpeed;
            originalWalkSpeed = WalkSpeed;
        
            // 设为0使玩家无法移动
            RunSpeed = 0;
            WalkSpeed = 0;
        
            // 如果指定了持续时间，启动计时器
            if (duration > 0)
            {
                stunnedTimer = duration;
            }
        }
        else
        {
            // 恢复原始速度
            RunSpeed = originalRunSpeed;
            WalkSpeed = originalWalkSpeed;
            stunnedTimer = 0;
        }
    }

    public void SaveInitialLookRotation()
    {
        initialLookRotation = transform.rotation;
    }

    public bool HasTurnedEnough(float minAngle)
    {
        float angle = Quaternion.Angle(initialLookRotation, transform.rotation);
        return angle >= minAngle;
    }
    // 锁定相机
    public void SetCameraLocked(bool locked)
    {
    }
    //玩家死亡
    public void OnPlayerKilled()
    {
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.playerDieClip01,transform.position);
        ChangeState(Player_StateType.Dead);
        UIManager.Instance.openPanel<FailGamePanel>();
        Debug.Log("玩家已被鬼魂杀死！游戏结束");
    }

    #endregion
    
}
