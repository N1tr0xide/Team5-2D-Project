using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private int speed = 20;

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.up * (speed * Time.deltaTime));
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!other.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
