using Microlight.MicroBar;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProPanel : BasePanel
{
    [Header("准心的Image")] public Image crosshairImage;
    [Header("拾起的Image")] public Image pickUpImage;
    
    [Header("角色的理智条")] public MicroBar sanitySlider;
    [Header("角色的理智条")] public MicroBar bodyPowerSlider;

    void OnEnable()
    {
        sanitySlider.Initialize(100);
        bodyPowerSlider.Initialize(100);
        // 禁用CanvasRenderer的射线检测
        Image[] images = GetComponentsInChildren<Image>();
        foreach (Image image in images)
        {
            image.raycastTarget = false;
        }
        crosshairImage.gameObject.SetActive(true);
        pickUpImage.gameObject.SetActive(false);
        
        EventCenter.Instance.AddEventListener<float>(GameEvent.理智值变化,SanitySliderChanged);
        EventCenter.Instance.AddEventListener<float>(GameEvent.体力值变化,BodyPowerSliderChanged);
    }
    
    private void SanitySliderChanged(float currentSanity) { sanitySlider.UpdateBar(currentSanity); }
    private void BodyPowerSliderChanged(float currentBodyPower) { bodyPowerSlider.UpdateBar(currentBodyPower); }
    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener<float>(GameEvent.理智值变化,SanitySliderChanged);
        EventCenter.Instance.RemoveEventListener<float>(GameEvent.体力值变化,BodyPowerSliderChanged);
    }
}
