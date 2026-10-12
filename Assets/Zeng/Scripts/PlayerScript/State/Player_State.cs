using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 玩家状态基类 继承状态基类
/// </summary>
public class Player_State : State_Base
{
    protected PlayerController player;

    public override void Init(IState_MachineOwner owner)
    {
        player = Object.FindObjectOfType<PlayerController>();
        base.Init(owner);
        player = (PlayerController)owner;
    }

    protected virtual bool CheckAnimatorStateName(string stateName, out float normalizedTime,int layerIndex = 0)
    {
        player = Object.FindObjectOfType<PlayerController>();
        AnimatorStateInfo nextInfo = player.playerModel.Animator.GetNextAnimatorStateInfo(layerIndex);
        if (nextInfo.IsName(stateName))//判断当前动画是不是下一个动画
        {
            normalizedTime = nextInfo.normalizedTime;
            return true;
        }
        AnimatorStateInfo info = player.playerModel.Animator.GetCurrentAnimatorStateInfo(layerIndex);
        normalizedTime = info.normalizedTime;
        return info.IsName(stateName);
    }
}
