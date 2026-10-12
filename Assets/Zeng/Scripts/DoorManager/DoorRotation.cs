using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DoorRotation : MonoBehaviour
{
    #region 数据

    private float duration = 1;      // 旋转时间
    private float openAngle = 90f;   // 旋转角度
    private PlayerController player; // 玩家对象
    private Vector3 initialRotation; // 门的初始位置
    private bool isOpen;             // 判断门是否打开
    private bool isAnimating;        // 是否正在播放动画
    [Header("轴物体（旋转点）")] public Transform RoundY;

    #endregion

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        initialRotation = RoundY.localEulerAngles;
        EventCenter.Instance.AddEventListener<DoorRotation>(GameEvent.开门事件, OnDoorInteract);
    }
    
    void OnDestroy()
    {
        // 移除事件监听
        EventCenter.Instance.RemoveEventListener<DoorRotation>(GameEvent.开门事件, OnDoorInteract);
    }

    /// <summary>
    /// 门交互事件处理
    /// </summary>
    /// <param name="interactedDoor">被交互的门</param>
    private void OnDoorInteract(DoorRotation interactedDoor)
    {
        if (interactedDoor == null) { print("门为空"); return; }
        // 只有被交互的这个门才会响应
        if (interactedDoor == this)
        {
            if (!isOpen) { OpenDoor(player.transform); }
            else { CloseDoor(); }
        }
    }

    public void OpenDoor(Transform playerTransform)
    {
        if(isOpen || isAnimating) return;
        isAnimating = true;
        // 1. 计算角色相对于门的方向
        Vector3 relativePosition = playerTransform.position - RoundY.position;
        // 2. 判断角色在门的正面还是背面（假设门的初始朝向为前方）
        //  Dot乘积大于0，角色在门前方；否则在后方
        bool isInFront = Vector3.Dot(RoundY.forward, relativePosition) > 0;
        // 3. 决定最终的旋转角度和目标角度
        // 例如：角色在门前，门向内开；在门后，门向外开。可根据需求调整
        float targetYRotation = initialRotation.y + (isInFront ? -openAngle : openAngle);
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.openDoorClip,transform.position);
        //旋转门
        transform.DOLocalRotate(new Vector3(initialRotation.x, targetYRotation, initialRotation.z), duration)
            .SetEase(Ease.OutQuad).OnComplete(() => { isOpen = true;isAnimating = false; }); // 使用缓动函数让动画更自然
    }

    private void CloseDoor()
    {
        if(!isOpen || isAnimating) return;
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.closeDoorClip,transform.position);
        isAnimating = true;
        transform.DOLocalRotate(initialRotation, duration)
            .SetEase(Ease.OutQuad).OnComplete(() => { isOpen = false;isAnimating = false; }); // 使用缓动函数让动画更自然
    }
}
