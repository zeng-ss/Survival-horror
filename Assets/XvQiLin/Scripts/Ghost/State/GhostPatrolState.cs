using UnityEngine;

public class GhostPatrolState : GhostState
{
    private float patrolTimer;
    private float directionChangeTimer;
    private const float DIRECTION_CHANGE_INTERVAL = 5f; // 定期改变方向

    public override void Enter()
    {
        base.Enter();
        ghost.PlayerAnimation("Move");
        patrolTimer = 0f;
        directionChangeTimer = 0f;
        ghost.isChasingPlayer = false;
        
        // 设置初始随机方向
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        ghost.SetPatrolDirection(randomDirection);
        
        Debug.Log("进入巡逻状态");
    }

    public override void Update()
    {
        ghost.ApplyGravity();
        ghost.UpdateAnimation();
        
        patrolTimer += Time.deltaTime;
        directionChangeTimer += Time.deltaTime;

        // 巡逻时间结束，切换到普通状态
        if (patrolTimer >= ghost.patrolDuration)
        {
            ghost.ChangeState(Ghost_StateType.Normal);
            return;
        }

        // 定期随机改变方向（避免一直走直线）
        if (directionChangeTimer >= DIRECTION_CHANGE_INTERVAL)
        {
            directionChangeTimer = 0f;
            ChangeRandomDirection();
        }

        // 碰撞检测
        if (ghost.CheckCollisionAhead())
        {
            ghost.RandomRotateDirection();
        }

        // 简单移动（不使用寻路）
        ghost.SimpleMove(ghost.currentPatrolDirection, ghost.patrolSpeed);

        // 巡逻状态下也要检测玩家
        if (ghost.DetectPlayer())
        {
            ghost.isChasingPlayer = true;
            ghost.ChangeState(Ghost_StateType.Hunt);
            return;
        }
    }

    public override void Exit()
    {
        ghost.WalkToRunValue = 0f;
        Debug.Log("退出巡逻状态");
    }

    private void ChangeRandomDirection()
    {
        // 随机微调方向
        float angle = Random.Range(-30f, 30f);
        Quaternion rotation = Quaternion.Euler(0, angle, 0);
        Vector3 newDirection = rotation * ghost.currentPatrolDirection;
        ghost.SetPatrolDirection(newDirection);
        
        Debug.Log("巡逻方向微调");
    }
}