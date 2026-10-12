using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 玩家的第一个状态 Idle状态 继承玩家状态基类
/// </summary>
public class Player_IdleState : Player_State
{
    public override void Enter()
    {
        base.Enter();
        //播放Idle动画
        player.PlayerAnimation("Idle");
    }

    public override void Update()
    {
        if (player.isLookDiary || KeyBindingManager.Instance.isEscKey) { return; } //如果在看日记就不能走路
        //检测跳跃
        if (KeyBindingManager.Instance.GetActionDown(GameAction.Jump))  {  player.ChangeState(Player_StateType.Jump); return; }
        //检测蹲下
        if (KeyBindingManager.Instance.GetActionDown(GameAction.Crouch))  {  player.ChangeState(Player_StateType.Crouch);  return; }
        
        //检测玩家移动
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (h != 0 || v != 0) { player.ChangeState(Player_StateType.Move); } }
}
