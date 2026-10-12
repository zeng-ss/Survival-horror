using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_CrouchWalkState : Player_State
{
    private static readonly int crouchH = Animator.StringToHash("CrouchH");
    private static readonly int crouchV = Animator.StringToHash("CrouchV");
    private Vector3 currentMoveDirection;
    public override void Enter() { player.PlayerAnimation("CrouchWalk"); }
    
    public override void Update()
    {
        if (KeyBindingManager.Instance.isEscKey) player.ChangeState(Player_StateType.Crouch);
        if (KeyBindingManager.Instance.GetActionDown(GameAction.Crouch)) { player.ChangeState(Player_StateType.CrouchToStand); }
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        // 统一的移动方向计算
        Vector3 cameraForward = player.CurrentCamera.transform.forward;
        Vector3 cameraRight = player.CurrentCamera.transform.right;
            
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();
        currentMoveDirection = (cameraForward * v + cameraRight * h).normalized;
        
        // 手动移动 CharacterController
        float moveSpeed = player.WalkSpeed / 2;
        //player.CharacterController.Move(moveDir * (moveSpeed * Time.deltaTime));
        player.CharacterController.Move(currentMoveDirection * (moveSpeed * Time.deltaTime));
        
        // 设置动画参数 - 关键改进！
        SetMovementAnimationParameters(h, v);
        /*//处理移动
        Vector3 input = new Vector3(h, 0, v);
        //获取y的旋转值
        float y = player.cameraOne.transform.rotation.eulerAngles.y;
        //获取经过y旋转过后的方向向量
        Vector3 moveDir = Quaternion.Euler(0, y, 0) * input;
        
        if (h == 0 && v == 0) { player.ChangeState(Player_StateType.Crouch); }
        else if (h != 0 || v != 0)
        {
            player.characterController.Move(moveDir * (Time.deltaTime * 0.7f));
            //处理模型旋转
            player.playerModel.transform.rotation = Quaternion.Slerp(player.playerModel.transform.rotation, 
                Quaternion.LookRotation(moveDir), player.RotateSpeed * Time.deltaTime);
        }*/
    }
    
    private void SetMovementAnimationParameters(float horizontalInput, float verticalInput)
    {
        player.playerModel.Animator.SetFloat(crouchH, horizontalInput);
        player.playerModel.Animator.SetFloat(crouchV, verticalInput);
    }
    
}
