using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostModle : MonoBehaviour
{
    //拿到动画控制器
    [SerializeField] private Animator ghost;
    
    public Animator Animator { get => ghost; } //可以拿到动画控制器

    private Action footStepAction;//脚步声方法
    
    private Action SkillHitAction;//技能伤害方法

    public void Init(Action footStepAction )
    {
        this.footStepAction = footStepAction;
    }

    //执行脚步声音方法
    public void footStep()
    {
        footStepAction?.Invoke();
    }
    
    #region 根运动

    public Action<Vector3, Quaternion> rootMotionAction;

    /// <summary>
    /// 设置跟运动
    /// </summary>
    /// <param name="rootMotionAction"></param>
    public void setRootMotionAction(Action<Vector3, Quaternion> rootMotionAction)
    {
        this.rootMotionAction = rootMotionAction;
    }
    /// <summary>
    /// 清除跟运动
    /// </summary>
    public void clearRootMotionAction()
    {
        this.rootMotionAction = null;
    }

    /// <summary>
    /// 开启跟运动方法 一帧一帧执行
    /// </summary>
    private void OnAnimatorMove()
    {
        //Animator.deltaPosition是相对于上一帧偏移的位置，Animator.deltaRotation是相对于上一帧偏移的
        this.rootMotionAction?.Invoke(Animator.deltaPosition, Animator.deltaRotation);
    }

    #endregion
}
