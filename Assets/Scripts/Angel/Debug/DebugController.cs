using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DebugController : MonoBehaviour
{
    private bool _showConsole;
    private bool _showHelp;
    private Vector2 _scroll;
    private readonly int _boxHeight = 40;
    private string _input;
    
    //Commands
    private List<object> _commandList;
    private static DebugCommand _help;
    private static DebugCommand _resetScene;
    private static DebugCommand<string> _loadScene;

    // Start is called before the first frame update
    void Awake()
    {
        _resetScene = new DebugCommand("reset_scene", "reloads the current scene", "reset_scene", () =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });

        _loadScene = new DebugCommand<string>("load_scene", "load the specified scene", "load_scene <sceneName>", SceneManager.LoadScene);

        _help = new DebugCommand("help", "show list of commands", "help", () => _showHelp = true);

        _commandList = new List<object>
        {
            _resetScene,
            _loadScene,
            _help
        };
    }

    // Update is called once per frame
    void Update()
    {
        ToggleDebug(Input.GetKeyDown(KeyCode.RightShift));

        if (_showConsole)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                print("command entered");
                HandleInput();
                _input = "";
            }
        }
    }
    
    void ToggleDebug(bool key)
    {
        if(key) { _showConsole = !_showConsole; }
    }

    /// <summary>
    /// check input to see if it corresponds to any command id.
    /// </summary>
    void HandleInput()
    {
        string[] properties = _input.Split(' ');
        
        for (int i = 0; i < _commandList.Count; i++) // for every command on commandList
        {
            //turn list object to DebugCommandBase, then check if text entered matches the id
            if (_commandList[i] is DebugCommandBase commandBase && _input != null && _input.Contains(commandBase.CommandId))
            {
                if (_commandList[i] is DebugCommand)
                {
                    (_commandList[i] as DebugCommand)?.Invoke();
                }
                else if (_commandList[i] is DebugCommand<string> && properties.Length > 1)
                {
                    (_commandList[i] as DebugCommand<string>)?.Invoke(properties[1]);
                }
            }
        }
    }

    /// <summary>
    /// Draw debug boxes
    /// </summary>
    private void OnGUI()
    {
        if(!_showConsole) {return;}
        float y = Screen.height -_boxHeight; //put box at bottom

        GUIStyle style = new GUIStyle
        {
            fontSize = 25,
            alignment = TextAnchor.MiddleLeft,
        };

        if (_showHelp)
        {
            GUI.Box(new Rect(0, y - 100, Screen.width, 100), "");
            Rect viewport = new Rect(0, 0, Screen.width - 30, 20 * _commandList.Count);
            _scroll = GUI.BeginScrollView(new Rect(0, y - 100, Screen.width, 80), _scroll, viewport);

            for (int i = 0; i < _commandList.Count; i++)
            {
                DebugCommandBase command = _commandList[i] as DebugCommandBase;

                string label = $"{command.CommandFormat} - {command.CommandDescription}";
                Rect labelRect = new Rect(5, 25 * i, viewport.width - 100, 20);
                
                GUI.Label(labelRect, label, style);
            }
            
            GUI.EndScrollView();
        }
        
        GUI.Box(new Rect(0, y, Screen.width, _boxHeight), "");
        _input = GUI.TextField(new Rect(10f, y, Screen.width - 20f, _boxHeight), _input, style);
    }
}
