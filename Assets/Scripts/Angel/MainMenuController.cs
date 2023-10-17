using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MainMenuController : MonoBehaviour
{
    public void OnClickPLay()
    {
        //play transition
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
