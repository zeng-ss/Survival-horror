// Scripts/Ghost/Extensions/WaterGhostFeature.cs
using UnityEngine;

/// <summary>
/// 水鬼专属特性组件，挂载在GhostController同一对象上
/// </summary>
[RequireComponent(typeof(GhostController))]
public class WaterGhostFeature : MonoBehaviour, IGhostFeature
{
    private GhostController ghost;
    private GhostFeatureConfig config;
    private bool isInWater; // 是否处于水域中

    #region IGhostFeature接口实现
    public void OnInit(GhostController ghost)
    {
    }

    public void OnUpdate()
    {
    }

    public void OnStateEnter(Ghost_StateType stateType)
    {
    }

    public void PerformUniqueAction()
    {
    }

    public GhostFeatureConfig GetConfig()
    {
        return config;
    }
    #endregion
    
}