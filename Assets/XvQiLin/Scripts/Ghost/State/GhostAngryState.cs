using UnityEngine;

public class GhostAngryState : GhostState
{
    private float lastObstacleCheckTime;
    private const float OBSTACLE_CHECK_INTERVAL = 0.2f;
    private Vector3 currentAngryTarget; // 愤怒状态当前目标
    private const float ARRIVAL_DISTANCE = 1f;

    public override void Enter()
    {
        base.Enter();
        ghost.PlayerAnimation("Move");
        ghost.isChasingPlayer = true;
        lastObstacleCheckTime = -OBSTACLE_CHECK_INTERVAL;

        // 初始化目标：优先使用临时目标，否则用玩家
        UpdateAngryTarget();
        Debug.Log($"进入愤怒状态，当前追踪目标: {currentAngryTarget}");
    }

    public override void Update()
    {
        ghost.ApplyGravity();
        ghost.UpdateAnimation();

        // 实时更新目标（优先临时目标，否则玩家）
        UpdateAngryTarget();

        // 检查是否到达目标
        if (Vector3.Distance(ghost.transform.position, currentAngryTarget) < ARRIVAL_DISTANCE)
        {
            // 到达临时目标后，先恢复默认目标，再切普通状态
            if (ghost.isUsingTempAngryTarget)
            {
                ghost.ResetAngryTargetToPlayer();
            }
            ghost.ChangeState(Ghost_StateType.Normal);
            return;
        }

        // 定期检测障碍物并处理
        if (Time.time - lastObstacleCheckTime >= OBSTACLE_CHECK_INTERVAL)
        {
            lastObstacleCheckTime = Time.time;
            HandleObstacles();
        }

        // 计算朝向目标的方向并移动
        Vector3 directionToTarget = (currentAngryTarget - ghost.transform.position).normalized;
        directionToTarget.y = 0; // 保持同一水平面

        // 1.5倍最高速度移动
        float angrySpeed = ghost.RunSpeed * 1.5f;
        ghost.SimpleMove(directionToTarget, angrySpeed);
    }

    // 新增：更新愤怒状态目标（外部可调用）
    public void UpdateAngryTarget(Vector3? customTarget = null)
    {
        if (customTarget.HasValue)
        {
            // 传入了自定义目标，直接使用
            currentAngryTarget = customTarget.Value;
        }
        else if (ghost.isUsingTempAngryTarget && ghost.playerTransform != null)
        {
            // 使用临时目标（SetChaseTarget设置的）
            currentAngryTarget = ghost.tempAngryTarget;
        }
        else if (ghost.playerTransform != null)
        {
            // 默认追踪玩家
            currentAngryTarget = ghost.playerTransform.position;
        }
        else
        {
            // 兜底：无玩家时保持当前位置（避免空目标）
            currentAngryTarget = ghost.transform.position;
            Debug.LogWarning("愤怒状态无有效目标，默认停留在当前位置");
        }
    }

    // 新增：恢复目标为玩家
    public void ResetTargetToPlayer()
    {
        if (ghost.playerTransform != null)
        {
            currentAngryTarget = ghost.playerTransform.position;
        }
    }

    private void HandleObstacles()
    {
        // 原障碍物处理逻辑保持不变
        if (ghost.CheckCollisionAhead())
        {
            Vector3[] possibleDirections = new Vector3[4]
            {
                ghost.transform.right,
                -ghost.transform.right,
                ghost.transform.forward + ghost.transform.right,
                ghost.transform.forward - ghost.transform.right
            };

            foreach (var dir in possibleDirections)
            {
                if (!Physics.Raycast(ghost.transform.position + Vector3.up * 0.5f, 
                    dir, ghost.collisionCheckDistance, ghost.obstacleLayer))
                {
                    ghost.SimpleMove(dir, ghost.RunSpeed * 1.5f);
                    return;
                }
            }

            float randomAngle = Random.Range(90f, 180f);
            randomAngle *= Random.Range(0, 2) == 0 ? 1 : -1;
            ghost.transform.Rotate(0, randomAngle, 0);
            Debug.Log($"愤怒状态下检测到障碍物，旋转 {randomAngle} 度避开");
        }
    }

    public override void Exit()
    {
        ghost.WalkToRunValue = 0;
        ghost.isChasingPlayer = false;
        // 退出时清空临时目标标记（防止下次进入异常）
        ghost.isUsingTempAngryTarget = false;
        Debug.Log("退出愤怒状态，切换到普通状态");
    }
}