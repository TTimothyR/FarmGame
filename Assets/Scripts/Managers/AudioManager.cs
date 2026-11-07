using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public enum SoundType
    {
        PlantSeed,
        WaterCrop,
        Harvest,
        SplatInsect,
        LowTimeWarning,
        Count
    }

    [System.Serializable]
    public class Sound
    {
        public SoundType type;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    public AudioSource source;
    public AudioSource loopingSource;
    public Sound[] sounds;

    private readonly Dictionary<SoundType, Sound> _lookUp = new Dictionary<SoundType, Sound>();
    
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        foreach (Sound sound in sounds)
        {
            _lookUp[sound.type] = sound;
        }
    }

    public void Play(SoundType type)
    {
        if (!_lookUp.TryGetValue(type, out Sound sound))
        {
            return;
        }

        AudioClip clip = sound.clip;
        if (clip == null)
        {
            return;
        }
        source.PlayOneShot(clip, sound.volume);
    }
    
    public void PlayLoop(SoundType type)
    {
        if (!_lookUp.TryGetValue(type, out Sound sound))
        {
            return;
        }

        AudioClip clip = sound.clip;

        if (clip == null)
        {
            return;
        }

        if (loopingSource.isPlaying && loopingSource.clip == clip)
        {
            return;
        }

        loopingSource.clip = clip;
        loopingSource.volume = sound.volume;
        loopingSource.loop = true;
        loopingSource.Play();
    }

    public void StopLoop()
    {
        loopingSource.Stop();
    }
}
