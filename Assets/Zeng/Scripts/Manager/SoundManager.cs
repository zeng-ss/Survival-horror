using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI的声音实现的管理器
/// </summary>
public class SoundManager : UnitySingleTonMono<SoundManager>
{
    [HideInInspector] public AudioSource audioSource;
    [Header("开始场景背景音效")] public AudioClip startSound;
    [Header("开门音效")] public AudioClip openDoorClip;
    [Header("捡起音效")] public AudioClip pickUpClip;
    [Header("按钮二级音效")] public AudioClip baseBtnClick;
    [Header("按钮叮咚音效")] public AudioClip mesBtnClick;
    [Header("按钮开菜单音效")] public AudioClip menuBtnClick;
    [Header("关门音效")] public AudioClip closeDoorClip;
    [Header("开关灯音效")] public AudioClip switchClip;
    [Header("电梯到达音效")] public AudioClip elevatorTipClip;
    [Header("跳闸音效")] public AudioClip circuitBreakerTrippedClip;
    [Header("打开电闸音效")] public AudioClip openCircuitBreakerClip;
    [Header("火焰音效")] public AudioClip fireClip;
    [Header("喘息音效")] public AudioClip tiredClip;
    [Header("玩家死亡音效01")] public AudioClip playerDieClip01;
    [Header("玩家死亡音效02")] public AudioClip playerDieClip02;
    [Header("鬼的出现音效01")] public AudioClip ghostShow01;
    [Header("鬼的出现音效02")] public AudioClip ghostShow02;
    [Header("鬼的狂躁音效01")] public AudioClip ghostAngry01;
    [Header("鬼的跳脸音效01")] public AudioClip ghostJumpScary01;
    [Header("鬼的跳脸音效02")] public AudioClip ghostJumpScary02;
    [Header("鬼的跳脸音效03")] public AudioClip ghostJumpScary03;
    [Header("鬼的随机音效01")] public AudioClip ghostSuiJi01;
    [Header("鬼的随机音效02")] public AudioClip ghostSuiJi02;
    [Header("鬼的随机音效03")] public AudioClip ghostSuiJi03;
    [Header("鬼魂基础杂乱音效")] public List<AudioClip> baseGhostClips = new();
    [Header("鬼魂字幕音效")] public List<AudioClip> subtitlesGhostClips = new();
    [Header("剧情字幕音效")] public List<AudioClip> plotGhostClips = new();
    void Start() { audioSource = GetComponent<AudioSource>(); }
}
