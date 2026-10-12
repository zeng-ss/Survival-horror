using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicMgr : UnitySingleTonMono<MusicMgr>
{
    private AudioSource bkMusic; //音频组件
    private float bkVolume = 1; //背景音乐大小
    private float soundVolume = 1; //音效大小
    
    private List<AudioSource> soundlist = new List<AudioSource>();

    /// <summary>
    /// 播放背景音乐  在一个对象上放音频组件   
    /// </summary>
    public void PlayBGMusic(string name)
    {
        if (bkMusic == null) //判断是否有音频组件没有的话就添加一个音频组件
        {
            GameObject obj = new GameObject();
            obj.name = "BGMusic";
            bkMusic = obj.AddComponent<AudioSource>();
        }

        //加载背景音乐 
        bkMusic.clip = ResMgr.Instance.load<AudioClip>("Music/BG/" + name);
        bkMusic.loop = true; //设置背景音乐循环播放
        bkMusic.volume = bkVolume; //设置音频大小为默认的音频大小
        bkMusic.Play(); //播放音频文件
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopBKMusic()
    {
        if (bkMusic == null) return;
        bkMusic.Stop();
    }

    /// <summary>
    /// 暂停背景音乐
    /// </summary>
    public void PauseBKMusic()
    {
        if (bkMusic == null) return;
        bkMusic.Pause();
    }

    public void changeBkVolume(float volume)
    {
        if (bkMusic == null) return;
        bkMusic.volume = volume;
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="soundName"></param>
    public void PlaySound(string soundName, bool isLoop = false)
    {
        //获取音频对象 
        GameObject soundObj = PoolMgr.Instance.getObj("Music/Sound/" + soundName);
        //获取音频组件
        AudioSource source = soundObj.GetComponent<AudioSource>();
        if (soundObj.GetComponent<AudioSource>() == null) 
            source = soundObj.AddComponent<AudioSource>();//如果音频组件为空则添加一个音频组件
        source.clip = ResMgr.Instance.load<AudioClip>("Music/Sound/" + soundName);//加载音频文件
        source.volume = soundVolume;//设置音效大小
        source.loop = isLoop;//设置是否要循环播放
        source.Play();
        soundlist.Add(source);//将音频组件添加到音效列表中 
    }

    /// <summary>
    /// 停止音效
    /// </summary>
    public void StopSound(string soundName, AudioSource source)
    {
        soundName = "Music/Sound/" + soundName;
        if (soundlist.Contains(source))
        {
            soundlist.Remove(source);
            source.Stop();
            PoolMgr.Instance.pushObj(soundName, source.gameObject);
        }
    }

    public void ChangeSoundVolume(float volume)
    {
        soundVolume = volume;
        for (int i = 0; i < soundlist.Count; i++)
        {
            soundlist[i].volume = soundVolume;
        }
    }

    private void Update()
    {
        if (soundlist.Count == 0) return;
        for (int i = soundlist.Count - 1; i >= 0; i--)
        {
            string soundName = soundlist[i].name;
            if (!soundlist[i].isPlaying)
            {
                PoolMgr.Instance.pushObj(soundName, soundlist[i].gameObject);
                soundlist.RemoveAt(i);
            }
        }
    }
}