using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Player_DeadState : Player_State
{
    private Vector3 originCameraPos;
    private Vector3 targetCameraPos;

    public override void Enter()
    {
        player.PlayerAnimation("Dead");
        DeadRestAndCameraMove(); // 死亡效果和移动相机
    } 
    
/// <summary>
/// 死亡效果和移动相机方法
/// </summary>
    private void DeadRestAndCameraMove()
    {
        UIManager.Instance.closePanel<PlayerProPanel>();
        EventCenter.Instance.EventTrigger(GameEvent.光标出现);
        // 保存相机的原始位置
        originCameraPos = player.cameraOne.transform.localPosition;
        targetCameraPos = originCameraPos + Vector3.up * 1.5f;
        player.playerModel.setRootMotionAction(OnRootMotion);
        // 为相机添加角色层，使可以看到角色
        player.cameraOne.cullingMask |= 1 << LayerMask.NameToLayer("PlayerModel");
        // 创建动画序列
        Sequence seq = DOTween.Sequence();
        seq.Append(player.cameraOne.transform.DOLocalMove(targetCameraPos, 1f));
        seq.Join(player.cameraOne.transform.DOLookAt(player.transform.position, 1f))
            .OnComplete(() =>
            {
                // 开启死亡后处理效果
                player.cameraOne.GetComponent<CameraController>().StartDeadPost();
                DOVirtual.DelayedCall(3.5f, () => { UIManager.Instance.openPanel<FailGamePanel>(); });
            });
    }

    private void OnRootMotion(Vector3 arg1, Quaternion arg2) { player.CharacterController.Move(arg1); }
    public override void Exit() { player.playerModel.clearRootMotionAction(); }
}
