
using UnityEngine; //The core theory behind this way of doing could be applied to other engines (can't comment too much though on this though)

public class PlayerAdvanced : MonoBehaviour
{
	[SerializeField] private PlayerData data;
	[SerializeField] private AnimationHandler anim;

    #region COMPONENTS
    public Rigidbody2D RB { get; private set; }
    #endregion

    #region STATE PARAMETERS
    public bool IsFacingRight { get; private set; }
	public bool IsJumping { get; private set; }
	public bool IsWallJumping { get; private set; }
	public bool IsDashing { get; private set; }

	public float LastOnGroundTime; //{ get; private set; }
	public float LastOnWallTime { get; private set; }
	public float LastOnWallRightTime { get; private set; }
	public float LastOnWallLeftTime { get; private set; }

	private float _wallJumpStartTime;
	private int _lastWallJumpDir;

	private int _dashesLeft;
	private float _dashStartTime;
	private Vector2 _lastDashDir;
	private bool _dashAttacking;
	#endregion

	#region INPUT PARAMETERS
	public float LastPressedJumpTime { get; private set; }
	public float LastPressedDashTime { get; private set; }
	#endregion

	#region CHECK PARAMETERS
	[Header("Checks")] 
	[SerializeField] private Transform _groundCheckPoint;
	[SerializeField] private Vector2 _groundCheckSize;
	[Space(5)]
	[SerializeField] private Transform _frontWallCheckPoint;
	[SerializeField] private Transform _backWallCheckPoint;
	[SerializeField] private Vector2 _wallCheckSize;
    #endregion

    #region LAYERS & TAGS
    [Header("Layers & Tags")]
	[SerializeField] private LayerMask _groundLayer;
    #endregion


	private CircleCollider2D cc;
	private Vector2 colliderSize;
	[SerializeField]
	private float slopeCheckDistance;
	private float slopeDownAngleDown;
	private float slopeDownAngleLeft;
	private float slopeDownAngleRight;
	private float slopeDownAngleUp;
	private Quaternion slopeDownAngleFinal;
	private Vector2 SlopePerpDown;
	private Vector2 SlopePerpLeft;
	private Vector2 SlopePerpRight;
	private Vector2 SlopePerpUp;

	public string currentSlopeText;
	
	public float slopeMultipleCounter;
	public float slopetruefinal;

	[Range(0, 20)] public float slopeRationslider;
    private void Awake()
	{
		RB = GetComponent<Rigidbody2D>();
		cc = GetComponent<CircleCollider2D>();
	}

	private void Start()
	{
        #region SETUP INPUTS
        InputHandler.instance.OnJumpPressed += args => OnJump(args);
		InputHandler.instance.OnJumpReleased += args => OnJumpUp(args);
		InputHandler.instance.OnDash += args => OnDash(args);
        #endregion

        SetGravityScale(data.gravityScale);
		IsFacingRight = true;
		//colliderSize = cc.size;
	}

	private void Update()
	{

        #region TIMERS
        LastOnGroundTime -= Time.deltaTime;
		LastOnWallTime -= Time.deltaTime;
		LastOnWallRightTime -= Time.deltaTime;
		LastOnWallLeftTime -= Time.deltaTime;

		LastPressedJumpTime -= Time.deltaTime;
		LastPressedDashTime -= Time.deltaTime;
		#endregion

		#region GENERAL CHECKS
		if (InputHandler.instance.MoveInput.x != 0)
			CheckDirectionToFace(InputHandler.instance.MoveInput.x > 0);
		#endregion

		#region PHYSICS CHECKS
		if (!IsDashing && !IsJumping)
		{
			//Ground Check
			if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer)) //checks if set box overlaps with ground
				LastOnGroundTime = data.coyoteTime; //if so sets the lastGrounded to coyoteTime

			//Right Wall Check
			if ((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)
					|| (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight))
				LastOnWallRightTime = data.coyoteTime;

			//Right Wall Check
			if ((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)
				|| (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight))
				LastOnWallLeftTime = data.coyoteTime;

			//Two checks needed for both left and right walls since whenever the play turns the wall checkPoints swap sides
			LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
		}
		#endregion

		#region GRAVITY
		if (!IsDashing)
		{
			if (RB.velocity.y >= 0 || IsWallJumping)
				SetGravityScale(data.gravityScale);
			else if (InputHandler.instance.MoveInput.y < 0)
				SetGravityScale(data.gravityScale * data.quickFallGravityMult);
			else
				SetGravityScale(data.gravityScale * data.fallGravityMult);
		}
		#endregion

		#region JUMP CHECKS
		if (IsJumping && RB.velocity.y < 0)
		{
			IsJumping = false;
			//Debug.Break();
		}

		if (IsWallJumping && Time.time - _wallJumpStartTime > data.wallJumpTime)
			IsWallJumping = false;

		if (!IsDashing)
		{
			//Jump
			if (CanJump() && LastPressedJumpTime > 0)
			{
				IsJumping = true;
				IsWallJumping = false;
				Jump();
			}
			//WALL JUMP
			else if (CanWallJump() && LastPressedJumpTime > 0)
			{
				IsWallJumping = true;
				IsJumping = false;

				_wallJumpStartTime = Time.time;
				_lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;

				WallJump(_lastWallJumpDir);
			}
		}
		#endregion

		#region DASH CHECKS
		if (DashAttackOver())
		{
			if (_dashAttacking)
			{
				_dashAttacking = false;
				StopDash(_lastDashDir); //begins stopping dash
			}
			else if (Time.time - _dashStartTime > data.dashAttackTime + data.dashEndTime)
			{
				IsDashing = false; //dash state over, returns to idle/run/inAir
			}
		}

		if (CanDash() && LastPressedDashTime > 0)
		{
			if (InputHandler.instance.MoveInput != Vector2.zero)
				_lastDashDir = InputHandler.instance.MoveInput;
			else
				_lastDashDir = IsFacingRight ? Vector2.right : Vector2.left;

			_dashStartTime = Time.time;
			_dashesLeft--;
			_dashAttacking = true;

			IsDashing = true;
			IsJumping = false;
			IsWallJumping = false;

			StartDash(_lastDashDir);
		}
		#endregion

	}

	private void FixedUpdate()
	{
        #region DRAG
        if (IsDashing)
			Drag(DashAttackOver()? data.dragAmount : data.dashAttackDragAmount);
		else if(LastOnGroundTime <= 0)
			Drag(data.dragAmount);
        else
			Drag(data.frictionAmount);
		#endregion

		#region RUN
		if (!IsDashing)
		{
			if (IsWallJumping)
				Run(data.wallJumpRunLerp);
			else
				Run(1);
		}
		else if (DashAttackOver())
		{
			Run(data.dashEndRunLerp);
		}
		#endregion

		#region SLIDE
		if (LastOnWallTime > 0 && !IsJumping && !IsWallJumping && !IsDashing && LastOnGroundTime <= 0)
        {
			if((LastOnWallLeftTime > 0 && InputHandler.instance.MoveInput.x < 0) || (LastOnWallRightTime > 0 && InputHandler.instance.MoveInput.x > 0))
            {
				Slide();
            }
		}
        #endregion

		#region SLOPE CHECKS
		if (LastOnGroundTime > 0)
		{
			SlopeCheck();
			slopeMultipleCounter = Mathf.RoundToInt(transform.eulerAngles.z / 180);
		}
		else 
		{
			RB.rotation = Mathf.Lerp(RB.rotation, 0, slopeRationslider * Time.deltaTime);
			//slopeDownAngleFinal = 0;
			//RB.rotation = Mathf.Lerp (RB.rotation, 0, 10f * Time.deltaTime);
			currentSlopeText = "No Slope Detected";
		}
		#endregion
    }

    #region INPUT CALLBACKS
    //These functions are called when an even is triggered in my InputHandler. You could call these methods through a if(Input.GetKeyDown) in Update
    public void OnJump(InputHandler.InputArgs args)
	{
		LastPressedJumpTime = data.jumpBufferTime;
	}

	public void OnJumpUp(InputHandler.InputArgs args)
	{
		if (CanJumpCut() || CanWallJumpCut())
			JumpCut();
	}

	public void OnDash(InputHandler.InputArgs args)
	{
		LastPressedDashTime = data.dashBufferTime;
	}
    #endregion

    #region MOVEMENT METHODS

    public void SetGravityScale(float scale)
	{
		RB.gravityScale = scale;
	}

	private void Drag(float amount)
	{
		Vector2 force = amount * RB.velocity.normalized;
		force.x = Mathf.Min(Mathf.Abs(RB.velocity.x), Mathf.Abs(force.x)); //ensures we only slow the player down, if the player is going really slowly we just apply a force stopping them
		force.y = Mathf.Min(Mathf.Abs(RB.velocity.y), Mathf.Abs(force.y));
		force.x *= Mathf.Sign(RB.velocity.x); //finds direction to apply force
		force.y *= Mathf.Sign(RB.velocity.y);

		RB.AddForce(-force, ForceMode2D.Impulse); //applies force against movement direction
	}

	private void Run(float lerpAmount)
	{
		float targetSpeed = InputHandler.instance.MoveInput.x * data.runMaxSpeed; //calculate the direction we want to move in and our desired velocity
		float speedDif = targetSpeed - RB.velocity.x; //calculate difference between current velocity and desired velocity

		#region Acceleration Rate
		float accelRate;

		//gets an acceleration value based on if we are accelerating (includes turning) or trying to decelerate (stop). As well as applying a multiplier if we're air borne
		if (LastOnGroundTime > 0)
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? data.runAccel : data.runDeccel;
		else
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? data.runAccel * data.accelInAir : data.runDeccel * data.deccelInAir;

		//if we want to run but are already going faster than max run speed
		if (((RB.velocity.x > targetSpeed && targetSpeed > 0.01f) || (RB.velocity.x < targetSpeed && targetSpeed < -0.01f)) && data.doKeepRunMomentum)
		{
			accelRate = 0; //prevent any deceleration from happening, or in other words conserve are current momentum
		}
		#endregion

		#region Velocity Power
		float velPower;
		if (Mathf.Abs(targetSpeed) < 0.01f)
		{
			velPower = data.stopPower;
		}
		else if (Mathf.Abs(RB.velocity.x) > 0 && (Mathf.Sign(targetSpeed) != Mathf.Sign(RB.velocity.x)))
		{
			velPower = data.turnPower;
		}
		else
		{
			velPower = data.accelPower;
		}
		#endregion

		// applies acceleration to speed difference, then is raised to a set power so the acceleration increases with higher speeds, finally multiplies by sign to preserve direction
		float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
		movement = Mathf.Lerp(RB.velocity.x, movement, lerpAmount); // lerp so that we can prevent the Run from immediately slowing the player down, in some situations eg wall jump, dash 

		RB.AddForce(movement * Vector2.right); // applies force force to rigidbody, multiplying by Vector2.right so that it only affects X axis 

		if (InputHandler.instance.MoveInput.x != 0)
			CheckDirectionToFace(InputHandler.instance.MoveInput.x > 0);
	}

	private void Turn()
	{
		IsFacingRight = !IsFacingRight;
		//anim.Turn();
	}

	private void Jump()
	{
		//ensures we can't call a jump multiple times from one press
		LastPressedJumpTime = 0;
		LastOnGroundTime = 0;

		#region Perform Jump
		float force = data.jumpForce;
		if (RB.velocity.y < 0)
			force -= RB.velocity.y;

		RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
		anim.Jump();
		#endregion
	}

	private void WallJump(int dir)
	{
		//ensures we can't call a jump multiple times from one press
		LastPressedJumpTime = 0;
		LastOnGroundTime = 0;
		LastOnWallRightTime = 0;
		LastOnWallLeftTime = 0;

		#region Perform Wall Jump
		Vector2 force = new Vector2(data.wallJumpForce.x, data.wallJumpForce.y);
		force.x *= dir; //apply force in opposite direction of wall

		if (Mathf.Sign(RB.velocity.x) != Mathf.Sign(force.x))
			force.x -= RB.velocity.x;

		if (RB.velocity.y < 0) //checks whether player is falling, if so we subtract the velocity.y (counteracting force of gravity). This ensures the player always reaches our desired jump force or greater
			force.y -= RB.velocity.y;

		RB.AddForce(force, ForceMode2D.Impulse);
		#endregion
	}

	private void JumpCut()
	{
		//applies force downward when the jump button is released. Allowing the player to control jump height
		RB.AddForce(Vector2.down * RB.velocity.y * (1 - data.jumpCutMultiplier), ForceMode2D.Impulse);
	}

	private void Slide()
	{
		//works the same as the Run but only in the y-axis
		float targetSpeed = 0;
		float speedDif = targetSpeed - RB.velocity.y;

		float movement = Mathf.Pow(Mathf.Abs(speedDif) * data.slideAccel, data.slidePower) * Mathf.Sign(speedDif);
		RB.AddForce(movement * Vector2.up, ForceMode2D.Force);
	}

	private void StartDash(Vector2 dir)
	{
		LastOnGroundTime = 0;
		LastPressedDashTime = 0;

		SetGravityScale(0);

		RB.velocity = dir.normalized * data.dashSpeed;
	}

	private void StopDash(Vector2 dir)
    {
		SetGravityScale(data.gravityScale);

		if (dir.y > 0)
		{
			if (dir.x == 0)
				RB.AddForce(Vector2.down * RB.velocity.y * (1 - data.dashUpEndMult), ForceMode2D.Impulse);
			else
				RB.AddForce(Vector2.down * RB.velocity.y * (1 - data.dashUpEndMult) * .7f, ForceMode2D.Impulse);
		}
	}
    #endregion

    #region CHECK METHODS
    public void CheckDirectionToFace(bool isMovingRight)
	{
		if (isMovingRight != IsFacingRight)
			Turn();
	}

	private bool CanJump()
    {
		return LastOnGroundTime > 0 && !IsJumping;
    }

	private bool CanWallJump()
    {
		return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && (!IsWallJumping ||
			 (LastOnWallRightTime > 0 && _lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && _lastWallJumpDir == -1));
	}

	private bool CanJumpCut()
    {
		return IsJumping && RB.velocity.y > 0;
    }

	private bool CanWallJumpCut()
	{
		return IsWallJumping && RB.velocity.y > 0;
	}

	private bool CanDash()
	{
		if (_dashesLeft < data.dashAmount && LastOnGroundTime > 0)
			_dashesLeft = data.dashAmount;

		return _dashesLeft > 0;
	}

	private bool DashAttackOver()
    {
		return IsDashing && Time.time - _dashStartTime > data.dashAttackTime;
	}

	private void SlopeCheck()
	{
		Vector2 checkPos = _groundCheckPoint.position - new Vector3(0.0f , colliderSize.y / 2);
		SlopeCheckVertical(checkPos);
		
	}
	private void SlopeCheckHorizontal(Vector2 checkPos)
	{

	}

	private void SlopeCheckVertical(Vector2 checkPos)
	{
		//Floor
		
		RaycastHit2D hitdown = Physics2D.Raycast(checkPos , Vector2.down, slopeCheckDistance, _groundLayer);
		if (hitdown)
			{
				Debug.DrawRay(hitdown.point, hitdown.normal, Color.red);
				//Debug.Log("Normal" + hit.point + hit.normal);
				SlopePerpDown = Vector2.Perpendicular(hitdown.normal);
				slopeDownAngleDown = Vector2.Angle(hitdown.normal, Vector2.up);
				//float angle = Vector3.Angle(hit.point, (hit.point + hit.normal));
				//Debug.DrawLine(hit.point, (hit.point + hit.normal), Color.blue);
				Debug.DrawRay(hitdown.point, SlopePerpDown, Color.blue);
				Debug.Log("Angle Floor" + slopeDownAngleDown);
				Debug.Log ("Slope Per Floor" + SlopePerpDown);
			}

		//Left Wall
		RaycastHit2D hitleft = Physics2D.Raycast(checkPos , Vector2.left, slopeCheckDistance, _groundLayer);
		if (hitleft)
			{
				Debug.DrawRay(hitleft.point, hitleft.normal, Color.cyan);
				SlopePerpLeft = Vector2.Perpendicular(hitleft.normal);
				slopeDownAngleLeft = Vector2.Angle(hitleft.normal, Vector2.right);
				Debug.DrawRay(hitleft.point, SlopePerpLeft, Color.blue);
				Debug.Log("Angle Left Wall" + slopeDownAngleLeft);
				Debug.Log ("Slope Per Left Wall" + SlopePerpLeft);
			}

		//Right Wall
		RaycastHit2D hitright = Physics2D.Raycast(checkPos , Vector2.right, slopeCheckDistance, _groundLayer);
		if (hitright)
			{
				Debug.DrawRay(hitright.point, hitright.normal, Color.cyan);
				SlopePerpRight = Vector2.Perpendicular(hitright.normal);
				slopeDownAngleRight = Vector2.Angle(hitright.normal, Vector2.left);
				Debug.DrawRay(hitright.point, SlopePerpRight, Color.blue);
				Debug.Log("Angle Right Wall" + slopeDownAngleRight);
				Debug.Log ("Slope Per Right Wall" + SlopePerpRight);
			}

		//Ceiling
		RaycastHit2D hitup = Physics2D.Raycast(checkPos , Vector2.up, slopeCheckDistance, _groundLayer);
		if (hitup)
			{
				Debug.DrawRay(hitup.point, hitup.normal, Color.yellow);
				SlopePerpUp = Vector2.Perpendicular(hitup.normal);
				slopeDownAngleUp = Vector2.Angle(hitup.normal, Vector2.down);
				Debug.DrawRay(hitup.point, SlopePerpUp, Color.blue);
				Debug.Log("Angle Ceiling" + slopeDownAngleUp);
				Debug.Log ("Slope Per Ceiling" + SlopePerpUp);
			}

		
		//Floor
		if (RB.rotation > -45 && RB.rotation < 45 || RB.rotation == 360)
			{
				//Floor
				if (SlopePerpDown.y > 0)
					{
						slopeDownAngleDown = ((slopeDownAngleDown * -1));
						Debug.Log ("Left Side Floor");
						currentSlopeText = "Floor Left";
					}
				else if (SlopePerpDown.y <= 0)
					{
						// normal stuff
						Debug.Log ("Right Side Floor");
						currentSlopeText = "Floor Right";
					}
				slopeDownAngleFinal = Quaternion.Euler(0, 0, slopeDownAngleDown);
			}
		
		//Right Wall
		else if (RB.rotation >= 45 && RB.rotation < 135 )
		{
			//Right Wall
			//Down Side of Right Wall
			if (SlopePerpRight.x < 0)
			{
				slopeDownAngleRight = ((slopeDownAngleRight * -1) + 90);
				Debug.Log ("Down Side Right Wall");
				currentSlopeText = "Right Wall Bottom";
			}
			//Up Side of Right Wall
			else if (SlopePerpRight.x > 0)
			{
				slopeDownAngleRight = slopeDownAngleRight + 90;
				Debug.Log ("Up Side Right Wall");
				currentSlopeText = "Right Wall Top";
			}
			slopeDownAngleFinal =  Quaternion.Euler(0, 0, slopeDownAngleRight);
		}
		
		//Ceiling
		else if (RB.rotation >= 135 && RB.rotation <= 180 || RB.rotation <= -135 && RB.rotation >= -180)
		{
			//Ceiling 
			//Right Side of Ceiling
			if (SlopePerpUp.y < 0)
			{
				slopeDownAngleUp = ((slopeDownAngleUp * -1) + 180);
				Debug.Log ("Right Side Ceiling");
				currentSlopeText = "Ceiling Right";
			}
			//Left Side of Ceiling
			else if (SlopePerpUp.y > 0)
				{
					slopeDownAngleUp = slopeDownAngleUp - 180;
					Debug.Log ("Left Side Ceiling");
					currentSlopeText = "Ceiling Left";
				}
			else if (SlopePerpUp.y == 0)
			{
				
				Debug.Log ("Flat Ceiling");
				currentSlopeText = "Ceiling Flat";
			}
			slopeDownAngleFinal =  Quaternion.Euler(0, 0, slopeDownAngleUp);
		}

		//Left Wall
		else if (RB.rotation <= -45 && RB.rotation > -135)
		{
			//Left Wall
			// Up Side of Left Wall
			if (SlopePerpLeft.x > 0)
			{
				slopeDownAngleLeft = ((slopeDownAngleLeft * -1) - 90);
				Debug.Log ("Up Side Left Wall");
				currentSlopeText = "Left Wall Top";
			}
			//Down Side of Left Wall
			else if (SlopePerpLeft.x <= 0)
			{
				slopeDownAngleLeft = slopeDownAngleLeft - 90;
				Debug.Log ("Down Side Left Wall");
				currentSlopeText = "Left Wall Bottom";
			}
			slopeDownAngleFinal = Quaternion.Euler(0, 0, slopeDownAngleLeft);
		}

		
		else 
		{
			//slopeDownAngle = 0;
			Debug.Log ("No Slope Found");
			currentSlopeText = "None";
		}
		
		Debug.Log("Final Angle" + slopeDownAngleFinal);
		
		//slopeMultipleCounter = Mathf.RoundToInt(transform.rotation.z / 360);
		/*if (slopeMultipleCounter > 0 )
		{
			slopetruefinal = slopeDownAngleFinal * slopeMultipleCounter;
		}
		else if (slopeMultipleCounter == 0)
		{
			slopetruefinal = slopeDownAngleFinal;;
		}
		else if (slopeMultipleCounter < 0 )
		{
			slopetruefinal = slopeDownAngleFinal * slopeMultipleCounter;
		}*/
		Debug.Log ("Slope Multiplier" + slopeMultipleCounter);
		Debug.Log ("Slope True Final Value" + slopetruefinal);
		Debug.Log ("Slope Final" + slopeDownAngleFinal);
		//RB.rotation
		//transform.rotation = Quaternion.Euler (new Vector3 (0, 0, (Mathf.Lerp (RB.rotation, slopeDownAngleFinal, slopeRationslider * Time.deltaTime))));
		transform.rotation = Quaternion.Lerp(transform.rotation, slopeDownAngleFinal, slopeRationslider * Time.deltaTime);
		//transform.eulerAngles = new Vector3(0, 0, (Mathf.Lerp (RB.rotation, slopeDownAngleFinal, slopeRationslider * Time.deltaTime))); 
	}
	#endregion
}