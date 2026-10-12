using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CircuitbreakerController : MonoBehaviour
{
    // 提示文字
    public TextMeshPro tipText;
    private Material tmpMaterial; // 文字的材质
    private float timer; // 文字时间

    // 灯
    private List<Light> lights = new List<Light>();
    public int maxLightCount; // 最大控制点灯
    
    // 电闸
    [HideInInspector] public bool isOpen; // 判断是否打开了电闸
    private float currentFillAmount; // 当前的进度值
    private float holdTimer; // 添加计时器
    private float holdDuration = 3f; // 开启电闸时间
    private Outline outline;

    void Start()
    {
        outline = GetComponent<Outline>();
        tmpMaterial = tipText.GetComponent<MeshRenderer>().material;
        tmpMaterial.SetFloat("_FaceDilate", -1);
        Light[] allLights = FindObjectsOfType<Light>();
        foreach (Light _light in allLights)
        {
            if (!_light.CompareTag("Flashlight")) { lights.Add(_light); }
        }
    }
    private void Update()
    {
        ControllerLights(); // 控制灯
        OpenCircuitbreaker(); // 控制电闸
    }

    /// <summary>
    /// 打开电闸方法
    /// </summary>
    private void OpenCircuitbreaker()
    {
        if (isOpen) return;
        if (outline.OutlineWidth >= 10) // 说明检测到了电闸（玩家到达了可交互范围且准心对着电闸）
        {
            TextShowAnimation(); // 设置文字效果
            if (Input.GetKey(KeyCode.E)) // 开始打开
            {
                IncreaseFillAmount(); // 增加进度
            }
            else { DecreaseFillAmount(); } // 如果松开了 E 就要减少进度
            
        }
        else // 角色移出可交互范围
        {
            DecreaseFillAmount(); // 减少进度
            tmpMaterial.SetFloat("_FaceDilate", -1); // 将文字透明度设置为透明
            tmpMaterial.SetFloat("_OutlineWidth", 0);
        }
    }

    /// <summary>
    /// 减少进度的方法
    /// </summary>
    private void DecreaseFillAmount()
    {
        if (currentFillAmount > 0) // 如果没有按下或者松开了 E 且还有进度，就要减少进度
        {
            UIManager.Instance.openPanel<CircleTipPanel>().transform.position = new Vector3(0, 400, 0);
            holdTimer = Mathf.Max(0, holdTimer - Time.deltaTime * 2); // 快速减少时间
            currentFillAmount = Mathf.Clamp01(holdTimer / holdDuration); // 快速减少进度
            // 实时设置进度
            UIManager.Instance.getPanel<CircleTipPanel>().progressImage.fillAmount = currentFillAmount;
        }
        else if (currentFillAmount <= 0) // 如果没有进度或者进度减少到 0 ，关闭面板
        { UIManager.Instance.closePanel<CircleTipPanel>(); }
    }
    
    /// <summary>
    /// 增加进度的方法
    /// </summary>
    private void IncreaseFillAmount()
    {
        UIManager.Instance.openPanel<CircleTipPanel>().transform.position = new Vector3(0, 400, 0);
        holdTimer += Time.deltaTime; // 增加按下时间
        currentFillAmount = Mathf.Clamp01(holdTimer / holdDuration); // 增加进度
        // 实时设置进度
        UIManager.Instance.getPanel<CircleTipPanel>().progressImage.fillAmount = currentFillAmount;
        if (currentFillAmount >= 1f) // 进度完成，打开电闸
        {
            // 播放声音
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.openCircuitBreakerClip,transform.position);
            UIManager.Instance.closePanel<CircleTipPanel>();
            tmpMaterial.SetFloat("_FaceDilate", -1); // 隐藏提示文字
            tmpMaterial.SetFloat("_OutlineWidth", 0);
            isOpen = true; holdTimer = 0;
        }
    }

    /// <summary>
    /// 提示文字的动画效果
    /// </summary>
    private void TextShowAnimation()
    {
        if (isOpen)
        {
            tmpMaterial.SetFloat("_FaceDilate", -1);
            return;
        }
        timer += Time.deltaTime;
        // 使用正弦波实现循环效果
        float dilateValue = Mathf.Lerp(-1,0.3f,(Mathf.Sin(timer) + 1) * 0.5f);
        float thicknessValue = Mathf.Lerp(0,0.3f,(Mathf.Sin(timer) + 1) * 0.5f);
        tmpMaterial.SetFloat("_FaceDilate", dilateValue);
        tmpMaterial.SetFloat("_OutlineWidth", thicknessValue);
    }
    
    /// <summary>
    /// 控制灯的方法
    /// </summary>
    private void ControllerLights()
    {
        if (!isOpen) { return; }
        // 每帧清空列表，重新统计当前开启的灯
        List<Light> currentLights = new List<Light>();
        foreach (Light _light in lights)
        {
            if (_light.enabled) { currentLights.Add(_light); } // 保存当前所亮的灯到 currentLights
            if (currentLights.Count > maxLightCount) // 超出最大灯的数量
            {
                foreach (Light currentLight in currentLights)
                {
                    currentLight.enabled = false;
                }
            }
            if (currentLights.Count > maxLightCount)
            {
                // 关闭所有灯
                foreach (Light light in lights)
                {
                    light.enabled = false;
                }
                // 触发跳闸
                OnCircuitBreakerTripped();
            }
        }
    }
    
    /// <summary>
    /// 电闸跳闸时的处理
    /// </summary>
    private void OnCircuitBreakerTripped()
    {
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.circuitBreakerTrippedClip,transform.position,0.02f);
        // 跳闸时重置动画参数
        isOpen = false;
        timer = 0f; // 重置动画计时器，让正弦波重新开始循环
        // 重置材质参数到初始状态，避免动画卡死
        tmpMaterial.SetFloat("_FaceDilate", -1f);
        tmpMaterial.SetFloat("_OutlineWidth", 0f);
    }
    
}
