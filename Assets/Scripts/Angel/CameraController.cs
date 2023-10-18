using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    private Transform _playerPos;
    [SerializeField] private float offsetX;
    [SerializeField] private float speed;
    private float _lookAhead;
    private int _multiplier;

    [Header("Minimum y positions per level.")]
    [SerializeField] float level1_MinHeight;
    [SerializeField] float level2_MinHeight;
    float yMin;

    [Header("Minimum x positions per level.")]
    [SerializeField] float level1_minXpos;
    [SerializeField] float level2_minXpos;
    float xMin;

    // Start is called before the first frame update
    void Start()
    {
        _playerPos = GameObject.FindWithTag("Player").transform;
        Checklevel();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(_playerPos.position.x + _lookAhead, _playerPos.position.y, transform.position.z);
        ClampCameraPos();
        _lookAhead = Mathf.Lerp(_lookAhead, (offsetX * CheckDirection()), Time.deltaTime * speed);
    }
    
    int CheckDirection()
    {
        if (Input.GetAxis("Horizontal") > .1f) { _multiplier = 1; }
        else if (Input.GetAxis("Horizontal") < -.1f) { _multiplier = -1; }

        return _multiplier;
    }

    void ClampCameraPos()
    {
        float clampY = Mathf.Clamp(transform.position.y, yMin, 500);
        float ClampX = Mathf.Clamp(transform.position.x, xMin, 500000000);
        transform.position = new Vector3(ClampX, clampY, transform.position.z);
    }

    void Checklevel()
    {
        if (SceneManager.GetActiveScene().name == "Level_1")
        {
            yMin = level1_MinHeight;
            xMin = level1_minXpos;
        }
        else if (SceneManager.GetActiveScene().name == "Level_2")
        {
            yMin = level2_MinHeight;
            xMin = level2_minXpos;
        }
    }
}
