using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class AudioController : MonoBehaviour
{
    [SerializeField]  private AudioAssets audioAssets;
    [SerializeField] private AudioSource musicPlayer;
    [SerializeField] private AudioSource sfxPlayer;
    
    // Start is called before the first frame update
    void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        if (currentScene == "MainMenu")
        {
            PlayMusic(audioAssets.mainMenu);
        }
        else if (currentScene == "Level_1")
        {
            PlayMusic(audioAssets.level1);
        }
        // else if (currentScene == "Level_2")
        // {
        //     PlayMusic(audioAssets.level2);
        // }
        // else if (currentScene == "Credits")
        // {
        //     PlayMusic(audioAssets.beatLevel);
        // }
    }
    
    /// <summary>
    /// play a sound clip from the musicPlayer Audio Source
    /// </summary>
    /// <param name="soundToPlay"></param>
    public void PlayMusic(AudioClip soundToPlay)
    {
        musicPlayer.clip = soundToPlay;
        musicPlayer.Play();
    }

    /// <summary>
    /// Stop the music player.
    /// </summary>
    public void StopMusic()
    {
        musicPlayer.Stop();
    }
    
    /// <summary>
    /// Play a sound clip from the sfxPlayer. Can play multiple at once.
    /// </summary>
    /// <param name="soundToPlay"></param>
    public void PlaySfx(AudioClip soundToPlay)
    {
        sfxPlayer.clip = soundToPlay;
        sfxPlayer.Play();
    }
}
