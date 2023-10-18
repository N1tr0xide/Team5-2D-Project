using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraController : MonoBehaviour
{
    private Transform _playerPos;
    [SerializeField] private float offsetX;
    [SerializeField] private float speed;
    private float _lookAhead;
    private int _multiplier;

    // Start is called before the first frame update
    void Start()
    {
        _playerPos = GameObject.FindWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(_playerPos.position.x + _lookAhead, _playerPos.position.y, transform.position.z);
        _lookAhead = Mathf.Lerp(_lookAhead, (offsetX * CheckDirection()), Time.deltaTime * speed);
    }
    
    int CheckDirection()
    {
        if (Input.GetAxis("Horizontal") > .1f) { _multiplier = 1; }
        else if (Input.GetAxis("Horizontal") < -.1f) { _multiplier = -1; }

        return _multiplier;
    }
}
