using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SoundManager : MonoBehaviour
{

    public AudioSource[] destroyNoise;

    public void PlayRandomDestroyNoise()
    {
        //choose random number to randomly play clip
        int clipToPlay = Random.Range(0, destroyNoise.Length);
        //play clip
        destroyNoise[clipToPlay].Play();
    }
   
}
