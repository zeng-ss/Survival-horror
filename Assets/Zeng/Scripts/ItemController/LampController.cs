using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 灯泡道具控制器
/// </summary>
public class LampController : ItemController
{
    private bool isGhostInArea;
    private GameObject ghostModel; // 创建的鬼的黑色模型
    private GameObject effect;     // 特效
    private Light baseLight;       // 基础发光效果
    private float showTimer;       // 鬼出现时计时器
    private float waitTimer;       // 下一次鬼出现计时器
    private SphereCollider sphereCollider;
    private Coroutine ghostShowCoroutine; // 标记是否正在执行鬼显隐的协程，避免重复触发
    private new void Start()
    {
        base.Start();
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.enabled = false;
        baseLight = GetComponentInChildren<Light>();
        baseLight.enabled = false;
    }


    private new void Update()
    {
        base.Update();
        if (player == null || player.cameraOne == null || ghost == null)
        {
            return;
        }
        // 只有捡起来且已经装备了才可以开始道具功能
        if (transform.parent == player.cameraOne.transform && gameObject.activeInHierarchy && !isOpen)
        {
            if (KeyBindingManager.Instance.GetAction(GameAction.UsingItem))
            {
                OpenIncreaseFillAmount();
                if (holdTimer >= duration) // 进度完成，开启道具功能
                {
                    isOpen = true; holdTimer = 0;
                    sphereCollider.enabled = true;
                    player.isStartOpen = false;
                    player.DropItem();
                    transform.tag = "Untagged";
                    baseLight.enabled = true;
                    UIManager.Instance.closePanel<HorProgressPanel>();
                }
            }
            else { OpenDecreaseFillAmount(); }
        }
        else if (isOpen)
        {
            OnWork();
        }
        if (effect != null)
        {
            effect.transform.up = new Vector3(0, 0, -90);
            ghostModel.transform.up = new Vector3(0, 0, 0);
        }
    }

    private void OnWork()
    {
        if (!isOpen) return;
        if (isGhostInArea)
        {
            if (effect == null &&
                (ghost.name == "LiGui"|| ghost.name == "JingZhongHun"||
                 ghost.name == "HuaPi"|| ghost.name == "ZhaiGui"))
            {
                // 创建空物体作为中间层
                GameObject effectRoot = new GameObject("HologramRed_Root");
                effectRoot.transform.SetParent(transform);
                effectRoot.transform.localPosition = Vector3.zero;
                effectRoot.transform.localRotation = Quaternion.identity; // 空物体继承位置，旋转清零

                // 加载特效到空物体下
                effect = ResMgr.Instance.load<GameObject>("Model/effects/HologramRed", effectRoot.transform);
                if (effect == null)
                {
                    print("HologramRed_Root not found");
                    return;
                }
                effect.transform.localScale = Vector3.one * 0.85f;
                effect.transform.localRotation = Quaternion.identity; // 特效旋转独立，始终朝上
                
                // 创建鬼的黑色模型
                GhostData ghostData = GameManager.Instance.GhostDataDict[GameManager.Instance.currentChooseGhostId];
                ghostModel = ResMgr.Instance.load<GameObject>($"Model/ghostModel/{ghostData.model}", effectRoot.transform);
                if (ghostModel == null)
                {
                    print("ghostModel为空！");
                    return;
                }
                ghostModel.transform.localScale = Vector3.one * 0.6f;
                // 初始化：鬼模型先隐藏
                ghostModel.SetActive(false);
                // 拿到鬼的材质
                List<SkinnedMeshRenderer> ghostRendererList = new List<SkinnedMeshRenderer>();
                ghostModel.GetComponentsInChildren(true, ghostRendererList);
                Material material = ResMgr.Instance.load<Material>("Material&Animation&Fonts/Black");
                if (material == null)
                {
                    print("黑色材质为空");
                    return;
                }
                // 设置材质为黑色
                foreach (var gRenderer in ghostRendererList)
                {
                    gRenderer.material = material;
                }
                // 启动协程：处理鬼的显隐逻辑
                // 如果已有协程在运行，先停止（避免重复）
                if (ghostShowCoroutine != null)
                {
                    StopCoroutine(ghostShowCoroutine);
                }
                ghostShowCoroutine = StartCoroutine(GhostShowSequence());
            }
        }
        else { ClearGhostAndEffect(); }
    }
    
    /// <summary>
    /// 鬼显隐的核心协程逻辑
    /// </summary>
    private IEnumerator GhostShowSequence()
    {
        while (isOpen && isGhostInArea && ghostModel != null)
        {
            // 随机等待 2~8 秒
            float waitTime = Random.Range(2, 8);
            yield return new WaitForSeconds(waitTime);
            // 显示鬼模型
            ghostModel.SetActive(true);
            // 显示 0.3秒后隐藏
            yield return new WaitForSeconds(0.3f);
            ghostModel.SetActive(false);
        }
    }

    /// <summary>
    /// 清理鬼模型、特效和协程
    /// </summary>
    private void ClearGhostAndEffect()
    {
        // 停止协程
        if (ghostShowCoroutine != null)
        {
            StopCoroutine(ghostShowCoroutine);
            ghostShowCoroutine = null;
        }
        // 销毁/隐藏特效和鬼模型（根据你的资源管理方式选择）
        if (effect != null)
        {
            // 销毁中间层空物体（连带特效和模型）
            Destroy(effect.transform.parent.gameObject); 
            effect = null;
        }
        ghostModel = null;
    }
    
    /// <summary>
    /// 道具关闭时清理资源
    /// </summary>
    private void OnDestroy()
    {
        ClearGhostAndEffect();
        if (sphereCollider != null) { sphereCollider.enabled = false; }
        if (baseLight != null) { baseLight.enabled = false; }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ghost"))
        {
            isGhostInArea = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ghost"))
        {
            isGhostInArea = false;
        }
    }
    
}
