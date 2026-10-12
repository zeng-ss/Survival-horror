using System.Collections.Generic;
using UnityEngine;
using AutoPool_Tool; // 确保引入正确的命名空间

public class SoundAudioPool : MonoBehaviour
{
    // 音效播放器预制体（仅包含AudioSource组件）
    public GameObject audioPlayerPrefab;
    // 单例实例
    public static SoundAudioPool Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
        
        // 预加载 5个音效播放器（可根据需求调整）
        ObjectPool.SetPreload(audioPlayerPrefab, 8);
    }
    
    // 全局音量系数（ 0~1 ）
    private float globalSoundVolume = 1;
    // 保存音量大小
    [HideInInspector] public float previousVolume = 1;

    public float GlobalSoundVolume
    {
        get => globalSoundVolume;
        set
        {
            globalSoundVolume = Mathf.Clamp01(value); // 限制音量范围在 0~1 之间
            UpdateAllPlayingSoundVolume(); // 如果有正在播放的音效，实时更新它们的音量
        }
    }

    /// <summary>
    /// 实时更新正在播放的音效的音量
    /// </summary>
    private void UpdateAllPlayingSoundVolume()
    {
        PooledObject[] objects = FindObjectsOfType<PooledObject>();
        foreach (var player in objects)
        {
            player.GetComponent<AudioSource>().volume = globalSoundVolume;
        }
    }


    /// <summary>
    /// 从池获取音效播放器并播放指定音效
    /// </summary>
    /// <param name="clip">音效片段</param>
    /// <param name="position">播放位置</param>
    /// <param name="volume">基础音量（0~1）</param>
    /// <param name="pitch">音调（默认1）</param>
    public void PlaySound(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        // 增加详细的错误日志，方便排查
        if (clip == null)
        {
            Debug.LogWarning("SoundAudioPool: 要播放的AudioClip为空！"); return;
        }
        if (audioPlayerPrefab == null)
        {
            Debug.LogError("SoundAudioPool: audioPlayerPrefab引用丢失！"); return;
        }
        try
        {
            // 从对象池获取播放器
            var audioPlayer = ObjectPool.Get(audioPlayerPrefab, position, Quaternion.identity);
            if (audioPlayer == null)
            {
                Debug.LogError("SoundAudioPool: 从对象池获取audioPlayer失败！"); return;
            }
            var audioSource = audioPlayer.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("SoundAudioPool: audioPlayer预制体缺少AudioSource组件！"); return;
            }

            float soundVolume = globalSoundVolume * volume;
            
            // 设置音效属性
            audioSource.clip = clip;
            audioSource.volume = soundVolume;
            audioSource.pitch = pitch;
            audioSource.Play();
            
            // 播放完毕后自动回收（增加微小延迟，避免音效未播放完就被回收）
            float clipLength = clip.length + 0.1f;
            audioPlayer.ReturnAfter(clipLength);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SoundAudioPool播放音效失败：{e.Message}");
        }
    }
}