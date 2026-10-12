using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_JumpState : Player_State
{
    private float previousMoveSpeed;// 记录跳跃前的移动速度
    public override void Enter()
    {
        // 只在Player真的被销毁时终止，正常情况执行跳跃
        if (player == null)
        {
            //Debug.LogError("PlayerController已销毁，无法跳跃");
            return;
        }
        player.PlayerAnimation("Jump");
        player.hasGravity = false;
        // 记录跳跃前的移动速度
        previousMoveSpeed = player.currentMoveSpeed;
        //注册根运动
        player.playerModel.setRootMotionAction(OnRootMotion);
    }
/// <summary>
/// 根运动方法
/// </summary>
/// <param name="arg1"></param>
/// <param name="arg2"></param>
    private void OnRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        deltaPosition.y *= player.jumpPower;
        player.CharacterController.Move(deltaPosition);
    }

    public override void Update()
    {
        if (CheckAnimatorStateName("Jump", out float animationTime))
        {
            if (animationTime > 0.1f && animationTime < 0.6f)
            {
                player.hasGravity = true;
                #region 角色移动
                
                float h = Input.GetAxis("Horizontal");
                float v = Input.GetAxis("Vertical");
                if (h != 0 || v != 0)
                {
                    //处理移动
                    Vector3 input = new Vector3(h, 0, v);
                    //计算出相对摄像机移动的世界空间中的方向向量
                    Vector3 dir = player.CurrentCamera.transform.TransformDirection(input);
                    player.CharacterController.Move(dir * (Time.deltaTime * previousMoveSpeed));
                    float y = player.CurrentCamera.transform.rotation.eulerAngles.y;
                    Vector3 moveDir = Quaternion.Euler(0, y, 0) * input;
                    //处理模型旋转
                    player.playerModel.transform.rotation = Quaternion.Slerp(player.playerModel.transform.rotation, 
                        Quaternion.LookRotation(moveDir), player.RotateSpeed * Time.deltaTime);
                }

                #endregion
            }
            else if(animationTime >= 0.6f)
            {
                // 根据输入决定切换到哪个状态
                float h = Input.GetAxis("Horizontal");
                float v = Input.GetAxis("Vertical");
                player.currentMoveSpeed = previousMoveSpeed;
                if (h != 0 || v != 0)
                {
                    // 有移动输入，直接切换到移动状态
                    player.ChangeState(Player_StateType.Move);
                }
                else
                {
                    // 没有移动输入，切换到闲置状态
                    player.ChangeState(Player_StateType.Idle);
                }
            }
        }
    }

    public override void Exit()
    {
        player.hasGravity = true;//开启重力
        // 确保速度被恢复
        if (player.currentMoveSpeed == 0) { player.currentMoveSpeed = previousMoveSpeed; }
        player.playerModel.clearRootMotionAction();
    }
}
