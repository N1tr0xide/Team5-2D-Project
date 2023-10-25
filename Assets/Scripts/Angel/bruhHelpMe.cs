using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class bruhHelpMe : MonoBehaviour
{
    private EnemyController _controller;
    
    // Start is called before the first frame update
    void Start()
    {
        _controller = GetComponent<EnemyController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_controller._currentHealth <= 0)
        {
            SceneManager.LoadScene("Credit");
        }
    }
}
