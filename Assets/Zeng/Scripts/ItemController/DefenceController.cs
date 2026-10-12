using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 符箓的防御控制器 
/// </summary>
public class DefenceController : ItemController
{
    [Header("护盾配置")]
    public float orbitRadius = 0.5f;
    public float orbitSpeed = 240f;
    private int shieldCount = 3; // 三个护盾
    private List<GameObject> shields = new ();
    private Coroutine shieldCoroutine;

    private new void Start() { base.Start(); }
    
    new void Update()
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
                    UIManager.Instance.closePanel<HorProgressPanel>();
                }
            }
            else { OpenDecreaseFillAmount(); }
        }
        else if (isOpen)
        {
            // 初始化护盾后立即销毁道具
            SpawnShields();
            DiscardAndDestroyItem();
            isOpen = false; // 避免重复执行
        }
    }
    
    /// <summary>
    /// 生成三个独立旋转的护盾
    /// </summary>
    private void SpawnShields()
    {
        float angleStep = 360f / shieldCount; // 每个护盾间隔120度
        for (int i = 0; i < shieldCount; i++)
        {
            // 加载预制体
            GameObject shieldPrefab = ResMgr.Instance.load<GameObject>("Model/effects/PowerupShield");
            if (shieldPrefab == null)
            {
                Debug.LogError("护盾预制体加载失败！检查路径：Model/effects/PowerupShield");
                continue;
            }
            // 实例化护盾
            GameObject shield = shieldPrefab;
            shield.name = "DefenceEffect_" + i;

            // 给护盾添加旋转跟随脚本
            ShieldOrbit orbit = shield.AddComponent<ShieldOrbit>();
            orbit.target = player.transform; // 跟随角色
            orbit.orbitRadius = this.orbitRadius;
            orbit.orbitSpeed = this.orbitSpeed;
            orbit.initialAngle = i * angleStep; // 初始角度偏移（0/120/240度）
            shields.Add(shield);
        }
        shieldCoroutine = StartCoroutine(WaitToDestoryShield());
    }

    private IEnumerator WaitToDestoryShield()
    {
        ghost.SetPlayerInvulnerable();
        // 30 秒摧毁护盾
        yield return new WaitForSeconds(30);
        foreach (GameObject shield in shields) { Destroy(shield); }
        shields.Clear();
    }

    private void OnDestroy()
    {
        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
            shieldCoroutine = null;
        }
    }

    /// <summary>
    /// 丢弃并销毁道具
    /// </summary>
    private void DiscardAndDestroyItem()
    {
        player.isStartOpen = false;
        player.DropItem();
        transform.parent = null;
    }
    
}
