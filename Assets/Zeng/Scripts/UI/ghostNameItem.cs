using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ghostNameItem : MonoBehaviour
{
    public Text ghostName;
    public Image lineImage;
    public GhostData currentGhostData;
    // 获取当前选中证据的 id 列表
    
    void Start()
    {
        lineImage.gameObject.SetActive(false);
    }
    
    void Update()
    {
        UpdateLineImage();
    }
    
    
    public void UpdateLineImage()
    {
        if (currentGhostData != null && GameManager.Instance.SaveEvidenceList != null) 
        {
            //if (GameManager.Instance.SaveEvidenceList)
            // 检查当前选中的证据是否包含这个鬼的所有必需证据
            bool hasAllRequiredEvidence = CheckIfHasAllEvidence();
            
            // 如果没有所有必需证据，就划掉这个鬼名
            lineImage.gameObject.SetActive(!hasAllRequiredEvidence);
        }
    }
    
    /// <summary>
    /// 检查当前选中的证据是否包含这个鬼的所有必需证据
    /// </summary>
    private bool CheckIfHasAllEvidence()
    {
        if (currentGhostData == null || currentGhostData.evidenceList == null)
            return false;
            
        if (GameManager.Instance.SaveEvidenceList == null || GameManager.Instance.SaveEvidenceList.Count == 0)
            return true; // 如果没选任何证据，认为是所有鬼的子集（都不划掉）
        
        // 获取当前选中证据的 id 列表
        List<int> selectedEvidenceIds = new ();
        foreach (EvidenceData evidence in GameManager.Instance.SaveEvidenceList)
        {
            selectedEvidenceIds.Add(evidence.id);
        }
        
        // 检查每个选中的证据是否都在鬼要求的证据列表中
        foreach (int selectedId in selectedEvidenceIds)
        {
            if (!currentGhostData.evidenceList.Contains(selectedId))
            {
                // 只要有一个选中的证据不在鬼要求的列表中，就不是子集
                return false;
            }
        }
        // 所有选中的证据都在鬼要求的列表中（是子集）
        return true;
    }
    
    public void UpdateGhostData(GhostData data)
    {
        currentGhostData = data;
        ghostName.text = data.name;
        UpdateLineImage();
    }
}
