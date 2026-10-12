using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloorManager : UnitySigleTon<FloorManager>
{
    private PlayerController player;
    [Header("音效设置")]
    [Header("瓷砖音效")]public AudioClip tileAudioClip;
    [Header("水泥音效")]public AudioClip cementAudioClip;
    [Header("木板音效")]public AudioClip woodAudioClip;
    private String currentTag;//当前的地板标签
    [HideInInspector] public AudioClip currentAudioClip;//当前的地板音效
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    void Update()
    {
        RaycastHit hit;
        Vector3 startPosition = player.transform.position + Vector3.up * 0.2f;
        // 调试显示
        Debug.DrawRay(startPosition, Vector3.down * 2f, Color.green);
        if (Physics.Raycast(startPosition, Vector3.down, out hit,2f))
        {
            currentTag = hit.collider.tag;
            switch (currentTag)
            {
                case "Wood":
                    currentAudioClip = woodAudioClip;
                    break;
                case "cement":
                    currentAudioClip = cementAudioClip;
                    break;
                case "Whitetile":
                    currentAudioClip = tileAudioClip;
                    break;
            }
        }
    }
    
}
