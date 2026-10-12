using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
// 游戏失败面板
public class FailGamePanel: BasePanel
{
    public Button backBtn;
    private Image backgroundImage;

    void OnEnable()
    {
        backgroundImage = GameObject.Find("background").GetComponent<Image>();
        backBtn.onClick.AddListener(() =>
        {
            GameObject.FindGameObjectWithTag("Ghost").GetComponent<GhostController>().ChangeState(Ghost_StateType.Disappear);
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick,transform.position);
            backgroundImage.gameObject.SetActive(true);
            backgroundImage.GetComponent<FaderBackGround>().StartFade();
            UIManager.Instance.closePanel<FailGamePanel>();
            DOVirtual.DelayedCall(3.5f, () =>
            {
                SceneMgr.Instance.LoadSceneAsync("StartScene", () =>
                {
                    GameManager.Instance.SaveEvidenceList.Clear();
                    UIManager.Instance.closePanel<LoadPanel>();
                    UIManager.Instance.closePanel<FailGamePanel>();
                    UIManager.Instance.openPanel<StartPanel>();
                    SoundManager.Instance.audioSource.enabled = true; // 打开背景音乐
                    SoundManager.Instance.audioSource.Play();
                    EventCenter.Instance.EventTrigger(GameEvent.光标出现);
                });
            });
        });
    }

    void OnDisable()
    {
        backBtn.onClick.RemoveAllListeners();
    }
}
