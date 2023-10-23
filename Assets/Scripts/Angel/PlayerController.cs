using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Angel
{
    public class PlayerController : MonoBehaviour
    {
        public ScriptableStats stats;
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private int _fixedFrame;
    
        //Inputs
        private float _horizontalInput; 
        private bool _isJumpButtonPressed; 
        private bool _isJumpButtonHeld;
        private bool _shootingInput;
        private bool _dashingInput;
        private bool _slideInput;

        //Jumping
        private bool _canJump;
        private bool _canBufferJump;
        private bool _isHoldingJump;
        private bool _isCoyoteUsable;
        private int _frameJumpWasPressed;
    
        //Dashing
        private TrailRenderer _dashingTrailRenderer;
        private bool _canDash = true;
        private bool _isDashing;
        
        //Sliding
        private bool _canSlide = true;
        private bool _isSliding;
        private CapsuleCollider2D _mainCollider;
        [SerializeField] private CapsuleCollider2D slideCollider;
    
        //Movement && Collisions
        LayerMask _groundLayerMask; 
        private Vector2 _velocity;
        public bool Grounded { get; private set; }
        private int _frameLeftGround = int.MinValue;

        //Bullets
        private ObjectPooler _bulletPoller;
        [SerializeField] private GameObject bulletPrefab;
        [HideInInspector] public int currentAmmo;
        public event Action<int> OnAmmoChanged;
        public BulletPowerUpMode currentBulletPowerUp;

        public enum BulletPowerUpMode
        {
            Normal,
            Fire,
            Electric,
            Wind,
            Ice
        }

        //Enemy Interactions
        private bool _canBeDamaged = true;
        private bool _isBeingDamaged;
        private int _frameDamageReceived;
        
        //Health
        private int _currentHealth;
        public event Action<int> OnHealthChanged;
        
        //Audio
        private AudioController _audio;


        void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponentInChildren<SpriteRenderer>();
            _dashingTrailRenderer = GetComponent<TrailRenderer>();
            _mainCollider = GetComponent<CapsuleCollider2D>();
            slideCollider.enabled = false;
            _mainCollider.enabled = true;
            _groundLayerMask = LayerMask.GetMask("Ground");
            _audio = GameObject.FindWithTag("AudioObj").GetComponent<AudioController>();
            
            InitializeBulletPooler();
            _currentHealth = stats.maxHealth;
            currentAmmo = stats.maxAmmo;
            //currentBulletPowerUp = BulletPowerUpMode.Normal;
        }
    
        void Update()
        {
            GetInput();
        }

        private void FixedUpdate()
        {
            _fixedFrame++;
            CheckCollisions();
            
            if(_isDashing) return;
            HandleVertical();

            if (!_isSliding)
            {
                HandleJump();
                HandleHorizontal();
            }
            
            ApplyMovement();
        } 
        void GetInput()
        {
            _horizontalInput = Input.GetAxisRaw("Horizontal");
            _isJumpButtonPressed = Input.GetButtonDown("Jump"); 
            _isJumpButtonHeld = Input.GetButton("Jump");
            _shootingInput = Input.GetMouseButtonDown(0);
            _dashingInput = Input.GetKeyDown(KeyCode.LeftShift);
            _slideInput = Input.GetKeyDown(KeyCode.S);

            if (stats.snapInput) //Snap horizontal input after reaching threshold
            { 
                _horizontalInput = Mathf.Abs(_horizontalInput) < stats.horizontalDeadZoneThreshold ? 0 : Mathf.Sign(_horizontalInput);
            }
        
            if (_isJumpButtonPressed) // call jump
            { 
                _canJump = true; 
                _frameJumpWasPressed = _fixedFrame;
            }
        
            if (_shootingInput) { ShootBullet(); }

            if (_dashingInput && _canDash) { StartCoroutine(Dash()); }

            if (_slideInput && _canSlide && Grounded && _rb.velocity.x != 0) { StartCoroutine(Slide()); }
        }
    
        #region Jumping
            private bool HasBufferedJump => _canBufferJump && _fixedFrame < _frameJumpWasPressed + stats.jumpBufferFrames;
            private bool CanUseCoyote => _isCoyoteUsable && !Grounded && _fixedFrame < _frameLeftGround + stats.coyoteFrames;

            void HandleJump()
            {
                _isHoldingJump = !_isHoldingJump && !Grounded && _isJumpButtonHeld && _rb.velocity.y > 0; //return true if jump is being held while moving upwards
                
                if (!_canJump && !HasBufferedJump) return; // if player didn't pressed jump or didn't buffer jump. Cancel Jump.
                
                if (Grounded || CanUseCoyote) //jump if grounded or if coyote is active
                {
                    _isHoldingJump = false;
                    _frameJumpWasPressed = 0;
                    _canBufferJump = false;
                    _isCoyoteUsable = false;
                    _velocity.y = stats.jumpPower;
                }

                _canJump = false;
            }

        #endregion
    
        #region Movement
            private void HandleHorizontal()
            {
                if (_horizontalInput == 0)
                {
                    float deceleration = Grounded ? stats.groundDeceleration : stats.airDeceleration;  //changes horizontal deceleration depending if its grounded or in the air.
                    _velocity.x = Mathf.MoveTowards(_rb.velocity.x, 0, deceleration * Time.fixedDeltaTime);
                }
                else
                {
                    _velocity.x = Mathf.MoveTowards(_rb.velocity.x, _horizontalInput * stats.maxSpeed, stats.acceleration * Time.fixedDeltaTime);
                }
            }

            void HandleVertical()
            {
                if (_isBeingDamaged) {_velocity = _rb.velocity; return;}
                
                if (Grounded && _velocity.y <= 0f)
                {
                    _velocity.y = stats.groundedDownforce;  // if grounded. Apply a small force towards the ground. helps on slopes.
                }
                else
                {
                    float gravity = Physics2D.gravity.y * 2;
                    gravity = _isHoldingJump && _rb.velocity.y > 0 ? gravity / stats.jumpBeingHeldGravityModifier : gravity; // if jump is being held and player is moving up, gravity is divided to jump higher
                    _velocity.y = Mathf.MoveTowards(_velocity.y, stats.maxFallSpeed, gravity * Time.fixedDeltaTime); // Apply gravity until maxFallSpeed
                }
            }

            void ApplyMovement()
            {
                _rb.velocity = _velocity;
            }

        #endregion

        #region Collisions
            void CheckCollisions() 
            {
                if (IsGrounded()) // Landed on Ground
                {
                    Grounded = true;
                    _isCoyoteUsable = true;
                    _canBufferJump = true;
                    _isHoldingJump = false;
                }
                else // left the ground
                {
                    Grounded = false;
                    _frameLeftGround = _fixedFrame;
                }
            }
            
            private void OnCollisionEnter2D(Collision2D other)
            {
                if (other.gameObject.CompareTag("Enemy") && _canBeDamaged)
                {
                    StartCoroutine(HandleDamageReceived(other));
                }
            }

            private void OnTriggerEnter2D(Collider2D collision)
            {
                if (collision.gameObject.CompareTag("DeathZone"))
                {
                    gameObject.SetActive(false);
                    Invoke(nameof(RestartLevel), 3);
                }
            }

            void RestartLevel()
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            bool IsGrounded() //Check if player is touching the Ground LayerMask
            {
                float distance = 1.1f;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, distance, _groundLayerMask);
                return hit.collider;
            }

            bool IsUnderObject()
            {
                float distance = 1.3f;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, distance, _groundLayerMask);
                return hit.collider;
            }

        #endregion
    
        #region Shooting
            void ShootBullet() 
            {
                GameObject bulletToShoot = _bulletPoller.GetPooledObject();
            
                if (bulletToShoot != null && currentAmmo > 0) //if there is an inactive prefab and ammo is higher than 0, shoot bullet
                {
                    bulletToShoot.transform.position = transform.position;
                    bulletToShoot.SetActive(true);
                    _audio.PlaySfx(_audio.audioAssets.gun);
                    currentAmmo--;
                    OnAmmoUpdate();
                }
                else
                {
                    _audio.PlaySfx(_audio.audioAssets.gunEmpty);
                }
            }

            /// <summary>
            ///  // call subscriber to update ammo Ui
            /// </summary>
            public void OnAmmoUpdate()
            {
                OnAmmoChanged?.Invoke(currentAmmo);
            }
            
            void InitializeBulletPooler() 
            {
                GameObject bulletsPooler = new GameObject("BulletPooler").AddComponent<ObjectPooler>().gameObject;
                _bulletPoller = bulletsPooler.GetComponent<ObjectPooler>();
                _bulletPoller.objectToPool = bulletPrefab;
                _bulletPoller.amountToPool = stats.maxAmmo;
            }

        #endregion

        #region Player Health
        
            void TakeDamage(int amount)
            {
                _currentHealth = Mathf.Clamp(_currentHealth - amount, 0, stats.maxHealth);
                OnHealthChanged?.Invoke(_currentHealth); //calls subscriber to update UI

                if (_currentHealth <= 0)
                {
                    //ded
                }
            }
        
            private IEnumerator HandleDamageReceived(Collision2D other)
            {
                _canBeDamaged = false;
                _isBeingDamaged = true;
                _frameDamageReceived = _fixedFrame;
                TakeDamage(1);
                    
                Physics2D.IgnoreLayerCollision(6,7,true);
                Vector2 knockBackDir = (transform.position - other.transform.position).normalized;
                _rb.velocity = new Vector2(knockBackDir.x * stats.knockBackForceHorizontal, knockBackDir.y * stats.knockBackForceVertical);
                _sr.color = new Color(1,0,0,.5f);
                    
                yield return new WaitUntil(() => _frameDamageReceived + stats.knockBackFrames < _fixedFrame);
                _isBeingDamaged = false;
                _frameDamageReceived = _fixedFrame;
                    
                yield return new WaitUntil(() => _frameDamageReceived + stats.iFrames < _fixedFrame);
                Physics2D.IgnoreLayerCollision(6,7,false);
                _sr.color = Color.white;
                _canBeDamaged = true;
            }

        #endregion

        private IEnumerator Dash()
        {
            _canDash = false;
            _canSlide = false;
            _isDashing = true;
            _velocity.y = 0f;
            _rb.velocity = new Vector2(_velocity.x * stats.dashingPower, _velocity.y);
            _dashingTrailRenderer.emitting = true;
            
            yield return new WaitForSeconds(stats.dashingTime);
            _dashingTrailRenderer.emitting = false;
            _isDashing = false;
            yield return new WaitForSeconds(stats.dashingCooldown);
            _canDash = true;
            _canSlide = true;
        }

        IEnumerator Slide()
        {
            _canSlide = false;
            _canDash = false;
            _isSliding = true;
            _mainCollider.enabled = false;
            slideCollider.enabled = true;
            _rb.velocity = new Vector2(Mathf.Sign(_rb.velocity.x) * stats.slidePower, Physics2D.gravity.y);
            _dashingTrailRenderer.emitting = true;
            yield return new WaitForSeconds(stats.minSlideTime);

            //after min time passes, check if player is still under an object
            yield return new WaitWhile(IsUnderObject);
            _dashingTrailRenderer.emitting = false;
            _isSliding = false;
            _mainCollider.enabled = true;
            slideCollider.enabled = false;
            yield return new WaitForSeconds(stats.slideCooldown);
            _canSlide = true;
            _canDash = true;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (stats == null)
            {
                Debug.LogWarning("Please assign a ScriptableStats asset to PlayerController's Stats slot", this);
            }
        }
#endif
    }
}