using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.UI;

// 巡逻点数据结构
[System.Serializable]
public class PatrolPoint
{
    public Vector3 position;
    public List<PatrolActionType> possibleActions = new List<PatrolActionType>();
}

public class GhostController : MonoBehaviour, IState_MachineOwner
{
    #region 数据配置
    [System.NonSerialized] public bool canCatchPlayer = true;
    public Vector3 tempAngryTarget; // 存储临时指定的愤怒状态目标点
    public bool isUsingTempAngryTarget; // 是否使用临时目标的标记
    [Header("鬼魂特性扩展（新增）")]
    [Tooltip("拖拽对应鬼魂类型的特性配置文件")]
    public GhostFeatureConfig ghostFeatureConfig;
    private IGhostFeature currentFeature;
    public Ghost_StateType currentState;
    public Ghost_StateType lastState;
    
    #region A*寻路相关变量
    [HideInInspector] public Seeker seeker;
    public Path currentPath;
    [HideInInspector] public int currentWaypoint = 0;
    public float repathRate = 0.5f;
    [HideInInspector] public float lastRepath = -9999;
    public float nextWaypointDistance = 0.5f;
    #endregion

    #region 状态时间限制变量
    public float patrolDuration = 60f; // 巡逻持续时间
    public float huntDuration = 30f;  // 猎杀持续时间
    #endregion

    #region 巡逻相关变量
    [HideInInspector] public Vector3 patrolCenter;
    [HideInInspector] public float patrolRadius = 8f;
    [HideInInspector] public Vector3 currentPatrolDirection; // 当前巡逻方向
    [HideInInspector] public float patrolSpeed = 2f; // 巡逻速度较慢
    #endregion

    #region 原有核心变量
    public GhostModle ghostModle;
    private State_Machine state_Machine;
    [SerializeField] private CharacterController characterController;
    public CharacterController CharacterController { get => characterController; }
    public AudioSource audioSource;
    public SkinnedMeshRenderer skinnedMesh;
    
    [HideInInspector] public float WalkToRunValue;
    [HideInInspector] public Vector3 targetPosition;
    [HideInInspector] public float changeTargetTimer;
    [HideInInspector] public bool isChasingPlayer;
    [HideInInspector] public Transform playerTransform;
    
    [Header("移动速度设置")]
    [Header("走路的速度")] public float WalkSpeed;
    [Header("跑步的速度")] public float RunSpeed;
    [Header("旋转速度")] public float RotateSpeed;
    [Header("走切换到跑的速度")] public float WalkToRunSpeed;
    
    [Header("音效设置")]
    [Header("脚步声数组")] public AudioClip[] FootAudioClips;
    
    [Header("跳跃设置")]
    [Header("跳跃幅度")] public float jumpPower;
    [Header("跳跃的移动速度")] public float moveSpeedForJump;
    
    [Header("玩家检测设置")]
    [Header("玩家检测范围")] public float detectionRange = 10f;
    [Header("玩家视线检测角度")] public float detectionAngle = 90f;
    [Header("巡逻半径")] public float PatrolRadius = 8f;

    [HideInInspector] public bool hasGravity;
    [HideInInspector] public bool isOnGround;
    [HideInInspector] public bool isInDoorArea;
    [HideInInspector] public DoorRotation currentDoor;
    [SerializeField] private bool isShowChanged;
    [SerializeField] public bool isAppear;
    #endregion

    #region 速度平滑过度
    private float currentMoveSpeed;
    private float speedSmoothTime = 0.2f; // 速度过渡时间
    #endregion

    #region 重力与地面检测优化
    [Header("重力与地面设置")]
    public float gravity = -20f; // 增大重力值，让下落更自然
    public float groundCheckDistance = 0.2f; // 地面检测距离
    public LayerMask groundLayer; // 地面层级（需在Inspector指定）
    private Vector3 velocity; // 保留原有速度变量，专注处理Y轴
    #endregion
    
    #region 碰撞检测相关
    [Header("碰撞检测")]
    public float collisionCheckDistance = 1f;
    public LayerMask obstacleLayer = 1; // 默认层
    private Vector3 lastSafePosition;
    #endregion
    #region 动画状态同步
    [Header("动画参数")]
    public string speedParam = "Speed"; // 动画器中的速度参数名
    public string isGroundedParam = "IsGrounded"; // 地面状态参数
    private const float STUCK_TIME_THRESHOLD = 3f;
    #endregion
    
    #region 跳脸事件配置
    [SerializeField][Header("鬼脸图片")]private Texture2D jumpScareImage;//鬼脸图片
    [SerializeField][Header("鬼头模型")]private GameObject ghostHeadPrefab;//3d鬼头模型
    
    // 跳脸配置参数
    public float reactionTimeLimit = 2f; // 玩家反应时限（2秒内需要转头）
    public float catchCooldown = 1.5f; // 成功躲避后的冷却时间
    // 跳脸状态变量
    [System.NonSerialized] public bool isJumpScareActive = false;
    #endregion

    #endregion

    void Start()
    {
        Init();
        InitGhostFeature();
    }
    [System.NonSerialized] public float jumpScareTimer = 0;
    public float stunnedDuration = 10f; // 被抓后无法操作时长

    private void Init()
    {
        // 初始化A*寻路组件
        seeker = GetComponent<Seeker>();
        if (seeker == null)
            seeker = gameObject.AddComponent<Seeker>();

        isOnGround = true;
        hasGravity = true;
        ghostModle.Init(OnFootSetp);
        state_Machine = new State_Machine();
        state_Machine.Init(this);
        
        WalkToRunValue = 0f;
        changeTargetTimer = 0f;
        isChasingPlayer = false;
        patrolCenter = transform.position;
        patrolRadius = PatrolRadius;
        lastSafePosition = transform.position;
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        ChangeState(Ghost_StateType.Patrol);
        
        UpdateAppearanceState();
    }
    [System.NonSerialized] public bool playerTurnedSuccessfully = false;
    #region 基本方法
    // 更新状态切换方法
    public void ChangeState(Ghost_StateType stateType)
    {
        if (currentState != stateType)
        {
            lastState = currentState;
        }
        currentState = stateType;
        switch (stateType)
        {
            case Ghost_StateType.Idle:
                state_Machine.ChangeState<GhostIdleState>();
                break;
            case Ghost_StateType.Appear:
                state_Machine.ChangeState<GhostAppearState>();
                break;
            case Ghost_StateType.Hunt:
                state_Machine.ChangeState<GhostHuntState>();
                break;
            case Ghost_StateType.Patrol:
                state_Machine.ChangeState<GhostPatrolState>();
                break;
            case Ghost_StateType.Catch:
                state_Machine.ChangeState<GhostCatchState>();
                break;
            case Ghost_StateType.Normal:
                state_Machine.ChangeState<GhostNormalState>();
                break;
            case Ghost_StateType.Disappear:
                state_Machine.ChangeState<GhostDisappearState>();
                break;
            case Ghost_StateType.Angry:
                state_Machine.ChangeState<GhostAngryState>();
                break;
            
        }
    }
    private GameObject currentGhostHead;
    [SerializeField][Header("玩家控制器")]private PlayerController playerController;

    // 寻路方法
    public void SeekPath(Vector3 targetPosition)
    {
        if (float.IsNaN(targetPosition.x) || float.IsNaN(targetPosition.z))
        {
            Debug.LogWarning("寻路目标位置无效！");
            return;
        }

        if (seeker.IsDone() && Time.time - lastRepath > repathRate)
        {
            lastRepath = Time.time;
            seeker.StartPath(transform.position, targetPosition, (p) => {
                if (p.error)
                {
                    Debug.LogError("路径生成失败：" + p.errorLog);
                    currentPath = null;
                    currentWaypoint = 0;
                }
                else
                {
                    currentPath = p;
                    currentWaypoint = 0;
                }
            });
        }
    }

    // 路径跟随方法
    public void FollowPath(float targetSpeed)
    {
        if (currentPath == null || currentPath.vectorPath == null || currentPath.vectorPath.Count == 0)
        {
            WalkToRunValue = 0; // 动画归零，避免抽搐
            currentMoveSpeed = 0;
            return;// 直接退出，不执行移动逻辑
        }

        if (currentWaypoint >= currentPath.vectorPath.Count)
        {
            currentWaypoint = currentPath.vectorPath.Count - 1;
        }

        // 平滑速度过渡（与动画参数同步）
        currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, targetSpeed, Time.deltaTime / speedSmoothTime);

        // 计算XZ方向移动
        Vector3 targetPosition = currentPath.vectorPath[currentWaypoint];
        targetPosition.y = transform.position.y;
        Vector3 direction = (targetPosition - transform.position).normalized;
        Vector3 moveVelocity = direction * currentMoveSpeed;

        // 应用移动
        characterController.Move(moveVelocity * Time.deltaTime);

        // 旋转逻辑
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotateSpeed * Time.deltaTime);
        }

        // 路点更新逻辑
        if (Vector3.Distance(transform.position, targetPosition) < nextWaypointDistance)
        {
            currentWaypoint++;
            if (currentWaypoint >= currentPath.vectorPath.Count)
            {
                currentPath = null;
                currentWaypoint = 0;
            }
        }
    }
    // 碰撞检测方法（巡逻状态使用）
    public bool CheckCollisionAhead()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        return Physics.Raycast(origin, transform.forward, collisionCheckDistance, obstacleLayer);
    }

    public void UpdateAnimation()
    {
        // 关键：根据状态动态设置 WalkToRunValue，精准匹配混合树阈值
        if (isChasingPlayer)
        {
            // 猎杀状态：Speed 0.6-1.0（跑步），平滑过渡
            WalkToRunValue = Mathf.Lerp(WalkToRunValue, 0.8f, Time.deltaTime * WalkToRunSpeed);
            WalkToRunValue = Mathf.Clamp(WalkToRunValue, 0.6f, 1.0f);
        }
        else if (currentPath != null && currentPath.vectorPath.Count > 0)
        {
            // 巡逻移动状态：Speed 0.3-0.4（行走），平滑过渡
            WalkToRunValue = Mathf.Lerp(WalkToRunValue, 0.35f, Time.deltaTime * WalkToRunSpeed * 0.5f);
            WalkToRunValue = Mathf.Clamp(WalkToRunValue, 0.3f, 0.4f);
        }
        else
        {
            // 无路径/静止状态：Speed 0（Idle）
            WalkToRunValue = Mathf.Lerp(WalkToRunValue, 0f, Time.deltaTime * WalkToRunSpeed);
        }

        // 同步到动画器（直接传递 WalkToRunValue，与混合树阈值对应）
        ghostModle.Animator.SetFloat(speedParam, WalkToRunValue);
        //ghostModle.Animator.SetBool(isGroundedParam, isOnGround);
    }

    public void ApplyGravity()
    {
        isOnGround = IsGrounded();

        if (isOnGround)
        {
            // 地面上：重置Y轴速度（避免累积负速度）
            if (velocity.y < 0)
            {
                velocity.y = -2f; // 轻微负值，确保能检测到地面
            }
        }
        else
        {
            // 空中：应用重力加速度（随时间累积）
            velocity.y += gravity * Time.deltaTime;
        }

        // 通过CharacterController应用Y轴移动（与XZ轴移动分离，避免干扰）
        characterController.Move(velocity * Time.deltaTime);
    }

    // 随机旋转方向（巡逻状态使用）
    public void RandomRotateDirection()
    {
        // 随机选择顺时针或逆时针90度旋转
        float rotateAngle = Random.Range(0, 2) == 0 ? 90f : -90f;
        transform.Rotate(0, rotateAngle, 0);
        currentPatrolDirection = transform.forward;
        
        Debug.Log($"巡逻碰撞，旋转 {rotateAngle} 度");
    }

    // 玩家检测方法
    public bool DetectPlayer()
    {
        if (playerTransform == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // 超出检测范围
        if (distanceToPlayer > detectionRange)
            return false;

        // 计算方向
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        
        // 角度检查
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > detectionAngle * 0.5f)
            return false;

        // 视线检查
        if (!Physics.Raycast(transform.position + Vector3.up, directionToPlayer, 
            out RaycastHit hit, distanceToPlayer, LayerMask.GetMask("Default", "StaticEnvironment")))
        {
            return true; // 无遮挡
        }

        // 检查是否击中玩家
        if (hit.collider.CompareTag("Player"))
            return true;

        return false;
    }

    // 设置巡逻方向
    public void SetPatrolDirection(Vector3 direction)
    {
        currentPatrolDirection = direction.normalized;
    }

    // 简单移动方法（巡逻状态使用，不使用寻路）
    public void SimpleMove(Vector3 direction, float speed)
    {
        if (direction != Vector3.zero)
        {
            // 应用移动
            Vector3 moveVelocity = direction * speed;
            characterController.Move(moveVelocity * Time.deltaTime);

            // 旋转面向移动方向
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotateSpeed * Time.deltaTime);
        }
    }

    // 外观状态设置
    public void SetAppearance(bool appear)
    {
        if (isAppear != appear)
        {
            isAppear = appear;
            isShowChanged = true;
        }
    }

    // 更新外观状态
    private void UpdateAppearanceState()
    {
        if (ghostModle != null)
        {
            ghostModle.gameObject.SetActive(isAppear);
        }
        if (isAppear)
        {
            ChangeState(Ghost_StateType.Appear);
        }
    }

    // 播放动画
    public void PlayerAnimation(string animationName, float fixedTransitionTime = 0.25f)
    {
        ghostModle.Animator.Play(animationName, 0, 0);
    }

    // 播放音效
    public void PlayAudio(AudioClip audioClip)
    {
        audioSource.PlayOneShot(audioClip);
    }
    #endregion
    #region 辅助方法
    
    // 地面检测
    private bool IsGrounded()
    {
        // 从角色脚下发射射线检测地面
        Vector3 origin = transform.position + Vector3.up * 0.1f; // 略高于模型原点
        Vector3 direction = Vector3.down;
        return Physics.Raycast(origin, direction, out _, groundCheckDistance, groundLayer);
    }

    // 脚步声回调
    public void OnFootSetp()
    {
        audioSource.PlayOneShot(FootAudioClips[0]);
    }
    // 显示跳脸效果（预留）
    private void ShowJumpScareEffects()
    {
        // 显示鬼脸图片
        // 生成3D鬼头模型
    }

    // 路径完成回调
    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            currentPath = p;
            currentWaypoint = 0;
        }
    }

    #endregion
    #region 跳脸事件相关方法
    
    // 触发跳脸事件
    public void TriggerJumpScare()
    {
        if (isJumpScareActive) return;

        isJumpScareActive = true;
        jumpScareTimer = 0;
        playerTurnedSuccessfully = false;

        // 获取玩家控制器引用
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();

        if (playerController == null)
        {
            Debug.LogError("找不到PlayerController！");
            return;
        }
    
        // 将鬼魂传送到玩家摄像机前方
        TeleportGhostToPlayerFront();
    
        // 使用新的SetStunned方法让玩家被困
        playerController.SetStunned(true, reactionTimeLimit); // 被困时间为反应时间
    
        playerController.SaveInitialLookRotation();
        // 显示跳脸效果
        ShowJumpScareEffects();
        // 切换到抓住状态
        ChangeState(Ghost_StateType.Catch);
        CameraController cameraController = FindObjectOfType<CameraController>();
        if (cameraController != null)
        {
            cameraController.mouseSensitivity = 0;
        }

        Debug.Log("跳脸开始！2秒内迅速转头躲避！");
    }
    // 将鬼魂传送到玩家摄像机前方
    private void TeleportGhostToPlayerFront()
    {
        if (playerController == null || playerController.currentCamera == null) return;
    
        // 获取玩家摄像机的前方位置（距离2单位）
        Vector3 playerCameraPosition = playerController.currentCamera.transform.position;
        Vector3 playerCameraForward = playerController.currentCamera.transform.forward;
    
        // 计算鬼魂位置（在玩家前方2单位，稍微偏下让鬼魂看起来更恐怖）
        Vector3 ghostPosition = playerCameraPosition + playerCameraForward * 2f;
        ghostPosition.y -= 0.5f; // 稍微偏下
    
        // 传送鬼魂
        transform.position = ghostPosition;
    
        // 让鬼魂面向玩家
        Vector3 lookDirection = (playerCameraPosition - transform.position).normalized;
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0, lookDirection.z));
            transform.rotation = targetRotation;
        }
    
        Debug.Log($"鬼魂传送到玩家前方: {ghostPosition}");
    }
    // 更新跳脸状态
    public void UpdateJumpScare()
    {
        if (!isJumpScareActive || playerController == null) return;

        jumpScareTimer += Time.deltaTime;
        
        // 0.5秒后解除摄像机锁定，允许玩家转头
        if (jumpScareTimer >= 0.5f)
        {
            playerController.SetCameraLocked(false);
        }
        // 检查玩家是否成功转头（在解除摄像机锁定后才开始检测）
        if (!playerTurnedSuccessfully && jumpScareTimer >= 0.5f && playerController.HasTurnedEnough(90f))
        {
            playerTurnedSuccessfully = true;
            Debug.Log("成功转头躲避！");
        }


        // 反应时间结束，处理结果
        if (jumpScareTimer >= reactionTimeLimit)
        {
            EndJumpScare();
        }
    }
    // 结束跳脸事件
    private void EndJumpScare()
    {
        if (playerController == null) return;
    
        // 隐藏跳脸效果
        HideJumpScareEffects();

        if (playerTurnedSuccessfully)
        {
            // 成功躲避：玩家恢复移动
            OnJumpScareSuccess();
        }
        else
        {
            // 失败：玩家被杀死
            OnJumpScareFail();
        }

        // 重置跳脸状态
        isJumpScareActive = false;
        jumpScareTimer = 0;
        playerTurnedSuccessfully = false;
        CameraController cameraController = FindObjectOfType<CameraController>();
        if (cameraController != null)
        {
            cameraController.mouseSensitivity = 100;
        }
    }
    // 将鬼魂传送离开玩家
    private void TeleportGhostAwayFromPlayer()
    {
        if (playerController == null) return;
    
        // 传送到玩家侧方5单位位置
        Vector3 awayPosition = playerController.transform.position + 
                               playerController.transform.right * 5f;
        awayPosition.y = transform.position.y; // 保持原有高度
    
        transform.position = awayPosition;
    }
    // 成功躲避后的冷却协程
    private IEnumerator SuccessCooldown()
    {
        // 进入发呆状态
        ChangeState(Ghost_StateType.Idle);
    
        // 等待冷却时间
        yield return new WaitForSeconds(catchCooldown);
    
        // 冷却结束后继续猎杀
        if (isAppear) // 确保鬼魂仍然可见
        {
            ChangeState(Ghost_StateType.Hunt);
        }
    }
    // 跳脸成功处理
    private void OnJumpScareSuccess()
    {
        // 玩家恢复移动（通过SetStunned(false)恢复速度）
        playerController.SetStunned(false);
    
        // 将鬼魂传送离开玩家
        TeleportGhostAwayFromPlayer();
        // 鬼魂进入发呆状态
        StartCoroutine(SuccessCooldown());

        Debug.Log("跳脸躲避成功！鬼魂进入发呆状态");
    }

// 隐藏跳脸效果（预留）
    private void HideJumpScareEffects()
    {
        // 隐藏鬼脸图片
        // 销毁3D鬼头模型
    }
    // 跳脸失败处理
    private void OnJumpScareFail()
    {
        // 玩家被杀死
        playerController.OnPlayerKilled();
    
        // 鬼魂完成任务，可以消失或进入其他状态
        SetAppearance(false);
    
        Debug.Log("玩家被鬼魂杀死！游戏结束");
    }
    #endregion
    #region 猎杀状态专用寻路方法

// 优化的寻路方法（猎杀状态使用）
    public void SeekPathImmediate(Vector3 targetPosition)
    {
        if (float.IsNaN(targetPosition.x) || float.IsNaN(targetPosition.z))
        {
            Debug.LogWarning("寻路目标位置无效！");
            return;
        }

        // 猎杀状态下立即寻路，不等待冷却
        if (seeker.IsDone())
        {
            seeker.StartPath(transform.position, targetPosition, OnHuntPathComplete);
        }
    }

// 猎杀状态路径完成回调
    private void OnHuntPathComplete(Path p)
    {
        if (!p.error)
        {
            currentPath = p;
            currentWaypoint = 0;
        
            // 猎杀状态下如果路径无效，尝试直接朝向玩家
            if (currentPath.vectorPath == null || currentPath.vectorPath.Count == 0)
            {
                FallbackToDirectChase();
            }
        }
        else
        {
            // 寻路失败时使用备用追逐方式
            FallbackToDirectChase();
        }
    }

// 备用追逐方式：直接朝向玩家移动（当寻路失败时）
    private void FallbackToDirectChase()
    {
        if (playerTransform == null) return;
    
        // 计算到玩家的方向
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
    
        // 简单朝向玩家移动（可能会撞到障碍物，但至少不会卡住）
        SimpleMove(directionToPlayer, RunSpeed * 0.7f); // 稍微降低速度
    
        Debug.Log("寻路失败，使用备用追逐模式");
    }

    // 调试绘制
    private void OnDrawGizmosSelected()
    {
        // 绘制检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 绘制检测角度
        Vector3 leftBoundary = Quaternion.Euler(0, -detectionAngle * 0.5f, 0) * transform.forward * detectionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, detectionAngle * 0.5f, 0) * transform.forward * detectionRange;
        
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, leftBoundary);
        Gizmos.DrawRay(transform.position, rightBoundary);
        
        // 绘制巡逻范围
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(patrolCenter, patrolRadius);
    }

    #endregion
    #region 音乐管理相关

    private void UpdateMusicBasedOnState()
    {
        if (MusicManager.Instance == null) return;
    
        GhostMusicState targetMusicState = GetMusicStateForCurrentGhostState();
        MusicManager.Instance.ChangeMusicState(targetMusicState);
    }

    private GhostMusicState GetMusicStateForCurrentGhostState()
    {
        // 根据鬼魂的当前状态返回对应的音乐状态
        if (isJumpScareActive)
            return GhostMusicState.JumpScare;
    
        if (state_Machine != null)
        {
            var currentState = state_Machine.GetCurrentState();
        
            if (currentState is GhostHuntState)
                return GhostMusicState.Hunt;
            else if (currentState is GhostPatrolState)
                return GhostMusicState.Patrol;
            else if (currentState is GhostNormalState || currentState is GhostIdleState || currentState is GhostAppearState)
                return GhostMusicState.Normal;
        }
    
        return GhostMusicState.Normal; // 默认状态
    }

    #endregion
    #region 鬼魂特性
    /// <summary>
    /// 初始化鬼魂特性组件
    /// </summary>
    private void InitGhostFeature()
    {
        if (ghostFeatureConfig == null)
        {
            Debug.LogWarning("未配置鬼魂特性文件，将使用默认行为");
            return;
        }

        // 根据配置的鬼魂类型自动获取对应特性组件
        switch (ghostFeatureConfig.ghostType)
        {
            case GhostType.WaterGhost:
                currentFeature = GetComponent<WaterGhostFeature>();
                break;
            case GhostType.HangedGhost:
                currentFeature = GetComponent<HangedGhostFeature>();
                break;
            default:
                Debug.LogWarning($"暂未实现{ghostFeatureConfig.ghostType}类型的特性组件");
                break;
        }

        // 初始化特性组件
        currentFeature?.OnInit(this);
    }

    /// <summary>
    /// 状态进入扩展方法（供状态类调用）
    /// </summary>
    public void OnStateEnterExtension(Ghost_StateType stateType)
    {
        currentFeature?.OnStateEnter(stateType);
    }

    /// <summary>
    /// 触发独特互动行为（供状态类调用）
    /// </summary>
    public void TriggerUniqueAction()
    {
        currentFeature?.PerformUniqueAction();
    }

    

    #endregion

    #region 其他方法
    /// <summary>
    /// 设置鬼魂的追踪目标坐标并进入愤怒状态
    /// </summary>
    /// <param name="targetPosition">目标坐标</param>
    public void SetChaseTarget(Vector3 targetPosition)
    {
        // 验证目标坐标有效性
        if (float.IsNaN(targetPosition.x) || float.IsNaN(targetPosition.y) || float.IsNaN(targetPosition.z))
        {
            Debug.LogWarning("设置的追踪目标坐标无效！");
            return;
        }

        // 记录临时愤怒状态目标点（新增一个临时变量存储指定目标）
        tempAngryTarget = targetPosition;
        isUsingTempAngryTarget = true; // 标记使用临时目标

        // 强制切换到愤怒状态（替换原猎杀状态）
        ChangeState(Ghost_StateType.Angry);

        // 确保愤怒状态下立即更新目标
        if (state_Machine.GetCurrentState() is GhostAngryState angryState)
        {
            angryState.UpdateAngryTarget(tempAngryTarget); // 主动更新愤怒状态目标
            Debug.Log($"已进入愤怒状态并设置临时追踪目标: {targetPosition}");
        }
        else
        {
            Debug.LogWarning("切换到愤怒状态失败，无法更新追踪目标");
        }
    }
    /// <summary>
    /// 设置鬼魂无法抓到玩家的保护时间
    /// </summary>
    /// <param name="duration">保护时间（秒），默认30秒</param>
    public void SetPlayerInvulnerable(float duration = 30f)
    {
        // 启动协程处理保护时间逻辑
        StartCoroutine(InvulnerableCoroutine(duration));
    }
    /// <summary>
    /// 处理玩家无敌状态的协程
    /// </summary>
    /// <param name="duration">无敌持续时间</param>
    private IEnumerator InvulnerableCoroutine(float duration)
    {
        // 记录原始状态并开启无敌
        bool originalCanCatch = canCatchPlayer;
        canCatchPlayer = false;
    
        Debug.Log($"玩家进入无敌状态，持续 {duration} 秒");

        // 等待指定的无敌时间
        yield return new WaitForSeconds(duration);

        // 恢复原始状态
        canCatchPlayer = originalCanCatch;
        Debug.Log("玩家无敌状态结束，鬼魂可以再次攻击");
    }
    // 新增：恢复愤怒状态默认目标（玩家）的方法（供外部/效果结束时调用）
    public void ResetAngryTargetToPlayer()
    {
        isUsingTempAngryTarget = false;
        tempAngryTarget = Vector3.zero; // 清空临时目标
        // 如果当前处于愤怒状态，立即更新目标为玩家
        if (state_Machine.GetCurrentState() is GhostAngryState angryState)
        {
            angryState.ResetTargetToPlayer();
            Debug.Log("愤怒状态目标已恢复为玩家");
        }
    }
    #endregion
    void Update()
    {
        // 处理外观状态变化
        if (isShowChanged)
        {
            UpdateAppearanceState();
            isShowChanged = false;
            currentFeature?.OnUpdate();
        }

        // 优先处理重力（确保Y轴移动正确）
        ApplyGravity();
        
        // 更新动画（每帧同步速度状态）
        UpdateAnimation();
        // 更新音乐状态
        UpdateMusicBasedOnState();
        // 更新跳脸状态
        if (isJumpScareActive)
        {
            UpdateJumpScare();
        }
    }
}