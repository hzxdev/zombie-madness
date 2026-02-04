using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0, 1)]
    public float volume;
    [Range(-3, 3)]
    public float pitch = 1;

    [HideInInspector]
    public AudioSource source;
    
}
