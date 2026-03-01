using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioService : SingletonBase<AudioService>
{
    public AudioSource BKAudio;
    public AudioSource EffectAudio;
    public float BKVolume => BKAudio.volume;
    public float EffectVolume => EffectAudio.volume;
    public override void Init()
    {
        base.Init();
    }

    public void PlayBK(string ClipName)
    {
        AudioClip clip = Resources.Load<AudioClip>("Music/" + ClipName);
        BKAudio.clip = clip;
        BKAudio.Play();
    }
    public void StopBK()
    {
        BKAudio.Stop();
    }

    // 普通播放：从头播放，允许叠加 (PlayOneShot)
    public void PlayEffect(string ClipName)
    {
        AudioClip clip = Resources.Load<AudioClip>("Music/" + ClipName);
        if (clip != null)
        {
            EffectAudio.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// 截取播放：从指定时间开始，播放指定时长
    /// 注意：这会打断 EffectAudio 当前正在通过 .Play() 播放的主音效
    /// </summary>
    /// <param name="ClipName">音效名称</param>
    /// <param name="startTime">开始时间（秒）</param>
    /// <param name="duration">持续时长（秒）</param>
    public void PlayEffect(string ClipName, float startTime, float duration)
    {
        AudioClip clip = Resources.Load<AudioClip>("Music/" + ClipName);
        if (clip == null) return;

        EffectAudio.clip = clip;

        EffectAudio.time = startTime;

        EffectAudio.Play();

        TimerSystem.Instance.AddTask(duration, () =>
        {
            if (EffectAudio != null && EffectAudio.isPlaying && EffectAudio.clip == clip)
            {
                EffectAudio.Stop();
                EffectAudio.clip = null;
            }
        });
    }

    public void ChangeBKVolume(float value)
    {
        BKAudio.volume = value * 0.2f;
    }

    public void ChangeEffectVolume(float value)
    {
        EffectAudio.volume = value;
    }

    // v9 -- stop effect sound
    public void StopEffect()
    {
        if (EffectAudio != null)
        {
            EffectAudio.Stop();
            EffectAudio.clip = null;
        }
    }
}