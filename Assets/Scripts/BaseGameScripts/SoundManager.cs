using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SoundManager : MonoBehaviour
{

    public AudioSource[] destroyNoise;
    public AudioSource[] errorNoise;
    public AudioSource[] backGroundMusic;

    public void PlayRandomDestroyNoise()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            if(PlayerPrefs.GetInt("Sound") == 1)
            {
                //choose random number to randomly play clip
                int clipToPlay = Random.Range(0, destroyNoise.Length);
                //play clip
                destroyNoise[clipToPlay].Play();
            }
        }
        else
        {
            //choose random number to randomly play clip
            int clipToPlay = Random.Range(0, destroyNoise.Length);
            //play clip
            destroyNoise[clipToPlay].Play();
        }
        
    }

    public void PlayRandomErrorNoise()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 1)
            {
                //choose random number to randomly play clip
                int clipToPlay = Random.Range(0, destroyNoise.Length);
                //play clip
                destroyNoise[clipToPlay].Play();
            }
        }
        else
        {
            //choose random number to randomly play clip
            int clipToPlay = Random.Range(0, destroyNoise.Length);
            //play clip
            destroyNoise[clipToPlay].Play();
        }
        
    }

    public void PlayRandomBGMusic()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 1)
            {
                //choose random number to randomly play clip
                int clipToPlay = Random.Range(0, destroyNoise.Length);
                //play clip
                destroyNoise[clipToPlay].Play();
            }
        }
        else
        {
            //choose random number to randomly play clip
            int clipToPlay = Random.Range(0, destroyNoise.Length);
            //play clip
            destroyNoise[clipToPlay].Play();
        }
        
    }

    private void Start()
    {
        PlayRandomBGMusic();
    }
}
