using UnityEngine;
using System;
using System.Collections.Generic;
public class AudioManager : MonoBehaviour
{
    #region Private Fields

    private int hitSoundIndex;
    private static AudioManager instance;
    
    #endregion

    #region Inspector Control

    [SerializeField] private Sound[] sounds;
    [SerializeField] private Sound[] screamSounds;
    
    #endregion

    private void Awake()
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
        InitializeSounds(screamSounds);
    }


    private void InitializeSounds(IEnumerable<Sound> soundArray)
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
            
            var s = screamSounds[hitSoundIndex];
            hitSoundIndex = (hitSoundIndex + 1) % screamSounds.Length;
            s.audioSource.Play();
        }

        else
        {
            var s = Array.Find(sounds, sound => sound.name == soundName);
            if (s == null)
                return;

            s.audioSource.Play(); 
        }
        
        
    }
}