using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class AudioAssets : ScriptableObject
{
    [Header("Music")] 
    public AudioClip mainMenu;
    public AudioClip level1;
    public AudioClip level2;
    public AudioClip beatLevel;
    public AudioClip beatBoss;
    
    // [Header("Sound Effects")]
}
