using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GhostFeatureConfig", menuName = "Ghost/Create Feature Config", order = 1)]
public class GhostFeatureConfig : ScriptableObject
{
    [Header("基础特性配置")]
    [Tooltip("当前鬼魂类型")]
    public GhostType ghostType;

    [Header("水鬼专属配置")]
    [Tooltip("水中移动速度倍率")]
    public float waterSpeedMultiplier = 1.5f;
    [Tooltip("是否允许水中瞬移")]
    public bool canTeleportInWater = true;
    [Tooltip("水域检测层")]
    public LayerMask waterLayer;
    [Tooltip("水中瞬移范围（米）")]
    public float waterTeleportRange = 5f;

    [Header("吊死鬼专属配置")]
    [Tooltip("悬挂攻击范围（米）")]
    public float hangRange = 8f;
    [Tooltip("绞杀持续时间（秒）")]
    public float strangleDuration = 5f;
    [Tooltip("绞杀伤害值")]
    public int strangleDamage = 20;
    [Tooltip("绞杀降低理智值")]
    public int strangleSanityLoss = 30;
    [Tooltip("悬挂点列表（房梁/树枝等）")]
    public List<Transform> hangPoints;

    [Header("通用音效配置")]
    [Tooltip("专属待机音效")]
    public AudioClip uniqueIdleSound;
    [Tooltip("专属猎杀音效")]
    public AudioClip uniqueHuntSound;
    [Tooltip("专属互动音效")]
    public AudioClip uniqueActionSound;
}