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
        //choose random number to randomly play clip
        int clipToPlay = Random.Range(0, destroyNoise.Length);
        //play clip
        destroyNoise[clipToPlay].Play();
    }

    public void PlayRandomErrorNoise()
    {
        //choose random number to randomly play clip
        int clipToPlay = Random.Range(0, destroyNoise.Length);
        //play clip
        destroyNoise[clipToPlay].Play();
    }

    public void PlayRandomBGMusic()
    {
        //choose random number to randomly play clip
        int clipToPlay = Random.Range(0, destroyNoise.Length);
        //play clip
        destroyNoise[clipToPlay].Play();
    }

    private void Start()
    {
        PlayRandomBGMusic();
    }
}
