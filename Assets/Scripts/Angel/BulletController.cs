using Angel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BulletController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private int speed = 20;
    private Vector3 _cursorPos;
    private Vector3 _rotation;
    
    [Tooltip("time in seconds until bullet becomes inactive.")] [SerializeField] private int lifetime;

    //WindPowerUp Variables
    private readonly float _knockBackForce = 150f;
    private readonly int _radius = 5;

    private void Start()
    {
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

    void OnEnable()
    {
        //set up target and change rotation to look directly at it.
        _cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _rotation = _cursorPos - transform.position;
        float z = Mathf.Atan2(_rotation.x, _rotation.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0,0, -z);
        Invoke(nameof(SetInactive), lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.up * (speed * Time.deltaTime));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            
            switch (playerController.currentBulletPowerUp)
            {
                case PlayerController.BulletPowerUpMode.Electric:
                    enemy.ExecuteShake();
                    enemy.TakeDamage(3);
                    break;
                case PlayerController.BulletPowerUpMode.Fire:
                    enemy.TakeDamage(3);
                    enemy.Burn();
                    break;
                case PlayerController.BulletPowerUpMode.Ice:
                    enemy.StartCoroutine(enemy.Freeze());
                    break;
                case PlayerController.BulletPowerUpMode.Wind:
                    WindPowerUp(other);
                    break;
                default:
                    enemy.TakeDamage(1);
                    break;
            }
            
            gameObject.SetActive(false);
        }
        else if (!other.gameObject.CompareTag("Player"))
        {
            gameObject.SetActive(false);
        }
    }

    void SetInactive()
    {
        gameObject.SetActive(false);
    }

    #region PowerUps Behaviors
        
        /// <summary>
          /// /// Adds a force to all colliders around an area
          /// </summary>
          /// <param name="other"></param>
        void WindPowerUp(Collider2D other) 
        {
            Collider2D[] hitColliders =  Physics2D.OverlapCircleAll(transform.position, _radius, LayerMask.GetMask("Enemy"));
                    
            foreach (Collider2D col in hitColliders)
            { 
                Vector2 knockBackDir = (other.transform.position - transform.position).normalized; 
                col.GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(knockBackDir.x * _knockBackForce, knockBackDir.y * _knockBackForce * 2), ForceMode2D.Impulse);
                col.GetComponent<EnemyController>().TakeDamage(2);
            }
        }

    #endregion
}
