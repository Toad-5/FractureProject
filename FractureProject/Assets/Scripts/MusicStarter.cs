using System;
using UnityEngine;

public class MusicStarter : MonoBehaviour
{
    public bool onAwake, onTriggerEnter;
    public AudioClip music;
    
    private void Start()
    {
        if(onAwake) SoundManager.PlayMusic(music);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!onTriggerEnter) return;
        if (other.tag == "Player")
        {
            SoundManager.PlayMusic(music);
        }
    }
}
