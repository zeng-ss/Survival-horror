using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EscPanel : BasePanel
{
    public Button audioBtn;
    public Button keyCodeBtn;
    public Button backBtn;

    void Start()
    {
        UIManager.Instance.openPanel<EscAudioPanel>();//默认打开音量设置
        audioBtn.onClick.AddListener(() =>
        {
            
            UIManager.Instance.closePanel<EscAudioPanel>();
            UIManager.Instance.openPanel<EscAudioPanel>();
        });
        keyCodeBtn.onClick.AddListener(() => { UIManager.Instance.openPanel<EscKeyPanel>();});
        backBtn.onClick.AddListener(() => { UIManager.Instance.closePanel<EscPanel>();});
    }

    private void OnDestroy()
    {
        audioBtn.onClick.RemoveAllListeners();
        keyCodeBtn.onClick.RemoveAllListeners();
        backBtn.onClick.RemoveAllListeners();
    }
}
