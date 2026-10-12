using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
/// <summary>
/// 场景管理器  
/// </summary>
public class SceneMgr:UnitySingleTonMono<SceneMgr>
{
    /// <summary>
    /// 同步切换场景 
    /// </summary>
    /// <param name="sceneName"></param>
    public void LoadScene(string sceneName,UnityAction fun=null)
    {
        SceneManager.LoadScene(sceneName);
        fun?.Invoke();  
    }

    /// <summary>
    /// 异步加载场景的方法  
    /// </summary>  
    /// <param name="sceneName"></param>
    /// <param name="fun"></param>
    public void LoadSceneAsync(string sceneName, UnityAction fun = null)
    {
        StartCoroutine(LoadSceneEnumerator(sceneName,fun));
    }

    private IEnumerator LoadSceneEnumerator(string sceneName,UnityAction fun=null)
    {
        // 先打开加载面板
        UIManager.Instance.openPanel<LoadPanel>();
        // 等待一帧，确保加载面板已经显示
        yield return null;
    
        // 开始异步加载场景
        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);
        ao.allowSceneActivation = false; // 禁止自动切换场景
    
        float progress = 0;
        // 使用真实的加载进度
        while (!ao.isDone)
        {
            progress = ao.progress;
            // Unity的加载进度在0-0.9之间，到0.9时表示加载完成
            if (ao.progress >= 0.9f)
            {
                progress = 1.0f; // 显示100%
                break;
            }
        
            // 更新进度条
            EventCenter.Instance.EventTrigger(GameEvent.进度条加载, progress);
            yield return null;
        }
    
        // 确保进度条显示100%
        EventCenter.Instance.EventTrigger(GameEvent.进度条加载, 1.0f);
        // 等待一小段时间让玩家看到100%的进度
        yield return new WaitForSeconds(1f);
    
        // 允许场景激活
        ao.allowSceneActivation = true;
        // 等待场景真正切换完成
        yield return new WaitUntil(() => ao.isDone);
        
        // 执行回调
        fun?.Invoke();
    }
    
    
}
