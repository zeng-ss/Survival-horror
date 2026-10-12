using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GhostMusicState
{
    None,
    Normal,
    Patrol,
    Hunt,
    JumpScare
}

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    
    [Header("音乐播放器")]
    [SerializeField] private AudioSource musicSource;
    
    [Header("状态音乐 - 普通状态")]
    [SerializeField] private AudioClip[] normalMusicClips;
    
    [Header("状态音乐 - 巡逻状态")]
    [SerializeField] private AudioClip[] patrolMusicClips;
    
    [Header("状态音乐 - 猎杀状态")]
    [SerializeField] private AudioClip[] huntMusicClips;
    
    [Header("状态音乐 - 跳脸状态")]
    [SerializeField] private AudioClip[] jumpScareMusicClips;
    
    [Header("渐变设置")]
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float maxVolume = 0.8f;
    
    [Header("场景设置")]
    [SerializeField] private bool stopMusicOnSceneChange = true;
    [SerializeField] private string[] musicEnabledScenes = { "GameScene", "MainScene" }; // 在这些场景中允许播放音乐
    
    private GhostMusicState currentState = GhostMusicState.None;
    private Coroutine fadeCoroutine;
    private Dictionary<GhostMusicState, AudioClip> currentMusicMap = new Dictionary<GhostMusicState, AudioClip>();
    private string currentSceneName;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 初始化音乐播放器
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0f;
        }
        
        // 初始化当前音乐映射
        InitializeCurrentMusicMap();
        
        // 监听场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;
        currentSceneName = SceneManager.GetActiveScene().name;
    }
    
    private void OnDestroy()
    {
        // 取消监听场景加载事件
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    /// <summary>
    /// 场景加载完成时的回调
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string previousScene = currentSceneName;
        currentSceneName = scene.name;
        
        Debug.Log($"场景切换: {previousScene} -> {currentSceneName}");
        
        if (stopMusicOnSceneChange)
        {
            HandleSceneChangeMusic(previousScene, currentSceneName);
        }
    }
    
    /// <summary>
    /// 处理场景切换时的音乐逻辑
    /// </summary>
    private void HandleSceneChangeMusic(string previousScene, string newScene)
    {
        // 检查新场景是否在允许播放音乐的列表中
        bool newSceneAllowsMusic = IsSceneMusicEnabled(newScene);
        bool previousSceneAllowedMusic = IsSceneMusicEnabled(previousScene);
        
        if (!newSceneAllowsMusic)
        {
            // 新场景不允许播放音乐，停止音乐
            StopMusicImmediate();
            Debug.Log($"场景 {newScene} 不允许播放音乐，已停止所有音乐");
        }
        else if (!previousSceneAllowedMusic && newSceneAllowsMusic)
        {
            // 从不允许音乐的场景切换到允许音乐的场景，可以重新开始音乐
            Debug.Log($"切换到允许播放音乐的场景 {newScene}，可以手动重新开始音乐");
        }
        else
        {
            // 两个场景都允许音乐，可以选择是否保持音乐或重新开始
            Debug.Log($"场景切换，音乐状态保持: {currentState}");
            // 这里可以根据需要决定是否在场景切换时重新开始音乐
            // 如果需要重新开始，可以调用：ChangeMusicState(GhostMusicState.Normal);
        }
    }
    
    /// <summary>
    /// 检查指定场景是否允许播放音乐
    /// </summary>
    private bool IsSceneMusicEnabled(string sceneName)
    {
        if (musicEnabledScenes == null || musicEnabledScenes.Length == 0)
            return true; // 如果没有设置，默认所有场景都允许
        
        foreach (string enabledScene in musicEnabledScenes)
        {
            if (enabledScene == sceneName)
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// 初始化当前音乐映射
    /// </summary>
    private void InitializeCurrentMusicMap()
    {
        currentMusicMap[GhostMusicState.Normal] = GetRandomClip(normalMusicClips);
        currentMusicMap[GhostMusicState.Patrol] = GetRandomClip(patrolMusicClips);
        currentMusicMap[GhostMusicState.Hunt] = GetRandomClip(huntMusicClips);
        currentMusicMap[GhostMusicState.JumpScare] = GetRandomClip(jumpScareMusicClips);
    }
    
    /// <summary>
    /// 从数组中随机选择一个音乐剪辑
    /// </summary>
    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }
    
    /// <summary>
    /// 改变鬼魂音乐状态
    /// </summary>
    public void ChangeMusicState(GhostMusicState newState, bool forceChange = false)
    {
        // 检查当前场景是否允许播放音乐
        if (!IsSceneMusicEnabled(currentSceneName))
        {
            Debug.LogWarning($"当前场景 {currentSceneName} 不允许播放音乐");
            return;
        }
        
        if (currentState == newState && !forceChange) return;
        
        Debug.Log($"音乐状态切换: {currentState} -> {newState}");
        currentState = newState;
        
        // 停止当前的渐变协程
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        // 开始新的音乐渐变
        fadeCoroutine = StartCoroutine(FadeToNewState(newState));
    }
    
    /// <summary>
    /// 强制重新随机选择当前状态的音乐
    /// </summary>
    public void ShuffleCurrentStateMusic()
    {
        if (currentState == GhostMusicState.None) return;
        
        AudioClip newClip = GetRandomClipForState(currentState);
        if (newClip != null)
        {
            currentMusicMap[currentState] = newClip;
            
            // 如果当前正在播放这个状态的音乐，立即切换
            if (musicSource.isPlaying && GetMusicStateForClip(musicSource.clip) == currentState)
            {
                ChangeMusicState(currentState, true);
            }
        }
    }
    
    /// <summary>
    /// 渐变到新的音乐状态
    /// </summary>
    private IEnumerator FadeToNewState(GhostMusicState newState)
    {
        // 先淡出当前音乐
        yield return StartCoroutine(FadeVolume(0f, fadeDuration / 2f));
        
        // 获取新状态的随机音乐
        AudioClip newClip = GetMusicClipForState(newState);
        if (newClip != null)
        {
            musicSource.clip = newClip;
            musicSource.Play();
        }
        
        // 淡入新音乐
        yield return StartCoroutine(FadeVolume(maxVolume, fadeDuration / 2f));
    }
    
    /// <summary>
    /// 音量渐变
    /// </summary>
    private IEnumerator FadeVolume(float targetVolume, float duration)
    {
        float startVolume = musicSource.volume;
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, progress);
            yield return null;
        }
        
        musicSource.volume = targetVolume;
    }
    
    /// <summary>
    /// 根据状态获取对应的音乐剪辑（带缓存）
    /// </summary>
    private AudioClip GetMusicClipForState(GhostMusicState state)
    {
        if (currentMusicMap.ContainsKey(state) && currentMusicMap[state] != null)
        {
            return currentMusicMap[state];
        }
        
        // 如果缓存中没有，重新随机选择一个
        AudioClip newClip = GetRandomClipForState(state);
        currentMusicMap[state] = newClip;
        return newClip;
    }
    
    /// <summary>
    /// 为指定状态随机选择一个音乐剪辑
    /// </summary>
    private AudioClip GetRandomClipForState(GhostMusicState state)
    {
        switch (state)
        {
            case GhostMusicState.Normal:
                return GetRandomClip(normalMusicClips);
            case GhostMusicState.Patrol:
                return GetRandomClip(patrolMusicClips);
            case GhostMusicState.Hunt:
                return GetRandomClip(huntMusicClips);
            case GhostMusicState.JumpScare:
                return GetRandomClip(jumpScareMusicClips);
            default:
                return null;
        }
    }
    
    /// <summary>
    /// 根据音乐剪辑判断属于哪个状态
    /// </summary>
    private GhostMusicState GetMusicStateForClip(AudioClip clip)
    {
        if (clip == null) return GhostMusicState.None;
        
        if (ContainsClip(normalMusicClips, clip)) return GhostMusicState.Normal;
        if (ContainsClip(patrolMusicClips, clip)) return GhostMusicState.Patrol;
        if (ContainsClip(huntMusicClips, clip)) return GhostMusicState.Hunt;
        if (ContainsClip(jumpScareMusicClips, clip)) return GhostMusicState.JumpScare;
        
        return GhostMusicState.None;
    }
    
    /// <summary>
    /// 检查数组中是否包含指定的剪辑
    /// </summary>
    private bool ContainsClip(AudioClip[] clips, AudioClip targetClip)
    {
        if (clips == null) return false;
        
        foreach (var clip in clips)
        {
            if (clip == targetClip) return true;
        }
        return false;
    }
    
    /// <summary>
    /// 立即停止所有音乐（用于游戏暂停等）
    /// </summary>
    public void StopMusicImmediate()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        musicSource.volume = 0f;
        musicSource.Stop();
        currentState = GhostMusicState.None;
    }
    
    /// <summary>
    /// 淡出停止音乐
    /// </summary>
    public void StopMusicWithFade(float fadeTime = -1f)
    {
        if (fadeTime < 0) fadeTime = fadeDuration / 2f;
        
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeOutAndStop(fadeTime));
    }
    
    /// <summary>
    /// 淡出并停止音乐
    /// </summary>
    private IEnumerator FadeOutAndStop(float fadeTime)
    {
        yield return StartCoroutine(FadeVolume(0f, fadeTime));
        musicSource.Stop();
        currentState = GhostMusicState.None;
    }
    
    /// <summary>
    /// 恢复音乐到当前状态
    /// </summary>
    public void ResumeMusic()
    {
        if (currentState != GhostMusicState.None && IsSceneMusicEnabled(currentSceneName))
        {
            ChangeMusicState(currentState);
        }
    }
    
    /// <summary>
    /// 设置音乐音量
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        maxVolume = Mathf.Clamp01(volume);
        musicSource.volume = maxVolume;
    }
    
    /// <summary>
    /// 获取当前音乐状态
    /// </summary>
    public GhostMusicState GetCurrentMusicState()
    {
        return currentState;
    }
    
    /// <summary>
    /// 获取当前播放的音乐剪辑
    /// </summary>
    public AudioClip GetCurrentMusicClip()
    {
        return musicSource.clip;
    }
    
    /// <summary>
    /// 获取当前场景名称
    /// </summary>
    public string GetCurrentSceneName()
    {
        return currentSceneName;
    }
    
    /// <summary>
    /// 检查当前场景是否允许播放音乐
    /// </summary>
    public bool IsMusicEnabledInCurrentScene()
    {
        return IsSceneMusicEnabled(currentSceneName);
    }
    
    /*/// <summary>
    /// 调试方法：强制切换到指定状态
    /// </summary>
    [ContextMenu("切换到普通音乐")]
    public void DebugSwitchToNormal() => ChangeMusicState(GhostMusicState.Normal);
    
    [ContextMenu("切换到巡逻音乐")]
    public void DebugSwitchToPatrol() => ChangeMusicState(GhostMusicState.Patrol);
    
    [ContextMenu("切换到猎杀音乐")]
    public void DebugSwitchToHunt() => ChangeMusicState(GhostMusicState.Hunt);
    
    [ContextMenu("切换到跳脸音乐")]
    public void DebugSwitchToJumpScare() => ChangeMusicState(GhostMusicState.JumpScare);
    
    [ContextMenu("重新随机当前状态音乐")]
    public void DebugShuffleCurrentMusic() => ShuffleCurrentStateMusic();
    
    [ContextMenu("停止音乐")]
    public void DebugStopMusic() => StopMusicImmediate();
    
    [ContextMenu("淡出停止音乐")]
    public void DebugStopMusicWithFade() => StopMusicWithFade();
    
    [ContextMenu("打印当前音乐信息")]
    public void DebugPrintCurrentMusicInfo()
    {
        Debug.Log($"当前状态: {currentState}");
        Debug.Log($"当前音乐: {musicSource.clip?.name ?? "None"}");
        Debug.Log($"音量: {musicSource.volume}");
        Debug.Log($"是否播放: {musicSource.isPlaying}");
        Debug.Log($"当前场景: {currentSceneName}");
        Debug.Log($"场景允许音乐: {IsSceneMusicEnabled(currentSceneName)}");
    }*/
}