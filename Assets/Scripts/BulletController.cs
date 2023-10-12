using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BulletController : MonoBehaviour
{
    [SerializeField] private int speed = 20;
    private Vector3 _cursorPos;
    private Vector3 _rotation;

    [Tooltip("time in seconds until bullet becomes inactive.")] [SerializeField] private int lifetime;

    void OnEnable()
    {
        _cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _rotation = _cursorPos - transform.position;
        float z = Mathf.Atan2(_rotation.x, _rotation.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0,0, -z);
        StartCoroutine(SetInactive());
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.up * (speed * Time.deltaTime));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player"))
        {
            gameObject.SetActive(false);
        }
    }

    IEnumerator SetInactive()
    {
        yield return new WaitForSeconds(lifetime);
        gameObject.SetActive(false);
    }
}
