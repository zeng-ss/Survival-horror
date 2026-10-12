using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class ElevatorRotate : MonoBehaviour
{
    private bool isOpen;
    [Header("左边门")]
    public GameObject leftDoor;
    private Vector3 leftOpenPosition;
    private Vector3 leftClosedPosition;

    [Header("右边门")]
    public GameObject rightDoor;
    private Vector3 rightOpenPosition;
    private Vector3 rightClosedPosition;
    void Start()
    {
        EventCenter.Instance.AddEventListener<ElevatorRotate>(GameEvent.开电梯事件,OnElevatorOpen);
        EventCenter.Instance.AddEventListener<ElevatorRotate>(GameEvent.关电梯事件,OnElevatorClose);
        leftClosedPosition = leftDoor.transform.position;
        leftOpenPosition = leftClosedPosition + -Vector3.right * 1;
        rightClosedPosition = rightDoor.transform.position;
        rightOpenPosition = rightClosedPosition + Vector3.right * 1;
    }

    private void OnElevatorOpen(ElevatorRotate elevator)
    {
        if (elevator == null) { print("elevator is null"); return; }
        if (elevator == this)
        {
            if(!isOpen) OpenDoor();
        }
    }

    private void OnElevatorClose(ElevatorRotate elevator)
    {
        if (elevator == null) { print("elevator is null"); return; }
        if(isOpen) CloseDoor();
    }

    private void OpenDoor()
    {
        if(isOpen) return;
        StartCoroutine(OpenAfterDelay());
    }
    
    private IEnumerator OpenAfterDelay()
    {
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.elevatorTipClip,transform.position);
        yield return new WaitForSeconds(1.5f);
        leftDoor.transform.DOMove(leftOpenPosition, 1).OnComplete(() => {isOpen = true; });
        rightDoor.transform.DOMove(rightOpenPosition, 1).OnComplete(() => {isOpen = true; });
    }

    private void CloseDoor()
    {
        if (!isOpen) return;
        leftDoor.transform.DOMove(leftClosedPosition, 1).OnComplete(() => {isOpen = false; });
        rightDoor.transform.DOMove(rightClosedPosition, 1).OnComplete(() => {isOpen = false; });
    }

    void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener<ElevatorRotate>(GameEvent.开电梯事件,OnElevatorOpen);
        EventCenter.Instance.RemoveEventListener<ElevatorRotate>(GameEvent.关电梯事件,OnElevatorClose);
    }
    
}