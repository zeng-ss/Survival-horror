using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_CrouchState : Player_State
{
    private bool hasEnteredCrouchPose;
    public enum CrouchState
    {
        Crouching, // 下蹲过程
        CrouchPose, // 下蹲完成的 Pose
    }
    private CrouchState state;

    public CrouchState State
    {
        get => state;
        set
        {
            state = value;
            switch (state)
            {
                case CrouchState.Crouching:
                    player.PlayerAnimation("Crouch");
                    break;
                case CrouchState.CrouchPose:
                    player.PlayerAnimation("CrouchPose");
                    break;
            }
        }
    }
    
    public override void Enter()
    {
        if (player.lastState == Player_StateType.CrouchWalk)
        {
            State = CrouchState.CrouchPose;
            hasEnteredCrouchPose = true;
        }
        else
        {
            State = CrouchState.Crouching;
            hasEnteredCrouchPose = false;
        }
        
    }

    public override void Update()
    {
        if (KeyBindingManager.Instance.isEscKey) return;
        switch (state)
        {
            case CrouchState.Crouching:
                
                if (CheckAnimatorStateName("Crouch",out float animationTime))
                {
                    if (animationTime >= 0.95f)
                    {
                        State = CrouchState.CrouchPose;
                        hasEnteredCrouchPose = true;
                    }
                    UpdateCharacterControllerAndCamera(animationTime);
                }
                break;
            case CrouchState.CrouchPose:

                if (hasEnteredCrouchPose) { hasEnteredCrouchPose = false; }
                if (KeyBindingManager.Instance.GetActionDown(GameAction.Crouch))
                {
                    player.ChangeState(Player_StateType.CrouchToStand);
                    return;
                }
                IsMove();
                break;
        }
    }

    /// <summary>
    /// 检测移动方法
    /// </summary>
    private void IsMove()
    {
        if (KeyBindingManager.Instance.isEscKey) return;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (h != 0 || v != 0) 
        { player.ChangeState(Player_StateType.CrouchWalk); }
    }
    
    private void UpdateCharacterControllerAndCamera(float speed)
    {
        Vector3 standingCameraHeight = new Vector3(0.006f, 1.696f, -0.161f);
        Vector3 crouchingCameraHeight = new Vector3(0.01452732f, 0.678f, -0.1294771f);
        Vector3 crouchingCenter = new Vector3(0, 0.61f, 0);
        float crouchingHeight = 1.22f;
        float standingHeight = 1.8f;
        Vector3 standingCenter = new Vector3(0, 0.93f, 0);

        if (player.characterController != null)
        {
            float progress = speed;
            // 使用更平滑的插值
            float targetHeight = Mathf.SmoothStep(standingHeight, crouchingHeight, progress);
            Vector3 targetCenter = Vector3.Lerp(standingCenter, crouchingCenter, progress);
            Vector3 targetCameraPos = Vector3.Slerp(standingCameraHeight, crouchingCameraHeight, progress);
        
            player.characterController.height = targetHeight;
            player.characterController.center = targetCenter;
            player.cameraOne.transform.localPosition = targetCameraPos;
        }
    }
    
    
}
