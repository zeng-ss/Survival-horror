using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public class QuickSearch : MonoBehaviour
{
    [MenuItem("Tools/一键查找所有GameManager")]
    static void FindAll()
    {
        var allManagers = FindObjectsOfType<SoundManager>(true);
        Debug.Log($"找到 {allManagers.Length} 个GameManager实例:");
        
        foreach (var manager in allManagers)
        {
            string path = manager.name;
            Transform parent = manager.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            Debug.Log($"📍 {path}", manager.gameObject);
        }
        
        if (allManagers.Length > 0)
            Selection.activeGameObject = allManagers[0].gameObject;
    }
}
#endif
