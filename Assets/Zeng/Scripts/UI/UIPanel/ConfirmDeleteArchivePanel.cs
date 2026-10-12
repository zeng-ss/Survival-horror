using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmDeleteArchivePanel : BasePanel
{
    public Text tipText;
    public Button confirmButton;
    public Button cancelButton;
    public GameObject panel;
    public string archiveFolderPath;    // 记录当前存档项的完整路径
    public MainSaveData currentMainSaveData;
    void OnEnable()
    {
        confirmButton.onClick.AddListener(DeleteArchive);
        cancelButton.onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
            UIManager.Instance.closePanel<ConfirmDeleteArchivePanel>();
        });
    }

    void OnDisable()
    {
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// 删除存档
    /// </summary>
    private void DeleteArchive()
    {
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
        try
        {
            // 基础校验：路径为空
            if (string.IsNullOrEmpty(archiveFolderPath)) { throw new Exception("存档文件夹路径为空！"); }

            // 拼接 Unity 持久化路径（确保路径绝对正确）
            string fullAbsFolderPath = Path.Combine(Application.persistentDataPath, archiveFolderPath);
            //Debug.Log("最终要删除的文件夹路径：" + fullAbsFolderPath);

            // 检查文件夹是否存在
            if (Directory.Exists(fullAbsFolderPath))
            {
                // 递归删除文件夹及所有子文件
                Directory.Delete(fullAbsFolderPath, true); 
                //Debug.Log("存档文件夹已删除：" + fullAbsFolderPath);
            }
            else
            {
                // 检查是否是文件路径（防止传错）
                if (File.Exists(fullAbsFolderPath))
                {
                    throw new Exception("路径是文件而非文件夹：" + fullAbsFolderPath);
                }
                else throw new Exception("存档文件夹不存在：" + fullAbsFolderPath);
            }
            UIManager.Instance.closePanel<ConfirmDeleteArchivePanel>();
            // 刷新存档列表
            LoadFilePanel loadPanel = UIManager.Instance.getPanel<LoadFilePanel>();
            
            if (loadPanel != null) { loadPanel.UpdateData(); }
            else { Debug.LogWarning("LoadFilePanel未打开，跳过刷新"); }

            // 提示删除成功（防止 currentMainSaveData 为空）
            TipPanel tipPanel = UIManager.Instance.openPanel<TipPanel>();
            tipPanel.transform.localPosition = new Vector2(0, 428);
            string tipMsg = currentMainSaveData != null 
                ? $"存档「{currentMainSaveData.name}」已删除" 
                : "存档删除成功";
            tipPanel.ShowTip(tipMsg);
        }
        catch (Exception e)
        {
            Debug.LogError("删除存档失败：" + e.Message + "\n堆栈：" + e.StackTrace);
            // 提示删除失败
            TipPanel tipPanel = UIManager.Instance.openPanel<TipPanel>();
            tipPanel.transform.localPosition = new Vector2(0, 428);
            tipPanel.ShowTip("删除失败：" + e.Message);
        }
    
    }
}
