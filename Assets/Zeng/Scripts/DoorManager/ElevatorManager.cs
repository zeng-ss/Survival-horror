using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ElevatorManager : UnitySigleTon<ElevatorManager>
{
    #region 数据
    [Header("电梯位置设置")]
    public Vector3 lowElevatorPos = new Vector3(89.76031f, -12.33f, 11.8f);
    public Vector3 midElevatorPos = new Vector3(89.76031f, 6.69f, 11.76f);
    public Vector3 highElevatorPos = new Vector3(89.76031f, 25.3f, 11.35f);
    
    [Header("各楼层电梯门引用")]
    public ElevatorRotate floorMinus1Doors; // 负一楼电梯门
    public ElevatorRotate floor1Doors;      // 一楼电梯门  
    public ElevatorRotate floor2Doors;      // 二楼电梯门
    
    [Header("状态")]
    public int currentElevatorFloor = 2;
    public bool isMoving = false;
    public bool isWaitingForPlayer = false;
    public bool isAtDestination = false; // 是否已到达目标楼层
    
    private int targetFloor;
    private Coroutine waitCoroutine;
    private Coroutine destinationWaitCoroutine;
    private PlayerController player;
    
    #endregion

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    void Update()
    {
        IsPlayerInElevator = CheckPlayerInElevator();
    }
    

    /// <summary>
    /// 根据楼层获取对应的电梯门
    /// </summary>
    private ElevatorRotate GetDoorsByFloor(int floor)
    {
        switch (floor)
        {
            case -1: return floorMinus1Doors;
            case 1: return floor1Doors;
            case 2: return floor2Doors;
            default: 
                Debug.LogError($"未知楼层: {floor}");
                return null;
        }
    }

    #region 公共方法
    public void StartElevatorSequence(int selectedFloor, int playerCurrentFloor)
    {
        if (isMoving) return;
        
        targetFloor = selectedFloor;
        Debug.Log($"开始电梯序列: 玩家在{playerCurrentFloor}楼，选择去{targetFloor}楼");
        
        // 如果已经在目标楼层等待状态，直接重新开始等待
        if (isAtDestination && currentElevatorFloor == targetFloor)
        {
            Debug.Log("已在目标楼层，重新开始等待");
            StartDestinationWaiting();
            return;
        }
        
        isAtDestination = false;
        
        if (currentElevatorFloor != playerCurrentFloor)
        {
            MoveToFloor(playerCurrentFloor, () => 
            {
                OpenDoorsAndWaitForPlayer(playerCurrentFloor);
            });
        }
        else 
        { 
            OpenDoorsAndWaitForPlayer(playerCurrentFloor); 
        }
    }

    public bool IsPlayerInElevator { get; private set; }
    #endregion

    #region 私有方法
    
    /// <summary>
    /// 开门并等待玩家进入
    /// </summary>
    private void OpenDoorsAndWaitForPlayer(int floor)
    {
        Debug.Log($"在{floor}楼开门等待玩家");
        
        // 获取当前楼层的电梯门
        ElevatorRotate currentDoors = GetDoorsByFloor(floor);
        if (currentDoors != null)
        {
            EventCenter.Instance.EventTrigger(GameEvent.开电梯事件, currentDoors);
        }
        else
        {
            Debug.LogError($"{floor}楼的电梯门为 null，无法开门");
            return;
        }
        
        isWaitingForPlayer = true;
        waitCoroutine = StartCoroutine(WaitForPlayerEntry(floor));
    }

    private IEnumerator WaitForPlayerEntry(int floor)
    {
        Debug.Log("开始3秒等待玩家进入");
        float time = 0;
        
        while (time <= 3f)
        {
            time += Time.deltaTime;
            
            if (IsPlayerInElevator)
            {
                Debug.Log("玩家进入电梯，立即关门并移动");
                CloseDoorsAndMove(floor);
                yield break;
            }
            yield return null;
        }
        
        Debug.Log("3秒内玩家未进入，自动关门");
        CloseDoorsAndMove(floor, false); // false表示不移动
    }

    /// <summary>
    /// 关门并移动
    /// </summary>
    private void CloseDoorsAndMove(int floor, bool shouldMove = true)
    {
        isWaitingForPlayer = false;
        
        // 停止等待协程
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }
        
        // 获取当前楼层的电梯门
        ElevatorRotate currentDoors = GetDoorsByFloor(floor);
        if (currentDoors != null)
        {
            EventCenter.Instance.EventTrigger(GameEvent.关电梯事件, currentDoors);
        }
        
        if (shouldMove)
        {
            StartCoroutine(MoveAfterDoorClose());
        }
    }

    private IEnumerator MoveAfterDoorClose()
    {
        yield return new WaitForSeconds(1f);
        
        MoveToFloor(targetFloor, () => 
        {
            // 到达目标楼层后开门并开始等待
            ElevatorRotate targetDoors = GetDoorsByFloor(targetFloor);
            if (targetDoors != null)
            {
                EventCenter.Instance.EventTrigger(GameEvent.开电梯事件, targetDoors);
                Debug.Log($"到达目标楼层{targetFloor}，开门");
                
                // 开始目标楼层的等待逻辑
                StartDestinationWaiting();
            }
        });
    }

    /// <summary>
    /// 开始目标楼层的等待逻辑
    /// </summary>
    private void StartDestinationWaiting()
    {
        isAtDestination = true;
        
        // 停止之前的等待协程
        if (destinationWaitCoroutine != null)
        {
            StopCoroutine(destinationWaitCoroutine);
        }
        
        destinationWaitCoroutine = StartCoroutine(WaitAtDestination());
    }

    /// <summary>
    /// 在目标楼层等待玩家离开
    /// </summary>
    private IEnumerator WaitAtDestination()
    {
        Debug.Log($"在{targetFloor}楼等待玩家离开");
        
        // 等待玩家离开电梯
        yield return new WaitUntil(() => !IsPlayerInElevator);
        
        Debug.Log("玩家已离开电梯，开始等待关门");
        
        // 玩家离开后等待5秒，如果玩家没有回来就自动关门
        float waitTime = 0f;
        while (waitTime < 5f)
        {
            waitTime += Time.deltaTime;
            
            // 如果玩家重新进入电梯，重新开始等待
            if (IsPlayerInElevator)
            {
                Debug.Log("玩家重新进入电梯，继续等待");
                destinationWaitCoroutine = StartCoroutine(WaitAtDestination());
                yield break;
            }
            
            yield return null;
        }
        
        // 5秒后玩家没有回来，自动关门
        Debug.Log("玩家未返回，自动关门");
        CloseDoorsAtDestination();
    }

    /// <summary>
    /// 在目标楼层关门
    /// </summary>
    private void CloseDoorsAtDestination()
    {
        isAtDestination = false;
        
        ElevatorRotate targetDoors = GetDoorsByFloor(targetFloor);
        if (targetDoors != null)
        {
            EventCenter.Instance.EventTrigger(GameEvent.关电梯事件, targetDoors);
            Debug.Log("电梯门关闭，等待下次呼叫");
        }
        
        // 停止等待协程
        if (destinationWaitCoroutine != null)
        {
            StopCoroutine(destinationWaitCoroutine);
            destinationWaitCoroutine = null;
        }
    }

    /// <summary>
    /// 移动到指定楼层
    /// </summary>
    private void MoveToFloor(int floor, System.Action onComplete = null)
    {
        isMoving = true;
        Vector3 targetPos = GetFloorPosition(floor);
        float moveTime = CalculateMoveTime(currentElevatorFloor, floor);
        
        Debug.Log($"电梯移动: {currentElevatorFloor}楼 -> {floor}楼, 时间: {moveTime}秒");
        
        transform.DOLocalMove(targetPos, moveTime).OnComplete(() =>
        {
            currentElevatorFloor = floor;
            isMoving = false;
            onComplete?.Invoke();
        });
    }

    /// <summary>
    /// 检测玩家是否在电梯内
    /// </summary>
    private bool CheckPlayerInElevator()
    {
        if (player == null) return false;
        
        Vector3 playerPos = player.transform.position;
        Vector3 elevatorPos = transform.position;
        
        Vector2 playerXZ = new Vector2(playerPos.x, playerPos.z);
        Vector2 elevatorXZ = new Vector2(elevatorPos.x, elevatorPos.z);
        
        float horizontalDistance = Vector2.Distance(playerXZ, elevatorXZ);
        return horizontalDistance < 3.5f;
    }

    private Vector3 GetFloorPosition(int floor)
    {
        switch (floor)
        {
            case -1: return lowElevatorPos;
            case 1: return midElevatorPos;
            case 2: return highElevatorPos;
            default: return midElevatorPos;
        }
    }

    /// <summary>
    /// 计算移动时间
    /// </summary>
    private float CalculateMoveTime(int fromFloor, int toFloor)
    {
        return Mathf.Abs(toFloor - fromFloor) * 5f;
    }

    /// <summary>
    /// 当脚本销毁时停止所有协程
    /// </summary>
    private void OnDestroy()
    {
        if (waitCoroutine != null)
            StopCoroutine(waitCoroutine);
        
        if (destinationWaitCoroutine != null)
            StopCoroutine(destinationWaitCoroutine);
    }
    #endregion
}