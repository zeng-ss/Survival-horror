using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DoorMove : MonoBehaviour
{
    public BoxCollider boxCollider;
    public GameObject doorLeft;
    public GameObject doorRight;
    private Vector3 startLeftPos;
    private Vector3 startRightPos;
    private bool isOpen;
    private bool isPlayerInTrigger;
private void Start()
{
    startRightPos = doorRight.transform.position;
    startLeftPos = doorLeft.transform.position;
    // 确保碰撞体是触发器
    if (boxCollider != null) { boxCollider.isTrigger = true; }
}

private void Update()
{
    // 在Update中检测按键，但只在玩家在触发区域内时有效
    if (isPlayerInTrigger && Input.GetKeyDown(KeyCode.F))
    {
        if (!isOpen) OpenDoors();
        else CloseDoors();
    }
}

private void OnTriggerEnter(Collider other)
{
    // 检查进入碰撞体的对象是否是玩家
    if (other.gameObject.CompareTag("Player")) { isPlayerInTrigger = true; }
}

private void OnTriggerExit(Collider other)
{
    if (other.gameObject.CompareTag("Player")) { isPlayerInTrigger = false; }
}

private void OpenDoors()
{
    if (isOpen) return;
    doorLeft.transform.DOMove(doorLeft.transform.position + -Vector3.right, 1.5f);
    doorRight.transform.DOMove(doorRight.transform.position + Vector3.right, 1.5f).OnComplete(() => {isOpen = true;});
}

private void CloseDoors()
{
    if (!isOpen) return;
    doorLeft.transform.DOMove(startLeftPos, 1.5f);
    doorRight.transform.DOMove(startRightPos, 1.5f).OnComplete(() => {isOpen = false;});
}
}
