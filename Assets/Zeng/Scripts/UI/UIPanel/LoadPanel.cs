using UnityEngine;
using UnityEngine.UI;

public class LoadPanel : BasePanel
{
    public Text tipText;
    private int currentTipId;
    void OnEnable()
    {
        currentTipId = Random.Range(1,GameManager.Instance.LoadTipsDataDict.Count + 1);
        tipText.text = GameManager.Instance.LoadTipsDataDict[currentTipId].Tip;
    }
}
