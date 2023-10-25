using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EnemyController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private bool _isShaking;
    private bool _isBurning;
    private int _burnCounter = 0;
    
    [SerializeField] private GameObject damagePopupPrefab;
    [SerializeField] private GameObject powerUpPrefab;
    
    //Health
    [SerializeField] private int maxHealth;
    private int _currentHealth;
    
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isShaking)
        {
            Vector3 newPos = transform.localPosition + Random.insideUnitSphere * (Time.deltaTime * 10);
            newPos.y = transform.localPosition.y;
            newPos.z = transform.localPosition.z;
            transform.localPosition = newPos;
        }

        if (_isBurning && _burnCounter >= 5)
        {
            CancelInvoke(nameof(BurnDamage));
            _isBurning = false;
            _burnCounter = 0;
        }
    }

    #region PowerUp Responses

        public IEnumerator Freeze()
        {
            TakeDamage(1);
            yield return new WaitForSeconds(.2f);
            _rb.constraints = RigidbodyConstraints2D.FreezePositionX;
            _sr.color = Color.cyan;
            yield return new WaitForSeconds(3);
            _rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
            _sr.color = Color.white;
        }

        public void Burn()
        {
            _isBurning = true;
            InvokeRepeating(nameof(BurnDamage), 1, 1);
        }
        
        public void ExecuteShake()
        {
            StartCoroutine(Shake());
        }
        
        IEnumerator Shake()
        {
            _isShaking = true;
            yield return new WaitForSeconds(3);
            _isShaking = false;
        }

    #endregion

    #region Damage Handlers

        public void TakeDamage(int amount)
        {
            _currentHealth = Mathf.Clamp(_currentHealth - amount, 0, maxHealth);
            DamagePopup.Create(damagePopupPrefab, transform.position, amount);
            print($"took {amount} of damage");
            StartCoroutine(HandleDamageReceived());

            if (_currentHealth <= 0)
            {
                if (Random.Range(0, 100) > 30) //Change of 30% for new powerUp
                {
                    Instantiate(powerUpPrefab, transform.position, quaternion.identity);
                }
                
                Destroy(gameObject);
            }
        }

        private void BurnDamage()
        {
            TakeDamage(1);
            if (_isBurning) { _burnCounter++; }
        }

        private IEnumerator HandleDamageReceived()
        {
            _sr.color = new Color(1,0,0,.5f);
            yield return new WaitForSeconds(.2f);
            _sr.color = Color.white;
        }

    #endregion
}