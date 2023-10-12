using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private ScriptableStats stats;
    private Rigidbody2D _rb;
    private int _fixedFrame;
    
    //Inputs
    private float _horizontalInput; 
    private bool _isJumpButtonPressed; 
    private bool _isJumpButtonHeld;
    private bool _shootingInput;
    private bool _dashingInput;

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
    
    //Movement && Collisions
    private Vector2 _velocity;
    private bool _grounded;
    private int _frameLeftGround = int.MinValue;
    
    //BulletPooler
    private ObjectPooler _bulletPoller;
    private int _amountOfBulletsToPool = 5;
    
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _dashingTrailRenderer = GetComponent<TrailRenderer>();
        InitializeBulletPooler();
    }
    
    void Update()
    {
        GetInput();
    }

    private void FixedUpdate()
    {
        if(_isDashing) return;
        
        _fixedFrame++;
        CheckCollisions();
        HandleJump();
        HandleHorizontal();
        HandleVertical();
        ApplyMovement();
    } 
    void GetInput()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _isJumpButtonPressed = Input.GetButtonDown("Jump"); 
        _isJumpButtonHeld = Input.GetButton("Jump");
        _shootingInput = Input.GetMouseButtonDown(0);
        _dashingInput = Input.GetKeyDown(KeyCode.LeftShift);

        if (stats.snapInput) //Snap horizontal input after reaching threshold
        { 
            _horizontalInput = Mathf.Abs(_horizontalInput) < stats.horizontalDeadZoneThreshold ? 0 : Mathf.Sign(_horizontalInput);
        }
        
        if (_isJumpButtonPressed) // call jump
        { 
            _canJump = true; 
            _frameJumpWasPressed = _fixedFrame;
        }
        
        if (_shootingInput)
        {
            ShootBullet();
        }

        if (_dashingInput && _canDash)
        {
            StartCoroutine(Dash());
        }
    }
    
    #region Jumping
        private bool HasBufferedJump => _canBufferJump && _fixedFrame < _frameJumpWasPressed + stats.jumpBufferFrames;
        private bool CanUseCoyote => _isCoyoteUsable && !_grounded && _fixedFrame < _frameLeftGround + stats.coyoteFrames;

        void HandleJump()
        {
            _isHoldingJump = !_isHoldingJump && !_grounded && _isJumpButtonHeld && _rb.velocity.y > 0; //return true if jump is being held while moving upwards
            
            if (!_canJump && !HasBufferedJump) return; // if player didn't pressed jump or didn't buffer jump. Cancel Jump.
            
            if (_grounded || CanUseCoyote) //jump if grounded or if coyote is active
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
                float deceleration = _grounded ? stats.groundDeceleration : stats.airDeceleration;  //changes horizontal deceleration depending if its grounded or in the air.
                _velocity.x = Mathf.MoveTowards(_rb.velocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                _velocity.x = Mathf.MoveTowards(_rb.velocity.x, _horizontalInput * stats.maxSpeed, stats.acceleration * Time.fixedDeltaTime);
            }
        }

        void HandleVertical()
        {
            if (_grounded && _velocity.y <= 0f)
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
                _grounded = true;
                _isCoyoteUsable = true;
                _canBufferJump = true;
                _isHoldingJump = false;
            }
            else /*if (!IsGrounded())*/ // left the ground
            {
                _grounded = false;
                _frameLeftGround = _fixedFrame;
            }
        }
        
        bool IsGrounded() //Check if player is touching the Ground LayerMask
        {
            Vector2 origin = transform.position;
            Vector2 direction = Vector2.down;
            float distance = 1.1f;
            LayerMask ground = LayerMask.GetMask("Ground");
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, ground);

            return hit.collider != null;
        }
    
    #endregion
    
    #region Shooting
        void ShootBullet() 
        {
            GameObject bulletToShoot = _bulletPoller.GetPooledObject();
        
            if (bulletToShoot != null)
            {
                bulletToShoot.transform.position = transform.position;
                bulletToShoot.SetActive(true); 
            }
            else
            {
                print("there is no inactive prefab to use");
            }
        }
        
        void InitializeBulletPooler() 
        {
            GameObject bulletsPooler = new GameObject("BulletPooler").AddComponent<ObjectPooler>().gameObject;
            _bulletPoller = bulletsPooler.GetComponent<ObjectPooler>();
            _bulletPoller.objectToPool = PrefabUtility.LoadPrefabContents("Assets/Prefabs/Bullet.prefab");
            _bulletPoller.amountToPool = _amountOfBulletsToPool;
        }

    #endregion

    private IEnumerator Dash()
    {
        _canDash = false;
        _isDashing = true;
        _velocity.y = 0f;
        _rb.velocity = new Vector2(_velocity.x * stats.dashingPower, _velocity.y);
        _dashingTrailRenderer.emitting = true;
        yield return new WaitForSeconds(stats.dashingTime);
        _dashingTrailRenderer.emitting = false;
        _isDashing = false;
        yield return new WaitForSeconds(stats.dashingCooldown);
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