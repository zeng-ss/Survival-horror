// GhostCatchState.cs
using UnityEngine;

public class GhostCatchState : GhostState
{
    public override void Enter()
    {
        base.Enter();
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.ghostJumpScary02,ghost.transform.position);
        ghost.PlayerAnimation("Catch");
        // 停止移动
        ghost.WalkToRunValue = 0;
        ghost.currentPath = null;
        
        Debug.Log("进入抓住状态，播放Catch动画");
    }

    public override void Update()
    {
        base.Update();
        
        // 更新跳脸逻辑
        if (ghost.isJumpScareActive)
        {
            ghost.UpdateJumpScare();
        }
        
        // 确保在抓住状态下不移动
        ghost.WalkToRunValue = 0;
    }

    public override void Exit()
    {
        // 清理状态
        Debug.Log("退出抓住状态");
    }
}