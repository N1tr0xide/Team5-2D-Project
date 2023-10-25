using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTriggerController : MonoBehaviour
{
    private CameraController _mainCam;
    private LayerMask _cameraTriggerLayer;
    
    // Start is called before the first frame update
    void Start()
    {
        _cameraTriggerLayer = LayerMask.NameToLayer("CameraTrigger");
        _mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
    }

    // Update is called once per frame
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != _cameraTriggerLayer) return;

        _mainCam.FollowPlayer(false);
    }
}
