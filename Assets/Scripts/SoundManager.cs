using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{

    public AudioClip[] musicClips;
    public AudioClip[] winClips;
    public AudioClip[] loseClips;
    public AudioClip[] bonusClips;

    [Range(0, 1)]
    public float musicVolume = 0.5f;

    [Range(0, 1)]
    public float sfxVolume = 1.0f;

    [Range(0, 1)]
    public float lowPitch = 0.9f;
    [Range(0, 1)]
    public float highPitch = 1.0f;


    // Start is called before the first frame update
    void Start()
    {
        
    }


    public AudioSource PlayClipAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip != null)
        {
            GameObject go = new GameObject("SoundFX" + clip.name);
            go.transform.position = position;

            AudioSource source = go.AddComponent<AudioSource>();
            source.clip = clip;

            float randomPitch = Random.Range(lowPitch, highPitch);
            source.pitch = randomPitch;

            source.volume = volume;

            source.Play();
            Destroy(go, clip.length);
            return source;
        }
        return null;
    }

    public AudioSource PlayRandomClip(AudioClip[] clips, Vector3 position, float volume = 1f)
    {
        if (clips != null)
        {
            if(clips.Length != 0)
            {
                int randomIndex = Random.Range(0, clips.Length);

                if (clips[randomIndex] != null)
                {
                   AudioSource source = PlayClipAtPoint(clips[randomIndex], position, volume);
                    return source;
                }
            }
        }
       
        return null;
    }

    public void PlayRandomMusic()
    {
        PlayRandomClip(musicClips, Vector3.zero, musicVolume);
    }

    public void PlayWinSound()
    {
        PlayRandomClip(winClips, Vector3.zero, sfxVolume);
    }

    public void PlayLoseSound()
    {
        PlayRandomClip(loseClips, Vector3.zero, sfxVolume);
    }

    public void PlayBonusSound()
    {
        PlayRandomClip(bonusClips, Vector3.zero, sfxVolume);
    }




}
