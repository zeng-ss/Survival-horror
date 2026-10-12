using UnityEngine;

public class GhostDisappearState : GhostState
{
    public override void Enter()
    {
        base.Enter();
        // 隐藏鬼魂模型
        SetAppearance(false);
        ghost.WalkToRunValue = 0;
        ghost.currentPath = null;
        
        Debug.Log("进入消失状态：模型隐藏，停止所有动作");
    }

    public override void Update()
    {
    }

    public override void Exit()
    {
        Debug.Log("退出消失状态");
    }
}