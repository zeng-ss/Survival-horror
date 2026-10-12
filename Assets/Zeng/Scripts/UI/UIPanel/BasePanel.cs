using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class BasePanel : MonoBehaviour
{
    private RectTransform rootRect;
    protected IEnumerator DelayedLayoutUpdate()
    {
        yield return null; // 等待一帧
        LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);
    }
    //淡入淡出速度 
    private bool isShow;
    private UnityAction hideAction;
    public virtual void Awake()
    {
        rootRect = this.transform as RectTransform;
        // canvasGroup = GetComponent<CanvasGroup>();
        // if (canvasGroup == null) canvasGroup= this.gameObject.AddComponent<CanvasGroup>();
    }

    public virtual void Show()//虚函数 能够被重写  
    {
        this.gameObject.SetActive(true);
    }


    public virtual void Hide()
    {
        this.gameObject.SetActive(false);
    }

    private void Update()
    {
        // //淡入
        // if (isShow&&canvasGroup.alpha!=1)
        // {
        //     canvasGroup.alpha += alphaSpeed*Time.deltaTime;
        //     if (canvasGroup.alpha>=1)
        //     {
        //         canvasGroup.alpha = 1;
        //     }
        // }
        // else//淡出
        // {
        //     canvasGroup.alpha -= alphaSpeed*Time.deltaTime;
        //         print(canvasGroup.alpha);
        //     if (canvasGroup.alpha<=0)
        //     {
        //         canvasGroup.alpha = 0;
        //         //删除面板  
        //         hideAction?.Invoke();
        //     }
        // }
    }
}
