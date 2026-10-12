using UnityEngine;
using DG.Tweening;
/// <summary>
/// 蜡烛控制器
/// </summary>
public class CandleController : ItemController
{
    private bool isWork;
    private bool isGhostInArea;
    private GameObject greenFireEffect;
    private GameObject redFireEffect;
    private GameObject blueFireEffect;
    private Light greenFireLight;
    private Light redFireLight;
    private Light blueFireLight;

    private new void Start()
    {
        base.Start();
        greenFireEffect = ResMgr.Instance.load<GameObject>("Model/effects/GreenFire",transform);
        if (greenFireEffect == null)
        {
            print("特效为空！");
            return;
        }
        greenFireEffect.transform.localPosition = new Vector3(0, 0.5f, 0);
        greenFireEffect.transform.localRotation = Quaternion.Euler(90, 0, 0);
        redFireEffect = ResMgr.Instance.load<GameObject>("Model/effects/RedFire",transform);
        if (redFireEffect == null)
        {
            print("特效为空！");
            return;
        }
        redFireEffect.transform.localPosition = new Vector3(0, 0.5f, 0);
        redFireEffect.transform.localRotation = Quaternion.Euler(90, 0, 0);
        blueFireEffect = ResMgr.Instance.load<GameObject>("Model/effects/BlueFire",transform);
        if (blueFireEffect == null)
        {
            print("特效为空！");
            return;
        }
        blueFireEffect.transform.localPosition = new Vector3(0, 0.5f, 0);
        blueFireEffect.transform.localRotation = Quaternion.Euler(90, 0, 0);
        greenFireLight = greenFireEffect.GetComponentInChildren<Light>();
        redFireLight = redFireEffect.GetComponentInChildren<Light>();
        blueFireLight = blueFireEffect.GetComponentInChildren<Light>();
        greenFireEffect.SetActive(false);
        redFireEffect.SetActive(false);
        blueFireEffect.SetActive(false);
    }
    
    private new void Update()
    {
        base.Update();
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
            isOpen = false;
            player.isStartOpen = false;
            player.DropItem();
            transform.tag = "Untagged";
            DOVirtual.DelayedCall(1.5f, StartWork);
        }

        if (isWork)
        {
            switch (ghost.currentState)
            {
                case Ghost_StateType.Idle:
                case Ghost_StateType.Disappear:
                case Ghost_StateType.Normal:
                    greenFireEffect.SetActive(true);
                    redFireEffect.SetActive(false);
                    blueFireEffect.SetActive(false);
                    break;
                case Ghost_StateType.Patrol:
                    greenFireEffect.SetActive(false);
                    redFireEffect.SetActive(true);
                    blueFireEffect.SetActive(false);
                    break;
                case Ghost_StateType.Hunt:
                case Ghost_StateType.Angry: 
                    greenFireEffect.SetActive(false);
                    redFireEffect.SetActive(false);
                    blueFireEffect.SetActive(true);
                    break;
            }
            if (isGhostInArea)
            {
                if (ghost.name == "DiaoSiGui" 
                    || ghost.name == "NanChanGui" 
                    || ghost.name == "LiGui" 
                    || ghost.name == "JingZhongHun" )
                {
                    switch (ghost.currentState)
                    {
                        case Ghost_StateType.Idle:
                        case Ghost_StateType.Disappear:
                        case Ghost_StateType.Normal:
                            greenFireLight.intensity = 1f;
                            redFireLight.intensity = 0.1f;
                            blueFireLight.intensity = 0.1f;
                            break;
                        case Ghost_StateType.Patrol:
                            greenFireLight.intensity = 0.1f;
                            redFireLight.intensity = 1f;
                            blueFireLight.intensity = 0.1f;
                            break;
                        case Ghost_StateType.Hunt:
                        case Ghost_StateType.Angry: 
                            greenFireLight.intensity = 0.1f;
                            redFireLight.intensity = 0.1f;
                            blueFireLight.intensity = 1f;
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 蜡烛开始作用方法
    /// </summary>
    private void StartWork()
    {
        isWork = true;
        transform.rotation = Quaternion.identity;
        GameObject rangeEffect = ResMgr.Instance.load<GameObject>("Model/effects/T1WhiteLootItem",transform);
        if (rangeEffect == null)
        {
            print("特效为空！");
            return;
        }
        rangeEffect.transform.localPosition = new Vector3(0, 0.02f, 0);
        rangeEffect.transform.localRotation = Quaternion.Euler(-90, 0, 0);
    }
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ghost"))
        {
            isGhostInArea = true;
        }
    }
    
}
