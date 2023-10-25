using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class CameraController : MonoBehaviour
{
    private Transform _playerPos;
    [SerializeField] private float offsetX;
    [SerializeField] private float speed;
    private float _lookAhead;
    private int _multiplier;
    private bool _isFollowingPlayer;

    [Header("Minimum y positions per level.")]
    [SerializeField] float level1_MinHeight;
    [SerializeField] float level2_MinHeight;
    float _yMin;

    [Header("Minimum x positions per level.")]
    [SerializeField] float level1_minXpos;
    [SerializeField] float level2_minXpos;
    float _xMin;
    
    [Header("Camera Positions for bosses")]
    [SerializeField] private Vector3 level1MiniBossPos;
    [SerializeField] private Vector3 level2MiniBossPos;
    private Vector3 _staticPosition;

    // Start is called before the first frame update
    void Start()
    {
        _playerPos = GameObject.FindWithTag("Player").transform;
        CheckLevel();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isFollowingPlayer)
        {
            float xPos = Mathf.Lerp(transform.position.x, _staticPosition.x, Time.deltaTime * speed);
            float yPos = Mathf.Lerp(transform.position.y, _staticPosition.y, Time.deltaTime * speed);
            transform.position = new Vector3(xPos, yPos, _staticPosition.z);
            return;
        }

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
        float clampY = Mathf.Clamp(transform.position.y, _yMin, 500);
        float clampX = Mathf.Clamp(transform.position.x, _xMin, 500000000);
        transform.position = new Vector3(clampX, clampY, transform.position.z);
    }

    private void CheckLevel()
    {
        if (SceneManager.GetActiveScene().name == "Level_1")
        {
            _yMin = level1_MinHeight;
            _xMin = level1_minXpos;
            _isFollowingPlayer = true;
            _staticPosition = level1MiniBossPos;
        }
        else if (SceneManager.GetActiveScene().name == "Level_2")
        {
            _yMin = level2_MinHeight;
            _xMin = level2_minXpos;
            _isFollowingPlayer = true;
        }
    }

    public void FollowPlayer(bool follow)
    {
        _isFollowingPlayer = follow;
    }
}
