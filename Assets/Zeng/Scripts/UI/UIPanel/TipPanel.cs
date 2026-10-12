using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TipPanel : BasePanel
{
    public Text tipText;
    public CanvasGroup canvasGroup;
    public float showTime = 1f;
    private float fadeDuration = 0.6f;
    private RectTransform rectTransform;

    private void OnEnable()
    {
        if (showTime >= 3) { showTime = 1; }
    }
    
    public override void Awake()
    {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void ShowTip(string content)
    {
        tipText.text = content;
        canvasGroup.alpha = 0f;
        
        // 动画序列
        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(1f, fadeDuration))
            .AppendInterval(showTime)
            .Append(canvasGroup.DOFade(0f, fadeDuration))
            .OnComplete(() => UIManager.Instance.closePanel<TipPanel>());
    }
}
