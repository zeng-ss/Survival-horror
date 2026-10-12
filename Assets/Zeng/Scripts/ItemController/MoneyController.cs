using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
/// <summary>
/// 纸钱控制器 
/// </summary>
public class MoneyController : ItemController
{
    // 偏移
    [Header("火焰的偏移值")] public Vector3 fireOffset;
    [Header("火焰的偏旋转值")] public Vector3 fireRotation;
    
    // 状态
    private GameObject fireEffect; // 有鬼反应的效果
    private bool isWork;
    
    // 文字
    public TextMeshPro tipText;
    private float t; // 文字大小变化计时器
    private new void Start() { base.Start(); tipText.gameObject.SetActive(false); }
    private new void Update()
    {
        base.Update();
        if (player == null || player.cameraOne == null || ghost == null)
        {
            return;
        }
        // 实现提示文字的动态效果
        if (tipText.gameObject.activeSelf)
        {
            t += Time.deltaTime;
            tipText.fontSize = Mathf.Lerp(35, 50, (Mathf.Sin(t) + 1) * 0.5f);
        }
        // 只有捡起来且已经装备了才可以开始道具功能
        if (transform.parent == player.cameraOne.transform && gameObject.activeInHierarchy && !isOpen)
        {
            tipText.gameObject.SetActive(true);
            tipText.text = "长按右键开启";
            if (KeyBindingManager.Instance.GetAction(GameAction.UsingItem))
            {
                OpenIncreaseFillAmount();
            }
            else { OpenDecreaseFillAmount(); }
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            tipText.gameObject.SetActive(false); // 隐藏文字
            if (isOpen) OnWork();
        }

        if (isWork)
        {
            float dis = Vector3.Distance(transform.position, ghost.transform.position);
            if (dis <= 1 && ghost.lastState != Ghost_StateType.Idle)
            {
                if (ghost.gameObject.name == "MuGui" 
                    || ghost.gameObject.name == "NanChanGui" 
                    || ghost.gameObject.name == "ZhaiGui" 
                    || ghost.gameObject.name == "JingZhongHun" )
                {
                    ghost.ChangeState(Ghost_StateType.Idle);
                }
            }
        }
    }

    /// <summary>
    /// 纸钱工作的方法
    /// </summary>
    private void OnWork()
    {
        if (!isOpen) return;
        isOpen = false;              // 重置打开状态
        transform.tag = "Untagged";  // 重置标签防止被捡起
        player.isStartOpen = false;  // 重置是否可以切换状态
        player.DropItem(); t = 0;    
        DOVirtual.DelayedCall(1.5f, () =>    // 1.5s之后开启效果
        {
            isWork = true;
            transform.rotation = Quaternion.identity; // 重置旋转
            ghost.SetChaseTarget(transform.position);
            fireEffect = ResMgr.Instance.load<GameObject>("Model/effects/PurpleFire", transform);
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.fireClip,transform.position);
            fireEffect.transform.localPosition = fireOffset;
            fireEffect.transform.localRotation = Quaternion.Euler(fireRotation);
            DOVirtual.DelayedCall(6f, () =>
            {
                gameObject.transform.DOScale(0.1f, 5f).OnComplete(() =>
                {
                    ghost.ResetAngryTargetToPlayer();
                    ghost.ChangeState(Ghost_StateType.Normal);
                    Destroy(gameObject);
                });
            });
        });
    }
    
    /// <summary>
    /// 开启纸钱的增加进度方法
    /// </summary>
    public override void OpenIncreaseFillAmount()
    {
        base.OpenIncreaseFillAmount();
        if (holdTimer >= duration) // 进度完成，开启道具功能
        {
            tipText.text = "按G使用";
            isOpen = true; holdTimer = 0;
            UIManager.Instance.closePanel<HorProgressPanel>();
        }
    }
    
}
