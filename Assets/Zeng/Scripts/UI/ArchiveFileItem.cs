using System;
using System.IO;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ArchiveFileItem : MonoBehaviour
{
    private Button fileBtn;
    [Header("存档名字")]
    public Text archiveName;
    [Header("年月日")]
    public Text dateText;
    public Button deleteBtn;
    
    // 存档
    private MainSaveData mainSaveData;
    public string archiveFullPath;    // 记录当前存档项的完整文件路径  比如 Archive/EveryGame/123/main.json
    private string archiveFolderPath; // 存储存档文件夹路径（而非文件路径） 比如 Archive/EveryGame/123/
    
    void Start()
    {
        fileBtn = GetComponent<Button>();
        fileBtn.onClick.AddListener(LoadFileGame);
        deleteBtn.onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
            ConfirmDeleteArchivePanel confirmDeleteArchivePanel = UIManager.Instance.openPanel<ConfirmDeleteArchivePanel>();
            confirmDeleteArchivePanel.archiveFolderPath = archiveFolderPath;
            confirmDeleteArchivePanel.currentMainSaveData = mainSaveData;
            confirmDeleteArchivePanel.panel.transform.localPosition = new Vector2(0, 700);
            confirmDeleteArchivePanel.panel.transform.DOLocalMove(new Vector2(0, 0), 0.5f);
            confirmDeleteArchivePanel.tipText.text = $"确定要删除存档「{mainSaveData.name}」吗？删除后无法恢复！";
        });
    }
    
    /// <summary>
    /// 加载数据
    /// </summary>
    private void LoadFileGame()
    {
        SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
        UIManager.Instance.closePanel<LoadFilePanel>();
        // 先赋值存档数据
        GameManager.Instance.mainSaveData = mainSaveData;
        GameManager.Instance.currentArchivePath = archiveFullPath;
        // 强制重新初始化所有受存档影响的数据
        GameManager.Instance.GameDataInit();
        // 重新初始化按键绑定和音量大小（读档后生效）
        KeyBindingManager.Instance.InitDefaultBindings();
        SoundAudioPool.Instance.GlobalSoundVolume = GameManager.Instance.mainSaveData.GlobalSoundVolumeData;
        UIManager.Instance.openPanel<StartPanel>();
        
        // 验证数据
        Debug.Log("读档后金币：" + GameManager.Instance.Coin);
        Debug.Log("读档后道具7数量：" + GameManager.Instance.ItemHasNumDict[7]);
        Debug.Log("读档后按键跳跃绑定：" + KeyBindingManager.Instance.GetBindedKey(GameAction.Jump));
        Debug.Log("读档后音量：" + SoundAudioPool.Instance.GlobalSoundVolume);
        Debug.Log("解锁鬼3情况：" + GameManager.Instance.GhostLockDict[3]);
    }


    /// <summary>
    /// 更新文件信息  
    /// </summary>
    /// <param name="fullPath">完整存储路径</param>
    /// <param name="data">当前的存档对应的信息</param>
    public void UpdateData(MainSaveData data, string fullPath)
    {
        mainSaveData = data;
        archiveFullPath = fullPath; // 提取文件路径（有 main.json）
        archiveFolderPath = Path.GetDirectoryName(fullPath); // 提取文件夹路径（去掉 main.json）
        archiveName.text = data.name;
        // 转换为本地时间（和电脑时间一致）
        DateTimeOffset saveTime = DateTimeOffset.FromUnixTimeSeconds(data.saveRealTicks);
        dateText.text = $"{saveTime.ToLocalTime():yyyy/MM/dd/HH:mm}"; // 显示年月时分秒
    }
}
