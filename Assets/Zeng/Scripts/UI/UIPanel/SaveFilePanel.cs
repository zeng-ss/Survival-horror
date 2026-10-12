using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SaveFilePanel : BasePanel
{
    [Header("提示")] 
    public Text tipText;
    [Header("保存存档按钮")]
    public Button saveFileBtn;
    [Header("取消存档按钮")]
    public Button cancelBtn;    
    [Header("文档名")]
    public TMP_InputField saveFileInput;
    public GameObject panel;
    private string fillName = "默认存档";
    void OnEnable()
    {
        if (GameManager.Instance.currentArchivePath != null && File.Exists(GameManager.Instance.currentArchivePath))
        {
            tipText.text = "输入此存档的名称:";
        }
        else
        {
            tipText.text = "输入新存档的名称:";
        }
        panel.transform.localPosition = new Vector2(0f, 672);
        panel.transform.DOLocalMove(Vector2.zero, 0.5f);
        saveFileBtn.onClick.AddListener(SaveFile);
        cancelBtn.onClick.AddListener(Cancel);
    }
    
    private void OnDisable()
    {
        saveFileBtn.onClick.RemoveListener(SaveFile);
        cancelBtn.onClick.RemoveListener(Cancel);
    }

    /// <summary>
    /// 保存文档
    /// </summary>
    private void SaveFile()
    {
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
        string inputText = saveFileInput.text.Trim();
        // 简化名称判断逻辑（优先级：输入框 > 原有存档名称 > 默认名称）
        if (!string.IsNullOrEmpty(inputText))
        {
            // 输入框有内容 → 使用输入的名称
            fillName = inputText;
        }
        else if (GameManager.Instance.mainSaveData != null)
        {
            // 输入框为空，但有原有存档 → 使用原有名称
            fillName = GameManager.Instance.mainSaveData.name;
        }
        // 过滤存档名称非法字符（避免路径错误）
        fillName = System.Text.RegularExpressions.Regex.Replace(fillName, @"[\\/:*?""<>|]", "");
        // 限制名称长度（最多 20 字符）
        fillName = fillName.Length > 20 ? fillName.Substring(0, 20) : fillName;
        try
        {
            // 调用 GameManager 存档方法  
            GameManager.Instance.SaveArchive(fillName);
            UIManager.Instance.closePanel<SaveFilePanel>();
        
            // 退出游戏
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;    
#else
            Application.Quit();
#endif
        }
        catch (Exception e) { Debug.LogError($"存档失败：{e.Message}\n{e.StackTrace}"); }
    }

    private void Cancel()
    {
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;    
#else
                Application.Quit();
#endif
    }
    
}
