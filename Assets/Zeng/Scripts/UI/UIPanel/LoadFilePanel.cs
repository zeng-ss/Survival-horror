using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LoadFilePanel : BasePanel
{
    public GameObject content;
    public Button closeButton;
    void OnEnable()
    {
        UpdateData();
        closeButton.onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
            UIManager.Instance.openPanel<StartPanel>();
            UIManager.Instance.closePanel<LoadFilePanel>();
        });
    }

    
    public void UpdateData()
    {
        for (int i = 0; i < content.transform.childCount; i++) { Destroy(content.transform.GetChild(i).gameObject); }
        //克隆新的  拿到 archive 文件夹下面的所有子文件夹的名字  
        string parentPath = Application.persistentDataPath + "/Archive/EveryGame";
        if (!Directory.Exists(parentPath)) return;
        string[] allPath = Directory.GetDirectories(parentPath);
        foreach (var path in allPath)
        {
            UpdateNewFileItem(path);
        }   
    }
    
    /// <summary>
    /// 更新存档项
    /// </summary>
    /// <param name="path"></param>
    public void UpdateNewFileItem(string path)
    {
        string fullJsonPath = Path.Combine(path, "main.json"); // 完整的存档文件路径
        MainSaveData data = JsonMgr.Instance.LoadData<MainSaveData>("main", path);
        GameObject obj = ResMgr.Instance.load<GameObject>("UI/ArchiveFileItem",content.transform);
        obj.GetComponent<ArchiveFileItem>().UpdateData(data, fullJsonPath);
    }

    void OnDisable() { closeButton.onClick.RemoveAllListeners(); }
    
}
