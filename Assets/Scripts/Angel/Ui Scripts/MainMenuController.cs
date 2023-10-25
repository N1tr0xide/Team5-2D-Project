using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MainMenuController : MonoBehaviour
{
    private ScreenTransition _screenTransition;

    private void Start()
    {
        _screenTransition = GameObject.Find("LoadingScreen").GetComponent<ScreenTransition>();
    }

    public void OnClickPLay()
    {
        //StartCoroutine(_screenTransition.LoadSceneCrossFade(3, "Level_1"));
        SceneManager.LoadScene("Level_1");
    }

    public void OnClickHelp()
    {
        //change to help scene
    }

    public void OnClickQuit()
    {
        #if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
        #else
        Application.Quit();
        #endif
    }
}
