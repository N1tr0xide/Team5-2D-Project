using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenTransition : MonoBehaviour
{
    public static ScreenTransition Instance;
    
    public Animator crossFade;
    private static readonly int Start = Animator.StringToHash("Start");
    
    private void Awake() 
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    } // Singleton Method
    
    public IEnumerator LoadSceneCrossFade(int time , string sceneName)
    {
        yield return new WaitForSeconds(1);
        crossFade.SetTrigger(Start);
        yield return new WaitForSeconds(time);
        SceneManager.LoadScene(sceneName);
        crossFade.SetTrigger(Start);
    }
}
