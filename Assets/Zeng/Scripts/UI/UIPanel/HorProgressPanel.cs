using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HorProgressPanel : BasePanel
{
    public Text tipText; 
    public Image proGressImage;
    [HideInInspector] public float currentFillAmount;

    public void IncreaseFillAmount(float duration,float holdTime)
    {
        currentFillAmount = Mathf.Clamp01(holdTime / duration);
        proGressImage.fillAmount = currentFillAmount;
    }

    public void DecreaseFillAmount(float duration, float holdTime)
    {
        holdTime = Mathf.Max(0, holdTime);
        currentFillAmount = Mathf.Clamp01(holdTime / duration);
        proGressImage.fillAmount = currentFillAmount;
    }
    

}
