using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 黑板脚本，用来生成选择的道具 
/// </summary>
public class BlackHoldController : MonoBehaviour
{
    void Start() {StartCoroutine(Init()); }

    private IEnumerator Init()
    {
        yield return new WaitForSeconds(0.2f);
        // 当前的道具的下标
        int currentIndex = 0; 
        foreach (var selectItem in GameManager.Instance.ItemSelectNumDict)
        {
            int id = selectItem.Key;
            int itemNum = selectItem.Value;
            // 如果选择了道具（数量大于 0）
            if (itemNum > 0)
            {
                for (int i = 0; i < itemNum; i++)
                {
                    GameObject itemObj = ResMgr.Instance.load<GameObject>
                        ($"Model/itemModel/{GameManager.Instance.ItemDataDict[id].model}");
                    if (itemObj == null) Debug.LogError("itemObj is null");
            
                    // 设置大小
                    itemObj.GetComponent<Rigidbody>().isKinematic = true;
                    itemObj.GetComponent<Rigidbody>().useGravity = false;
                    itemObj.transform.localRotation = Quaternion.identity; // 重置旋转
                    itemObj.transform.SetParent(transform);
            
                    // 计算位置
                    int row = currentIndex / 5; // 行数：0=第一行，1=第二行，2=第三行...
                    int col = currentIndex % 5; // 列数：0=第一列，1=第二列...
                    float xPos = -2f - col * 2;
                    float yPos = 10.64f - row * 3;
                    itemObj.transform.localPosition = new Vector3(xPos, yPos, -1.59f);
                    currentIndex++;
                }
                
            }
            
        }
        // 清除选中的道具数量
        ClearSelectDict();
    }

    private void ClearSelectDict()
    {
        // 更新 1-9号道具的 ItemHasNumDict 
        for (int itemId = 1; itemId <= 9; itemId++)
        {
            // 确保字典中存在该 ID（避免KeyNotFoundException）
            if (GameManager.Instance.ItemHasNumDict.ContainsKey(itemId) && 
                GameManager.Instance.ItemSelectNumDict.ContainsKey(itemId))
            {
                // 清空选中数量（保存后重置选择）
                GameManager.Instance.ItemSelectNumDict[itemId] = 0;
            }
        }
        GameManager.Instance.ItemHasNumDict[1] = 1;
        GameManager.Instance.ItemHasNumDict[2] = 1;
    }
}
