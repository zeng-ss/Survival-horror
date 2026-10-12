using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 鬼的详细资料面板
/// </summary>
public class GhostDetailPanel : BasePanel
{
    // 容器，陆地
    public GameObject content;
    private GameObject modelContainer;
    
    // 按钮，文字
    public Text detailText;
    public Button backBtn;
    
    // 模型和模型旋转相关
    public RawImage modelRawImage;
    private ModelRotate modelRotate;
    private List<GameObject> modelList = new ();
    // 鬼数据字典
    private Dictionary<GameObject, GhostData> ghostDic = new ();
    
    void OnEnable()
    {
        modelRotate = modelRawImage.GetComponent<ModelRotate>();
        // 创建模型容器
        if (modelContainer == null)
        {
            modelContainer = new GameObject("ModelContainer")
            {
                transform = { position = new Vector3(1000, 1000, 1000) }
            };
            GameObject l = ResMgr.Instance.load<GameObject>("Model/effects/Light",modelContainer.transform);
            l.GetComponent<Light>().intensity = 5;
            l.transform.localPosition = new Vector3(0, 5, 0);
        }
        ClearGhostDetails();
        backBtn.onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
            UIManager.Instance.openPanel<StartPanel>();
            UIManager.Instance.closePanel<GhostDetailPanel>();
            ClearGhostDetails();
        });
        
        // 先清除鬼头像图标
        for (int i = 0; i < content.transform.childCount; i++) { Destroy(content.transform.GetChild(i).gameObject); }
        // 创建鬼头像图标
        foreach (GhostData ghostData in GameManager.Instance.GhostDataDict.Values) { CreateIcon(ghostData); }
    }
    
    private void CreateIcon(GhostData ghostData)
    {
        GameObject iconGameObject = new GameObject("GhostIcon");
        iconGameObject.AddComponent<Button>();
        iconGameObject.transform.SetParent(content.transform);
        Sprite sprite = ResMgr.Instance.load<Sprite>($"Image/{ghostData.icon}");
        iconGameObject.AddComponent<Image>().sprite = sprite;
        ghostDic.Add(iconGameObject, ghostData);
        
        // 按下头像更新鬼的详细信息
        iconGameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundManager.Instance.baseBtnClick, transform.position);
            UpdateGhostDetail(ghostData);
        });
    }

    private void UpdateGhostDetail(GhostData ghostData)
    {
        detailText.text = FormatGhostDetails(ghostData); // 更新文字信息
        // 先清除原来的模型
        if (modelList.Count > 0)
        {
            foreach (GameObject model in modelList) { Destroy(model); }
            modelList.Clear();
            modelRotate.ResetRotation();
        }
        // 还未解锁就创建未解锁文字
        if (GameManager.Instance.GhostLockDict[ghostData.id])
        {
            // 创建 GameObject
            GameObject textObject = new GameObject("LockTip");
            textObject.transform.SetParent(modelContainer.transform);
            textObject.transform.localPosition = new Vector3(0, 1, -12);
            textObject.transform.localRotation = Quaternion.Euler(0, 180, 0);
            // 将旋转对象设置为文字
            modelRotate.targetModel = null;
    
            // 添加 TextMeshPro 组件（这是3D文本）
            TextMeshPro textMesh = textObject.AddComponent<TextMeshPro>();
            textMesh.fontSize = 18;
            textMesh.text = "还未解锁鬼的模型";
            textMesh.color = Color.red;
            textMesh.alignment = TextAlignmentOptions.Center;
            TMP_FontAsset fontAsset = ResMgr.Instance.load<TMP_FontAsset>("Material&Animation&Fonts/字体管家棉花糖 SDF");
            textMesh.font = fontAsset;
            modelList.Add(textObject);
            return;
        }
        // 已经解锁
        // 设置 RawImage的旋转控制
        modelRotate.targetModel = modelContainer;
        // 克隆鬼模型
        GameObject ghostModel = ResMgr.Instance.load<GameObject>($"Model/ghostModel 1/{ghostData.model}");
        ghostModel.transform.SetParent(modelContainer.transform);
        ghostModel.transform.localPosition = Vector3.zero; // 相对于容器的位置
        ghostModel.transform.localRotation = Quaternion.identity;
        ghostModel.transform.localScale *= 1.5f;
        modelList.Add(ghostModel);
    }
    
    /// <summary>
    /// 格式化鬼的详细信息
    /// </summary>
    /// <param name="ghostData"></param>
    /// <returns></returns>
    private string FormatGhostDetails(GhostData ghostData)
    {
        // 根据你的GhostData结构来格式化显示信息
        string details = "";
        if (GameManager.Instance.GhostLockDict[ghostData.id])
        {
            details += "<b>还未解锁证据！</b>";
            return details;
        }
        
        // 示例格式
        details += $"<b>名称:</b> {ghostData.name}\n\n";
        details += $"<b>描述:</b> {ghostData.des}\n\n";
        // 如果有证据列表
        if (ghostData.evidenceList != null && ghostData.evidenceList.Count > 0)
        {
            details += "<b>证据:</b>\n";
            foreach (int evidenceId in ghostData.evidenceList)
            {
                // 根据evidenceId获取证据名称
                if (GameManager.Instance.EvidenceDataDict.ContainsKey(evidenceId))
                {
                    string evidenceDes = GameManager.Instance.EvidenceDataDict[evidenceId].evidence;
                    details += $"• {evidenceDes}\n";
                }
            }
        }
        return details;
    }
    
    /// <summary>
    /// 清理资源
    /// </summary>
    private void ClearGhostDetails()
    {
        backBtn.onClick.RemoveAllListeners();
        if (modelList.Count > 0)
        {
            foreach (GameObject model in modelList)
            {
                if (model != null) Destroy(model);
            }
            modelList.Clear();
        }
        detailText.text = "";
        modelRotate.ResetRotation();
    }
    private void OnDisable() { ClearGhostDetails(); }
}
