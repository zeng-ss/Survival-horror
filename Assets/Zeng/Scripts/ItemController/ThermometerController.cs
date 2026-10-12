using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 温度计控制器 
/// </summary>
public class ThermometerController : ItemController
{
    // 文字
    public TextMeshPro text;
    private float normalChangeTimer;
    private float workChangeTimer;
    
    // 温度计状态
    private float currentDegree; // 当前的温度值
    
    // 位置
    private readonly Vector3 thermometerHandRotate = new (-17, 220, 0); // 温度计在手中的旋转值
    private float distance;

    private new void Start() { base.Start(); text.gameObject.SetActive(false); }
    
    private new void Update()
    {
        base.Update();
        if (player == null || player.cameraOne == null || ghost == null)
        {
            return;
        }
        distance = Vector3.Distance(transform.position, ghost.transform.position);
        if (isEqupt)
        {
            if (distance <= 15) WorkChangeDegree();
            else NormalChangeDegree();
        }
    }

    /// <summary>
    /// 正常温度的变化
    /// </summary>
    private void NormalChangeDegree()
    {
        normalChangeTimer += Time.deltaTime;
        if (normalChangeTimer > 1f)
        {
            // 生成随机数并四舍五入到两位小数
            currentDegree = Mathf.Round(Random.Range(30f, 35f) * 100f) / 100f;
            text.text = $"当前温度：{currentDegree.ToString(CultureInfo.CurrentCulture)}°";
            text.color = Color.green;
            normalChangeTimer = 0f;
        }
    }
    
    /// <summary>
    /// 周围有鬼时的温度的变化
    /// </summary>
    private void WorkChangeDegree()
    {
        workChangeTimer += Time.deltaTime;
        float maxDegree;
        float minDegree;
        if (ghost.name == "ShuiGui" || ghost.name == "DiaoSiGui" || ghost.name == "MuGui" || ghost.name == "LiGui")
        {
            maxDegree = 0;
            minDegree = -20;
        }
        else
        {
            maxDegree = GetMaxDegree(distance);
            minDegree = GetMinDegree(distance);
        }
        if (workChangeTimer >= 0.5f)
        {
            // 生成随机数并四舍五入到两位小数
            currentDegree = Mathf.Round(Random.Range(maxDegree, minDegree) * 100f) / 100f;
            text.text = $"当前温度：{currentDegree.ToString(CultureInfo.CurrentCulture)}°";
            if (currentDegree is <= 15 and > 0) text.color = Color.magenta;
            else if (currentDegree <= 0) text.color = Color.red;
            else text.color = Color.green;
            workChangeTimer = 0f;
        }
    }
    
    /// <summary>
    /// 拿到对应的最大温度值
    /// </summary>
    /// <param name="distance">道具和鬼的距离</param>
    /// <returns></returns>
    private float GetMaxDegree(float distance)
    {
        if (distance <= 25f && distance > 15f) return 30;
        if (distance <= 12f && distance > 6f) return 18;
        if (distance <= 6f) return 10;
        return 35;
    }
    
    /// <summary>
    /// 拿到对应的最小温度值
    /// </summary>
    /// <param name="distance">道具和鬼的距离</param>
    /// <returns></returns>
    private float GetMinDegree(float distance)
    {
        if (distance <= 25f && distance > 15f) return 18;
        if (distance <= 15f && distance > 8f) return 10;
        if (distance <= 8f) return 0;
        return 30;
    }

    /// <summary>
    /// 装备道具
    /// </summary>
    /// <param name="handTransform">装备的道具的父物体（手）</param>
    public override void OnEquip(Transform handTransform)
    {
        base.OnEquip(handTransform);
        text.gameObject.SetActive(true);
        // 旋转面对玩家
        transform.localRotation = Quaternion.Euler(thermometerHandRotate);
    }
    
    /// <summary>
    /// 丢弃道具
    /// </summary>
    public override void OnDrop()
    {
        // 先重置旋转（确保往前面丢弃）
        transform.localRotation = Quaternion.identity;
        text.gameObject.SetActive(false);
        base.OnDrop();
    }
    
}
