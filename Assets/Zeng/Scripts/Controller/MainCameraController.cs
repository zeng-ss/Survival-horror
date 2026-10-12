using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class MainCameraController : MonoBehaviour
{
    // 位置偏移
    private Vector3 originalPosition; // 存储原始位置
    private Quaternion originalRotation; // 存储原始旋转
    private Vector3 targetPosition; // 存储 1 目标位置
    private Quaternion targetRotation; // 存储 1 目标旋转
    private Vector3 targetPosition2; // 存储 2 目标位置
    private Quaternion targetRotation2; // 存储 2 目标旋转

    // TimeLine 和相机
    private PlayableDirector timeLinePlayer; // TimeLine导演
    private GameObject cinematicCamera; // 自由相机
    private GameObject FeelLookCamera; // 自由相机

    // 过度面板效果
    private FaderBackGround postProcessFader;
    private Image backImage;
    private float timer; // 循环播放 Idle 动画计时器
    private bool isStartToPerson; // 判断相机是否开始了第二段移动

    private Tween delayedBackGround;

    void OnEnable()
    {
        timeLinePlayer = GameObject.Find("TimeLine").GetComponent<PlayableDirector>();
        backImage = GameObject.Find("background").GetComponent<Image>();
        backImage.enabled = false;
        postProcessFader = backImage.GetComponent<FaderBackGround>();

        timeLinePlayer.Play(); // 默认打开
        cinematicCamera = GameObject.Find("feelLookCamera");
        FeelLookCamera = GameObject.Find("FreeLook Camera");
        cinematicCamera.gameObject.SetActive(false);

        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;

        targetPosition = new Vector3(1.56f, 0.647f, 2.843f);
        targetRotation = Quaternion.Euler(16.664f, -84.017f, 20.855f);

        targetPosition2 = new Vector3(-0.667f, 1.345354f, 4.243112f);
        targetRotation2 = Quaternion.Euler(25.186f, 114.774f, -18.817f);
    }

    void Update()
    {
        if (isStartToPerson) return;
        timer += Time.deltaTime;
        if (timer >= 4.92f)
        {
            timeLinePlayer.Stop();
            timeLinePlayer.Play();
            timer = 0;
        }
    }

    /// <summary>
    /// 移动到目标1位置
    /// </summary>
    public void MoveToLookAtPanel()
    {
        // 直接使用 DOMove 和 DORotate 确保准确回到初始状态
        Sequence returnSeq = DOTween.Sequence();
        returnSeq.Append(transform.DOLocalMove(targetPosition, 2f))
            .Join(transform.DOLocalRotate(targetRotation.eulerAngles, 2f))
            .OnComplete(() =>
            {
                ChooseItemPanel chooseItemPanel = UIManager.Instance.openPanel<ChooseItemPanel>();
                chooseItemPanel.UpdateItemItem();
                chooseItemPanel.transform.localPosition = new Vector2(44, 52);
                UIManager.Instance.openPanel<LookSelectPanel>().transform.position = new Vector2(0, -519);
            });
    }

    /// <summary>
    /// 移动到原始位置
    /// </summary>
    public void MoveBackToStart()
    {
        // 直接使用 DOMove 和 DORotate 确保准确回到初始状态
        Sequence returnSeq = DOTween.Sequence();
        returnSeq.Append(transform.DOLocalMove(originalPosition, 2.5f))
            .Join(transform.DOLocalRotate(originalRotation.eulerAngles, 2.5f))
            .OnComplete(() => { UIManager.Instance.openPanel<StartPanel>(); });
    }

    /// <summary>
    /// 移动到目标 2 位置
    /// </summary>
    public void MoveToLookAtPerson()
    {
        // 先停止 TimeLine
        isStartToPerson = true;
        timeLinePlayer.Stop();
        GameObject player = GameObject.Find("PlayerArmature");
        // 旋转角色
        player.transform.rotation = Quaternion.Euler(0, -110, 0);
        Sequence seq = DOTween.Sequence();
        // 直接使用 DOMove 和 DORotate 到第二个地点
        seq.Append(transform.DOLocalMove(targetPosition2, 2f))
            .Join(transform.DOLocalRotate(targetRotation2.eulerAngles, 2f))
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                cinematicCamera.gameObject.SetActive(true); // 开启自由相机
                // 防止 CinemachineFreeLook 鼠标移动
                CinemachineFreeLook freeLook = FeelLookCamera.GetComponent<CinemachineFreeLook>();
                freeLook.m_XAxis.m_InputAxisName = ""; // 清空水平轴输入
                freeLook.m_YAxis.m_InputAxisName = ""; // 清空垂直轴输入

                timeLinePlayer.time = 4.93f; // TimeLine 从行走片段开始的
                timeLinePlayer.Play();
                DOVirtual.DelayedCall(3.3f, () =>
                {
                    // 3.3s之后打开黑屏动画
                    postProcessFader.isNeedCreatText = true; // 需要打开创建文字限制
                    postProcessFader.StartFade();
                    backImage.enabled = true;
                    delayedBackGround = DOVirtual.DelayedCall(15.5f, () =>
                    {
                        SoundManager.Instance.audioSource.enabled = false; //关闭背景音乐
                        // 15.5s之后跳转场景
                        SceneMgr.Instance.LoadSceneAsync("FactoryNight",
                            () => { UIManager.Instance.closePanel<LoadPanel>(); });
                    });
                });
            });
    }

    public void KillDelayCall()
    {
        if (delayedBackGround != null) delayedBackGround.Kill();
    }

}
