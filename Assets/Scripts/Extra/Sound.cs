using UnityEngine;

[System.Serializable]
public class Sound
{
   [SerializeField] public bool loop;
   [SerializeField] public string name;
   [SerializeField] public AudioClip audioClip;
   [Range(0f,1f)]
   [SerializeField] public float volume;
   [HideInInspector] public AudioSource audioSource;
}
