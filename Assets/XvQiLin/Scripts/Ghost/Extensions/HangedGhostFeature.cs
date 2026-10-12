// Scripts/Ghost/Extensions/HangedGhostFeature.cs
using UnityEngine;

/// <summary>
/// 吊死鬼专属特性组件，挂载在GhostController同一对象上
/// </summary>
[RequireComponent(typeof(GhostController))]
public class HangedGhostFeature : MonoBehaviour, IGhostFeature
{
    private GhostController ghost;
    private GhostFeatureConfig config;
    private Transform currentHangPoint; // 当前目标悬挂点

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