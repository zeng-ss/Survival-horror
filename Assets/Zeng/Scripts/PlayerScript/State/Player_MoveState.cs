using System.Collections;
using System.Collections.Generic;
using AutoPool_Tool;
using UnityEngine;
/// <summary>
/// 玩家移动状态
/// </summary>
public class Player_MoveState : Player_State
{
    private float lastH, lastV;
    // 减少体力值计时器
    private float bodyPowerTimer;
    // 体力
    
    // 新增方向参数哈希（用于2D混合树的方向维度）
    private static readonly int direction = Animator.StringToHash("Direction");
    // 保留Speed参数（用于2D混合树的走/跑维度）
    private static readonly int speed = Animator.StringToHash("Speed");
    
    private float WalkToRunValue;
    private Vector3 currentMoveDirection;
    public override void Enter() 
    {
        player.PlayerAnimation("Move");
        // 简化逻辑，正确处理所有进入情况
        if (player.lastState == Player_StateType.Jump)
        {
            // 从跳跃状态进入，保持之前的速度混合值
            float normalizedSpeed = Mathf.InverseLerp(player.WalkSpeed, player.RunSpeed, player.currentMoveSpeed);
            WalkToRunValue = Mathf.Clamp01(normalizedSpeed);
        }
        else
        {
            // 从其他状态进入，重置速度
            WalkToRunValue = 0;
            player.currentMoveSpeed = player.WalkSpeed; // 确保有基础速度
        }
        // 设置动画参数
        player.playerModel.Animator.SetFloat(speed, WalkToRunValue);
    }
    
    public override void Update()
    {
        if (KeyBindingManager.Instance.isEscKey) player.ChangeState(Player_StateType.Idle);
        if (KeyBindingManager.Instance.GetActionDown(GameAction.Jump)) { player.ChangeState(Player_StateType.Jump);}
        if (KeyBindingManager.Instance.GetActionDown(GameAction.Crouch)) { player.ChangeState(Player_StateType.Crouch);}
        OnMoveUpdate();
    }

    private void OnMoveUpdate()//移动逻辑
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        if (h == 0 && v == 0) { player.ChangeState(Player_StateType.Idle); }//不移动就切换回Idle状态
        else
        {
            if (Input.GetKey(KeyCode.LeftShift) && !player.isPowerRecovering)
            {
                WalkToRunValue = Mathf.Clamp(WalkToRunValue + Time.deltaTime * player.WalkToRunSpeed, 0, 1);
                // 奔跑时消耗体力
                UpdateBodyPowerConsume();
            }
            else
            {
                WalkToRunValue = Mathf.Clamp(WalkToRunValue - Time.deltaTime * player.WalkToRunSpeed, 0, 1);
            }
            player.playerModel.Animator.SetFloat(speed, WalkToRunValue);
            
            // 统一的移动方向计算
            Vector3 cameraForward = player.CurrentCamera.transform.forward;
            Vector3 cameraRight = player.CurrentCamera.transform.right;
            
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();
            currentMoveDirection = (cameraForward * v + cameraRight * h).normalized;
            
            // 手动移动 CharacterController
            float moveSpeed = Mathf.Lerp(player.WalkSpeed, player.RunSpeed, WalkToRunValue);
            player.currentMoveSpeed = moveSpeed;
            player.CharacterController.Move(currentMoveDirection * (moveSpeed * Time.deltaTime));
            
            // 设置动画参数 ！
            SetMovementAnimationParameters(h, v);
            // 角色始终面向相机方向（第一人称标准做法）
            HandleCharacterRotation();
        }
    }

    /// <summary>
    /// 体力消耗逻辑（奔跑时调用）
    /// </summary>
    private void UpdateBodyPowerConsume()
    {
        if (WalkToRunValue >= 1) // 仅当完全奔跑时消耗体力
        {
            bodyPowerTimer += Time.deltaTime;
            if (bodyPowerTimer >= 0.1f)
            {
                player.CurrentBodyPower -= 0.5f;
                bodyPowerTimer = 0;
            }

            if (player.CurrentBodyPower <= 0)
            {
                player.isPowerRecovering = true;
                player.powerRecoverTimer = 0; // 重置恢复计时器
                SoundAudioPool.Instance.PlaySound(SoundManager.Instance.tiredClip,player.transform.position);
                Debug.Log("Return,Play");
            }
        }
    }

    private void SetMovementAnimationParameters(float horizontalInput, float verticalInput)
    {
        // 1. 计算移动方向的角度（将 h 和 v 转换为 0-360° 的角度）
        float angle = Mathf.Atan2(horizontalInput, verticalInput) * Mathf.Rad2Deg;
        // 确保角度在0-360°范围内（避免负数）
        if (angle < 0) angle += 360;

        // 2. 只有输入变化时才更新动画参数（减少开销）
        if (Mathf.Abs(horizontalInput - lastH) > 0.01f || Mathf.Abs(verticalInput - lastV) > 0.01f)
        {
            // 传递方向角度给混合树的 Direction 参数
            player.playerModel.Animator.SetFloat(direction, angle);
            lastH = horizontalInput;
            lastV = verticalInput;
        }

        // 3. 传递Speed参数（控制走/跑切换，0 = 走，1 = 跑）
        player.playerModel.Animator.SetFloat(speed, WalkToRunValue);
    }
    
    private void HandleCharacterRotation()
    {
        // 在第一人称中，角色模型始终面向相机方向
        // 这样动画系统可以通过 Horizontal 和 Vertical 参数来表现各个方向的移动
        Vector3 cameraForward = player.CurrentCamera.transform.forward;
        cameraForward.y = 0;
        
        if (cameraForward != Vector3.zero)
        {
            player.playerModel.transform.rotation = Quaternion.Slerp(
                player.playerModel.transform.rotation, 
                Quaternion.LookRotation(cameraForward), 
                player.RotateSpeed * Time.deltaTime
            );
        }
    }
    
}
