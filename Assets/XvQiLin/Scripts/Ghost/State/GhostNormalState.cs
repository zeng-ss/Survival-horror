using UnityEngine;

public class GhostNormalState : GhostState
{
    private float interactionTimer;
    private const float INTERACTION_INTERVAL = 8f; // 互动间隔 
    private bool originalAppearanceState; // 保存原始外观状态

    public override void Enter()
    {
        base.Enter();
        // 保存原始外观状态以便退出时恢复
        originalAppearanceState = ghost.isAppear;
        // 确保在普通状态下不显示模型
        ghost.SetAppearance(false);
        
        ghost.PlayerAnimation("Move"); // 普通状态下也会移动
        interactionTimer = 0f;
        ghost.isChasingPlayer = false;
        
        Debug.Log("进入普通状态：隐藏模型，开始移动和互动");
    }

    public override void Update()
    {
        ghost.ApplyGravity();
        ghost.UpdateAnimation();
        
        interactionTimer += Time.deltaTime;

        // 定期进行互动（不影响玩家）
        if (interactionTimer >= INTERACTION_INTERVAL)
        {
            interactionTimer = 0f;
            PerformRandomInteraction();
        }

        // 即使在普通状态也要检测玩家，但不进入猎杀状态
        if (ghost.DetectPlayer())
        {
            Debug.Log("普通状态下检测到玩家，但不进行追击");
            // 改变移动方向
            ghost.RandomRotateDirection();
        }
        else
        {
            // 没有检测到玩家时继续随机移动
            ghost.SimpleMove(ghost.currentPatrolDirection, ghost.patrolSpeed * 0.7f);
        }
    }

    public override void Exit()
    {
        // 恢复原始外观状态
        ghost.SetAppearance(originalAppearanceState);
        Debug.Log("退出普通状态，恢复模型显示状态");
    }

    private void PerformRandomInteraction()
    {
        PatrolActionType action = GetRandomInteraction();
        
        switch (action)
        {
            case PatrolActionType.ThrowObject:
                ThrowObject();
                break;
            case PatrolActionType.OpenDoor:
                OpenDoor();
                break;
            case PatrolActionType.CloseDoor:
                CloseDoor();
                break;
            case PatrolActionType.Appear:
                AppearNearPlayer();
                break;
            case PatrolActionType.None:
                // 什么都不做
                break;
        }
        
        Debug.Log($"普通状态下执行互动: {action}");
    }

    private PatrolActionType GetRandomInteraction()
    {
        // 获取玩家控制器实例
        PlayerController playerController = GameObject.FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            // 如果找不到玩家，使用默认概率
            return GetDefaultInteraction();
        }

        // 计算Appear概率：理智值越低概率越高，0时达到50%
        // 公式：appearProbability = (100 - 理智值) * 0.005f
        // 当理智=100时：0%；理智=50时：25%；理智=0时：50%
        float appearProbability = (100 - playerController.CurrentSanity) * 0.005f;
        appearProbability = Mathf.Clamp01(appearProbability); // 确保在0-1范围内

        float randomValue = Random.Range(0f, 1f);

        // 先判断是否触发Appear互动
        if (randomValue < appearProbability)
        {
            return PatrolActionType.Appear;
        }

        // 剩余概率分配给其他互动
        float remaining = randomValue - appearProbability;
        float totalOtherProbability = 1f - appearProbability;

        // 按比例分配剩余概率
        if (remaining < totalOtherProbability * 0.3f) // 30%概率无互动
        {
            return PatrolActionType.None;
        }
        else if (remaining < totalOtherProbability * 0.6f) // 30%概率扔东西
        {
            return PatrolActionType.ThrowObject;
        }
        else if (remaining < totalOtherProbability * 0.8f) // 20%概率开门
        {
            return PatrolActionType.OpenDoor;
        }
        else // 20%概率关门
        {
            return PatrolActionType.CloseDoor;
        }
    }
    
    private PatrolActionType GetDefaultInteraction()
    {
        float randomValue = Random.Range(0f, 1f);
        if (randomValue < 0.1f) return PatrolActionType.Appear; // 默认10%概率出现
        if (randomValue < 0.4f) return PatrolActionType.None;   // 30%概率无互动
        if (randomValue < 0.7f) return PatrolActionType.ThrowObject; // 30%概率扔东西
        if (randomValue < 0.9f) return PatrolActionType.OpenDoor;    // 20%概率开门
        return PatrolActionType.CloseDoor; // 10%概率关门
    }
    // 新增：在玩家附近出现的互动
    private void AppearNearPlayer()
    {
        if (ghost.playerTransform == null) return;
        
        Debug.Log("鬼魂执行Appear互动，将在玩家附近出现");
        
        // 计算玩家附近的随机位置（3-5米范围内）
        Vector3 randomOffset = new Vector3(
            Random.Range(-1f, 1f), 
            0, 
            Random.Range(-1f, 1f)
        ).normalized * Random.Range(3f, 5f);
        
        Vector3 appearPosition = ghost.playerTransform.position + randomOffset;
        
        // 将鬼魂传送到出现位置
        ghost.transform.position = appearPosition;
        
        // 让鬼魂面向玩家
        Vector3 lookDirection = (ghost.playerTransform.position - appearPosition).normalized;
        ghost.transform.rotation = Quaternion.LookRotation(lookDirection);
        
        // 进入出现状态
        ghost.ChangeState(Ghost_StateType.Appear);
    }

    private void ThrowObject()
    {
        // 仅用Debug表示，不实际影响玩家
        Debug.Log("鬼魂在普通状态下丢了一个东西（不影响玩家）");
    }

    private void OpenDoor()
    {
        // 仅用Debug表示，不实际影响玩家
        Debug.Log("鬼魂在普通状态下尝试开门（不影响玩家）");
    }

    private void CloseDoor()
    {
        // 仅用Debug表示，不实际影响玩家
        Debug.Log("鬼魂在普通状态下尝试关门（不影响玩家）");
    }
}