// GhostIdleState.cs
using UnityEngine;

public class GhostIdleState : GhostState
{
    private float idleTimer;
    private float maxIdleTime;
    public override void Enter()
    {
        base.Enter();
        ghost.PlayerAnimation("Idle");
        idleTimer = 0;
        // 如果是跳脸成功后的发呆，设置较短时间
        maxIdleTime = ghost.isJumpScareActive ? ghost.catchCooldown : 5f;
    }

    public override void Update()
    {
        ApplyGravity();
        UpdateAnimation();
        
        idleTimer += Time.deltaTime;
        
        // 发呆时间结束后返回巡逻（如果是正常发呆）或猎杀（如果是跳脸后的发呆）
        if (idleTimer >= maxIdleTime)
        {
            if (ghost.isChasingPlayer)
            {
                ghost.ChangeState(Ghost_StateType.Hunt);
            }
            else
            {
                ghost.ChangeState(Ghost_StateType.Patrol);
            }
        }
    }
}