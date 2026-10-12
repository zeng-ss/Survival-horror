using UnityEngine;
using UnityEngine.UI;

public class EvidenceItem : MonoBehaviour
{
    // 证据
    public Text evidenceDes;
    // 打勾图标
    public Image tickImage;
    public Button tickBtn;
    
    public EvidenceData currentEvidence;         // 当前的证据
    public EvidenceData currentSaveEvidenceItem; // 当前保存的证据
    // 新增：获取CheckGhostPanel引用（面板打开时赋值）
    private CheckGhostPanel checkGhostPanel;
    

    void Start()
    {
        tickImage.gameObject.SetActive(false);
        tickBtn.onClick.AddListener(OnTick);
        // 自动找父面板
        checkGhostPanel = GetComponentInParent<CheckGhostPanel>();
    }


    
    /// <summary>
    ///  打勾方法
    /// </summary>
    private void OnTick()
    {
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
        // 如果已经保存过（打过 √）
        if (GameManager.Instance.SaveEvidenceList.Contains(currentSaveEvidenceItem))
        {
            tickImage.gameObject.SetActive(false);
            GameManager.Instance.SaveEvidenceList.Remove(currentSaveEvidenceItem);
        }
        else // 第一次保存（打勾）
        {
            tickImage.gameObject.SetActive(true);
            currentSaveEvidenceItem = currentEvidence;
            GameManager.Instance.SaveEvidenceList.Add(currentSaveEvidenceItem);
        }
        // 新增：勾选证据后，强制刷新所有鬼名的划线状态
        RefreshAllGhostNameLine();
    }
    
    /// <summary>
    /// 刷新所有鬼名的划线状态
    /// </summary>
    private void RefreshAllGhostNameLine()
    {
        if (checkGhostPanel == null) return;

        foreach (Transform child in checkGhostPanel.content2.transform)
        {
            ghostNameItem ghostItem = child.GetComponent<ghostNameItem>();
            if (ghostItem != null)
            {
                ghostItem.UpdateLineImage();
            }
        }
    }

    public void UpdateEvidenceData(EvidenceData evidenceData)
    {
        currentEvidence = evidenceData;
        evidenceDes.text = evidenceData.evidence;
    }
}
