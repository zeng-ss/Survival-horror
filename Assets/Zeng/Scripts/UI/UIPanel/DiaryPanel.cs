using UnityEngine.UI;

public class DiaryPanel : BasePanel
{
    public Text diaryText;

    void OnEnable()
    {
        int id = GameManager.Instance.currentChooseGhostId;
        diaryText.text = 
            $"{GameManager.Instance.DiaryDataDict[id].diaryContent}<b><color=red>\n现在抓鬼区域已经出现，请前往抓鬼</color></b>";
    }
}
