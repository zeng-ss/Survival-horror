using UnityEngine;

public class GhostHuntState : GhostState
{
    private float huntTimer;
    private float lastPathUpdateTime;
    private const float PATH_UPDATE_INTERVAL = 0.3f; // 更频繁的路径更新
    
    public override void Enter()
    {
        base.Enter();
        ghost.PlayerAnimation("Move");
        huntTimer = 0;
        lastPathUpdateTime = -9999;
        ghost.isChasingPlayer = true;
        
        // 立即开始寻路
        if (ghost.playerTransform != null)
        {
            ghost.SeekPath(ghost.playerTransform.position);
        }
        
        Debug.Log("进入猎杀状态！锁定玩家位置，使用最短路径追逐");
    }

    public override void Update()
    {
        ghost.ApplyGravity();
        ghost.UpdateAnimation();
        
        // 检查猎杀时间
        huntTimer += Time.deltaTime;
        if (huntTimer >= ghost.huntDuration)
        {
            // 猎杀时间结束，消失
            ghost.isChasingPlayer = false;
            ghost.SetAppearance(false);
            return;
        }

        // 检查玩家是否存在
        if (ghost.playerTransform == null)
        {
            ghost.ChangeState(Ghost_StateType.Patrol);
            return;
        }

        // 持续锁定玩家位置，频繁更新路径
        UpdatePlayerChase();

        // 检查是否抓到玩家
        float distanceToPlayer = Vector3.Distance(ghost.transform.position, ghost.playerTransform.position);
        if (distanceToPlayer < 1.5f && ghost.canCatchPlayer) 
        {
            ghost.TriggerJumpScare();
        }
    }

    private void UpdatePlayerChase()
    {
        // 频繁更新路径以确保始终以最短路径追逐玩家
        if (Time.time - lastPathUpdateTime >= PATH_UPDATE_INTERVAL)
        {
            lastPathUpdateTime = Time.time;
            ghost.SeekPathImmediate(ghost.playerTransform.position);
        }

        // 如果有有效路径，跟随路径；否则使用备用追逐
        if (ghost.currentPath != null && ghost.currentPath.vectorPath != null && ghost.currentPath.vectorPath.Count > 0)
        {
            ghost.FollowPath(ghost.RunSpeed);
        }
        else
        {
            // 没有有效路径时，直接朝向玩家移动
            Vector3 directionToPlayer = (ghost.playerTransform.position - ghost.transform.position).normalized;
            ghost.SimpleMove(directionToPlayer, ghost.RunSpeed * 0.8f);
        }

        // 可视化调试
        DebugDrawPathToPlayer();
    }

    private void DebugDrawPathToPlayer()
    {
        if (ghost.currentPath != null && ghost.currentPath.vectorPath != null)
        {
            for (int i = 0; i < ghost.currentPath.vectorPath.Count - 1; i++)
            {
                Debug.DrawLine(ghost.currentPath.vectorPath[i], ghost.currentPath.vectorPath[i + 1], Color.red);
            }
        }
        
        // 绘制直接到玩家的视线（调试用）
        if (ghost.playerTransform != null)
        {
            Debug.DrawLine(ghost.transform.position, ghost.playerTransform.position, Color.yellow);
        }
    }

    public override void Exit()
    {
        ghost.WalkToRunValue = 0;
        ghost.isChasingPlayer = false;
        ghost.currentPath = null; // 清除路径
        
        Debug.Log("退出猎杀状态");
    }
}