using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    private AudioSource audioSource;
    public AudioSource musicSource0,musicSource1;
    private int musicSourceIndex = 0;
    private Coroutine musicTransition;
    
    [SerializedDictionary("nom","AudioClip")]
    public SerializedDictionary<string, AudioClip> sfx = new SerializedDictionary<string, AudioClip>();
    
    [SerializedDictionary("nom","Musiqie")]
    public SerializedDictionary<string, AudioClip> musiques = new SerializedDictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else Instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(string name)
    {
        Instance.audioSource.PlayOneShot(Instance.sfx[name]);
    }

    public static void PlayMusic(string name)
    {
        if (Instance.musicTransition == null)
            Instance.musicTransition = Instance.StartCoroutine(Instance.MusicTransition(Instance.musiques[name]));
        else 
        { 
            Instance.StopCoroutine(Instance.musicTransition); 
            Instance.musicTransition = Instance.StartCoroutine(Instance.MusicTransition(Instance.musiques[name]));
        }
    }

    public IEnumerator MusicTransition(AudioClip newMusic, float time =2f)
    {
        AudioSource currentSource = musicSource0;
        AudioSource otherSource = musicSource1;
        if ( musicSourceIndex == 1) { currentSource =  musicSource1; otherSource = musicSource1; }

        float counter = 0f;
        otherSource.clip = newMusic;
        otherSource.Play();
        while (counter < time)
        {
            counter+=Time.deltaTime;
            currentSource.volume = 1-(counter/time);
            otherSource.volume = counter/time;
            yield return null;
        }
        currentSource.Stop();
    }
}
