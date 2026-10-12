using System.Collections.Generic;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UIManager.Instance.closePanel<PlayerProPanel>();
            List<int> itemLockId = new List<int>();
            foreach (var item in GameManager.Instance.ItemLockDict)
            {
                // 拿到所有 GameManager里面还没解锁的道具列表
                if (item.Value) { itemLockId.Add(item.Key); }
            }
            if (itemLockId.Count == 0)
            {
                GameObject.FindGameObjectWithTag("Ghost").GetComponent<GhostController>().ChangeState(Ghost_StateType.Disappear);
                SceneMgr.Instance.LoadSceneAsync("StartScene",() =>
                {
                    GameManager.Instance.SaveEvidenceList.Clear();
                    UIManager.Instance.closePanel<LoadPanel>();
                    UIManager.Instance.openPanel<StartPanel>().mainCamera = GameObject.Find("MainCamera").GetComponent<MainCameraController>();
                });
                return;
            }
            GameObject.FindGameObjectWithTag("Ghost").GetComponent<GhostController>().ChangeState(Ghost_StateType.Disappear);
            // 获取随机选中的道具 key
            int randomIndex = itemLockId[Random.Range(0, itemLockId.Count)];
            GameManager.Instance.GhostLockDict[GameManager.Instance.currentChooseGhostId] = false;
            GameManager.Instance.ItemLockDict[randomIndex] = false;
            SceneMgr.Instance.LoadSceneAsync("StartScene",() =>
            {
                GameManager.Instance.SaveEvidenceList.Clear();
                GameManager.Instance.isFirstPlayGame = false;
                UIManager.Instance.closePanel<LoadPanel>();
                UIManager.Instance.openPanel<WinGamePanel>()
                    .UpdateData
                    (GameManager.Instance.GhostDataDict
                        [GameManager.Instance.currentChooseGhostId]
                        ,GameManager.Instance.ItemDataDict[randomIndex]);
                EventCenter.Instance.EventTrigger(GameEvent.光标出现);
            });
        }
    }
}
