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
    
    [Header("Sound Effects")]
    public AudioClip gun;
    public AudioClip gunEmpty;
    public AudioClip reload1;
    public AudioClip reload2;
    public AudioClip reload3;
    public AudioClip whip;

}
