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
    private int _currentPowerUpIndex;
    
    [Tooltip("time in seconds until bullet becomes inactive.")] [SerializeField] private int lifetime;

    //WindPowerUp Variables
    private readonly float _knockBackForce = 150f;
    private readonly int _radius = 5;

    private void Awake()
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
        DeterminePowerUp();
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
            ApplyPowerUp(enemy);
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

        void ApplyPowerUp(EnemyController enemy)
        {
            switch (_currentPowerUpIndex)
            {
                case 0:  //Fire PowerUp
                    enemy.TakeDamage(3);
                    enemy.Burn();
                    break;
                case 1:  //Electric PowerUp
                    enemy.ExecuteShake();
                    enemy.TakeDamage(3);
                    break;
                case 2:  //Wind PowerUp
                    WindPowerUp(enemy.transform);
                    break;
                case 3: // Ice PowerUp
                    enemy.StartCoroutine(enemy.Freeze());
                    break;
                default:
                    enemy.TakeDamage(1);
                    break;
            }
        }

        /// <summary>
        /// Determines powerUp bullet will apply
        /// </summary>
        void DeterminePowerUp()
        {
            switch (playerController.currentBulletPowerUp)
            {
                case PlayerController.BulletPowerUpMode.Fire:
                    _currentPowerUpIndex = 0;
                    break;
                case PlayerController.BulletPowerUpMode.Electric:
                    _currentPowerUpIndex = 1;
                    break;
                case PlayerController.BulletPowerUpMode.Wind:
                    _currentPowerUpIndex = 2;
                    break;
                case PlayerController.BulletPowerUpMode.Ice:
                    _currentPowerUpIndex = 3;
                    break;
                default:
                    _currentPowerUpIndex = 4;
                    break;
            }
        }
        
        /// <summary>
          /// /// Adds a force to all colliders around an area
          /// </summary>
          /// <param name="other"></param>
        void WindPowerUp(Transform other) 
        {
            Collider2D[] hitColliders =  Physics2D.OverlapCircleAll(transform.position, _radius, LayerMask.GetMask("Enemy"));
                    
            foreach (Collider2D col in hitColliders)
            { 
                Vector2 knockBackDir = (other.position - transform.position).normalized; 
                col.GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(knockBackDir.x * _knockBackForce, knockBackDir.y * _knockBackForce * 2), ForceMode2D.Impulse);
                col.GetComponent<EnemyController>().TakeDamage(2);
            }
        }

    #endregion
}
