// GhostAppearState.cs (修改后)
using System.Collections;
using UnityEngine;

public class GhostAppearState : GhostState
{
    public override void Enter()
    {
        base.Enter();
        ghost.PlayerAnimation("Appear");
        ghost.StartCoroutine(DelayedAction());
    }
    
    private IEnumerator DelayedAction()
    {
        yield return new WaitForSeconds(1.5f);
        Exit();
        
        // 播放Idle动画5秒
        ghost.ChangeState(Ghost_StateType.Idle);
        yield return new WaitForSeconds(5f);
        
        // 根据条件决定下一个状态
        if (ShouldStartHunt())
        {
            ghost.ChangeState(Ghost_StateType.Patrol);
        }
        else
        {
            ghost.ChangeState(Ghost_StateType.Normal);
        }
    }
    
    private bool ShouldStartHunt()
    {
        // 查找玩家控制器
        PlayerController playerController = GameObject.FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            // 理智低于50时进入巡逻状态
            return playerController.CurrentSanity < 50f;
        }
        // 默认返回false，进入普通状态
        return false;
    }
}