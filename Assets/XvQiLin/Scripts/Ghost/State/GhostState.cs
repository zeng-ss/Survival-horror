using UnityEngine;
using Pathfinding;
public class GhostState : State_Base
{
        protected GhostController ghost;
    
        public override void Init(IState_MachineOwner owner)
        {
            base.Init(owner);
            ghost = (GhostController)owner;
        }
        protected virtual bool CheckAnimatorStateName(string stateName, out float normalizedTime,int layerIndex = 0)
        {
            AnimatorStateInfo nextInfo = ghost.ghostModle.Animator.GetNextAnimatorStateInfo(layerIndex);
            if (nextInfo.IsName(stateName))//判断当前动画是不是下一个动画
            {
                normalizedTime = nextInfo.normalizedTime;
                return true;
            }
            AnimatorStateInfo info = ghost.ghostModle.Animator.GetCurrentAnimatorStateInfo(layerIndex);
            normalizedTime = info.normalizedTime;
            return info.IsName(stateName);
        }
        // 公共方法供子类使用
        protected void UpdateAnimation()
        {
            ghost.UpdateAnimation();
        }
    
        protected void ApplyGravity()
        {
            ghost.ApplyGravity();
        }
    
        protected bool DetectPlayer()
        {
            return ghost.DetectPlayer();
        }
    
        protected void SeekPath(Vector3 target)
        {
            ghost.SeekPath(target);
        }
    
        protected void FollowPath(float speed)
        {
            ghost.FollowPath(speed);
        }
    
        protected void SetAppearance(bool appear)
        {
            ghost.SetAppearance(appear);
        }
    
        protected void TriggerJumpScare()
        {
            ghost.TriggerJumpScare();
        }
}