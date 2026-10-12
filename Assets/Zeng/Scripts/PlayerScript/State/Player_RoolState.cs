using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_RoolState : Player_State
{
    private float Speed;
    public override void Enter()
    {
        player.PlayerAnimation("Rool");
        player.hasGravity = false;
        player.volecity = Vector3.zero;
        player.playerModel.setRootMotionAction(OnRootMotion);
        
    }

    private void OnRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        player.CharacterController.Move(deltaPosition);
    }

    public override void Update()
    {
        AnimatorStateInfo stateInfo = player.playerModel.Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Rool"))
        {
            float normalizedTime = stateInfo.normalizedTime;
            if (normalizedTime >= 0.9f)
            {
                player.ChangeState(Player_StateType.Idle);
            }
        }
    }

    public override void Exit()
    {
        player.playerModel.clearRootMotionAction();
        player.hasGravity = true;
    }
}
