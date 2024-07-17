using Assets.Scripts.SoundSystem;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Observer, Singleton, Object Pool
/// </summary>
public class SoundManager : MonoBehaviour
{
    // Singleton
    public static SoundManager Instance { get; private set; }
    public List<AudioSource> AudioSources;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        IEffectPlayer.OnPlaySFX += PlaySFX;
    }
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        Debug.Log(clip.name + " is playing");
        foreach (var source in AudioSources)
        {
            if (source.isPlaying) continue;
            source.clip = clip;
            source.Play();
            break;
        }
    }
    private void OnDisable()
    {
        IEffectPlayer.OnPlaySFX -= PlaySFX;
    }
}
