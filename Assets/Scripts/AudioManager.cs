using UnityEngine;
using System;
using UnityEngine.Audio;
using Random = UnityEngine.Random;


public class AudioManager : MonoBehaviour
{
    [SerializeField] private Sound[] sounds;
    [SerializeField] private Sound[] ScreamSounds;
    private int hitSoundIndex;
    
    public static AudioManager instance;
    
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        InitializeSounds(sounds);
        InitializeSounds(ScreamSounds);
    }


    private void InitializeSounds(Sound[] soundArray)
    {
        foreach (var sound in soundArray)
        {
            sound.audioSource = gameObject.AddComponent<AudioSource>();
            sound.audioSource.clip = sound.audioClip;
            sound.audioSource.volume = sound.volume;
            sound.audioSource.loop = sound.loop;
        }
    }
    public void PlaySound(string soundName)
    {
        if (soundName == "Scream")
        {
            
            Sound s = ScreamSounds[hitSoundIndex];
            hitSoundIndex = (hitSoundIndex + 1) % ScreamSounds.Length;
            s.audioSource.Play();
        }

        else
        {
            Sound s = Array.Find(sounds, sound => sound.name == soundName);
            if (s == null)
                return;

            s.audioSource.Play(); 
        }
        
        
    }
}