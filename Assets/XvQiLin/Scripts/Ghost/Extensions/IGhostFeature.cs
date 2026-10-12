using UnityEngine;

/// <summary>
/// 鬼魂特性扩展接口，定义所有鬼魂专属行为的扩展点
/// </summary>
public interface IGhostFeature
{
    /// <summary>
    /// 初始化扩展（关联GhostController）
    /// </summary>
    void OnInit(GhostController ghost);

    /// <summary>
    /// 每帧更新扩展
    /// </summary>
    void OnUpdate();

    /// <summary>
    /// 状态进入扩展
    /// </summary>
    void OnStateEnter(Ghost_StateType stateType);

    /// <summary>
    /// 执行独特互动行为
    /// </summary>
    void PerformUniqueAction();

    /// <summary>
    /// 获取专属配置
    /// </summary>
    GhostFeatureConfig GetConfig();
}