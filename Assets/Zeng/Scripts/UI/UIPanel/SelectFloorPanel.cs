using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SelectFloorPanel : BasePanel
{
    #region 数据
    
    private PlayerController player;
    public Text select1Text;
    public Button select1; //选项 1
    public Button select2; //选项 2
    public Text select2Text;
    public Button backButton;

    #endregion
    
    void OnEnable()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController>();//拿到角色
        select1.onClick.AddListener(OnSelect1);
        select2.onClick.AddListener(OnSelect2);
        backButton.onClick.AddListener(() =>
        {
            //播放音效
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick,transform.position);
            UIManager.Instance.closePanel<SelectFloorPanel>();
            EventCenter.Instance.EventTrigger(GameEvent.光标消失);
        });
    }

    void Update()
    {
        float y = player.transform.position.y;
        //拿到角色所在楼层
        if (-4f <= y && y < -2f) { player.currentFloor = -1; }
        if (-0.1f <= y && y < 2f) { player.currentFloor = 1; }
        if (3.68f <= y) { player.currentFloor = 2; }
        switch (player.currentFloor)
        {
            case -1:
                select1Text.text = "< 1 >";
                select2Text.text = "< 2 >";
                break;
            case 1:
                select1Text.text = "< -1 >";
                select2Text.text = "< 2 >";
                break;
            case 2:
                select1Text.text = "< -1 >";
                select2Text.text = "< 1 >";
                break;
        }
    }
    
    /// <summary>
    /// 按下选项一
    /// </summary>
    private void OnSelect1()
    {
        //播放音效
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick,transform.position);
        //拿到目的地楼层
        int targetFloor = GetTargetFloorFromText(select1Text.text);
        ElevatorManager.Instance.StartElevatorSequence(targetFloor, player.currentFloor);
        UIManager.Instance.closePanel<SelectFloorPanel>();
        EventCenter.Instance.EventTrigger(GameEvent.光标消失);
    }
    
    private void OnSelect2()
    {
        //播放音效
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick,transform.position);
        int targetFloor = GetTargetFloorFromText(select2Text.text);
        ElevatorManager.Instance.StartElevatorSequence(targetFloor, player.currentFloor);
        UIManager.Instance.closePanel<SelectFloorPanel>();
        EventCenter.Instance.EventTrigger(GameEvent.光标消失);
    }

    private int GetTargetFloorFromText(string text)
    {
        switch (text)
        {
            case "< -1 >": return -1;
            case "< 1 >": return 1;
            case "< 2 >": return 2;
            default: return 0;
        }
    }

    private void OnDisable()
    {
        select1.onClick.RemoveListener(OnSelect1);
        select2.onClick.RemoveListener(OnSelect2);
        backButton.onClick.RemoveListener(() => { UIManager.Instance.closePanel<SelectFloorPanel>(); });
    }
}
