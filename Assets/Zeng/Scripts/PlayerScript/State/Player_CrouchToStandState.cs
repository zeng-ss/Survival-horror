using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_CrouchToStandState : Player_State
{
    public override void Enter()
    {
        player.PlayerAnimation("CrouchToStand");
    }

    public override void Update()
    {
        if (CheckAnimatorStateName("CrouchToStand", out float animationTime))
        {
            if (animationTime >= 1f) { player.ChangeState(Player_StateType.Idle); }
            UpdateCharacterControllerAndCamera(animationTime);
        }
    }
    /// <summary>
    /// 更新相机和 CharacterController 组件位置
    /// </summary>
    /// <param name="speed">更新的进度（动画的进度）</param>
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
            float progress = 1f - speed;
        
            float targetHeight = Mathf.Lerp(standingHeight, crouchingHeight, progress);
            Vector3 targetCameraPos = Vector3.Lerp(standingCameraHeight, crouchingCameraHeight, progress);
            Vector3 targetCenter = Vector3.Lerp(standingCenter, crouchingCenter, progress);
        
            player.characterController.height = targetHeight;
            player.characterController.center = targetCenter;
            player.cameraOne.transform.localPosition = targetCameraPos;
        }
    }
}
