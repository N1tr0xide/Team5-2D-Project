using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    #region COMPONENTS
    [SerializeField] private PlayerAdvanced playerMovement_;
    [SerializeField] private InputHandler inputHandler_;
    public Animator anim;
    [SerializeField] private Rigidbody2D RB;

    #endregion
    
    bool isInteractions;
    bool isRootMotion;
    bool canCancel;
    bool grounded;
    int vertical;
    int horizontal;
    float velocity;
    bool isFacingRight;
    float idleTime;


    #region CHECK PARAMETERS
	[Header("Checks")] 
	[SerializeField] private Transform _groundCheckPoint;
	[SerializeField] private Transform _frontWallCheckPoint;
	[SerializeField] private Transform _backWallCheckPoint;
    #endregion

    private string currentState;

    #region ANIMATION STATES
    //Animation States
    const string PLAYER_IDLE = "Player_idle";
    const string PLAYER_IDLE_ONE = "Player_idle";
    const string PLAYER_IDLE_TWO = "Player_idle";
    const string PLAYER_IDLE_THREE = "Player_idle";
    const string PLAYER_LOOK_UP = "Player_walk";
    const string PLAYER_CROUCH = "Player_crouch";
    const string PLAYER_WALK = "Player_walk";
    const string PLAYER_RUN = "Player_run";
    const string PLAYER_SPRINT = "Player_Sprint";
    const string PLAYER_SLIDE = "Player_Slide";

    const string PLAYER_JUMP_START = "Player_walk";
    const string PLAYER_AIRBORNE = "Player_walk";
    const string PLAYER_WALLJUMP = "Player_walk";
    const string PLAYER_WALLSLIDE = "Player_walk";

    const string PLAYER_ABILITY_SWITCH_G = "Player_walk"; // For Switching Abilities Grounded Idle
    const string PLAYER_ABILITY_SWITCH_A = "Player_walk"; // For Switching Abilities Airborne

    //Player Ground Combo
    const string PLAYER_MELEE_GAZERO = "Player_walk"; // M
    const string PLAYER_MELEE_GAONE = "Player_walk"; // M>M
    const string PLAYER_MELEE_GATWO = "Player_walk"; // M>M>M
    const string PLAYER_MELEE_GBTWO = "Player_walk"; // M>B>M
    const string PLAYER_MELEE_GCONE = "Player_walk"; // B>M
    const string PLAYER_MELEE_GCTWO = "Player_walk"; // B>M>M
    const string PLAYER_BULLET_GAZERO = "Player_walk"; // B
    const string PLAYER_BULLET_GAONE = "Player_walk"; // B>B
    const string PLAYER_BULLET_GATWO = "Player_walk"; // B>B>B
    const string PLAYER_BULLET_GBONE = "Player_walk"; // M>B
    const string PLAYER_BULLET_GBTWO = "Player_walk"; // M>B>B
    const string PLAYER_BULLET_GCTWO = "Player_walk"; // M>M>B

    //Player Air Combo
    const string PLAYER_MELEE_AAZERO = "Player_walk"; // M
    const string PLAYER_MELEE_AAONE = "Player_walk"; // M>M
    const string PLAYER_MELEE_AATWO = "Player_walk"; // M>M>M
    const string PLAYER_MELEE_ABTWO = "Player_walk"; // M>B>M
    const string PLAYER_MELEE_ACONE = "Player_walk"; // B>M
    const string PLAYER_MELEE_ACTWO = "Player_walk"; // B>M>M
    const string PLAYER_BULLET_AAZERO = "Player_walk"; // B
    const string PLAYER_BULLET_AAONE = "Player_walk"; // B>B
    const string PLAYER_BULLET_AATWO = "Player_walk"; // B>B>B
    const string PLAYER_BULLET_ABONE = "Player_walk"; // M>B
    const string PLAYER_BULLET_ABTWO = "Player_walk"; // M>B>B
    const string PLAYER_BULLET_ACTWO = "Player_walk"; // M>M>B

    // Player Proximity Attack
    const string PLAYER_MELEE_PROX_GAZERO = "Player_walk"; // Player Ground to Enemy Ground Melee Proximity Attack
    const string PLAYER_MELEE_PROX_AAZERO = "Player_walk"; // Player Ground to Enemy Air Melee Proximity Attack

    //Player Lasso Combo
    const string PLAYERG_LASSO_LGOOM = "Player_walk"; // Light Grounded 0/1 || M
    const string PLAYERG_LASSO_LGOOB = "Player_walk"; // Light Grounded 0/1 || B
    const string PLAYERG_LASSO_LGT = "Player_walk"; // Light Grounded 2 || None
    const string PLAYERG_LASSO_HGOOM = "Player_walk"; // Heavy Grounded 0/1 || M
    const string PLAYERG_LASSO_HGOOB = "Player_walk"; // Heavy Grounded 0/1 || B
    const string PLAYERG_LASSO_HGTM = "Player_walk"; // Heavy Grounded 2 || B
    const string PLAYERG_LASSO_HGTB = "Player_walk"; // Heavy Grounded 2 || B
    const string PLAYERG_LASSO_LAOOM = "Player_walk"; // Light Air 0/1 || M
    const string PLAYERG_LASSO_LAOOB = "Player_walk"; // Light Air 0/1 || M
    const string PLAYERG_LASSO_LAT = "Player_walk"; // Light Air 2 || None
    const string PLAYERG_LASSO_HAOOM = "Player_walk";  // Heavy Air 0/1 || Melee
    const string PLAYERG_LASSO_HAOOB = "Player_walk"; // Heavy Air 0/1 || B
    const string PLAYERG_LASSO_HAT = "Player_walk"; // Heavy Air 2 || None
    const string PLAYERA_LASSO_LGOO = "Player_walk"; // Light Ground 0/1 || None
    const string PLAYERA_LASSO_LGT = "Player_walk"; // Light Ground 2 || None
    const string PLAYERA_LASSO_HGOOM = "Player_walk"; // Heavy Grounded 0/1 || M
    const string PLAYERA_LASSO_HGOOB = "Player_walk"; // Heavy Grounded 0/1 || B
    const string PLAYERA_LASSO_HGTM = "Player_walk"; // Heavy Grounded 2 || M
    const string PLAYERA_LASSO_HGTB = "Player_walk"; // Heavy Grounded 2 || B
    const string PLAYERA_LASSO_LAOOM = "Player_walk"; // Light Air 0/1 || M
    const string PLAYERA_LASSO_LAOOB = "Player_walk"; // Light Air 0/1 || B
    const string PLAYERA_LASSO_LAT = "Player_walk"; // Light Air 2 || None
    const string PLAYERA_LASSO_HAOOM = "Player_walk"; // Heavy Air 0/1 || M
    const string PLAYERA_LASSO_HAOOB = "Player_walk"; // Heavy Air 0/1 || B
    const string PLAYERA_LASSO_HAT = "Player_walk"; // Heavy Air 2 || None

    // Dashes
    const string PLAYER_DASH_GS = "Player_dash_side"; // Ground Dash Side
    const string PLAYER_DASH_GU = "Player_dash_up"; // Ground Dash Up
    const string PLAYER_DASH_GUD = "Player_dash_side"; // Ground Dash Up Diagonal
    const string PLAYER_DASH_GD = "Player_dash_side"; // Ground Dash Down
    const string PLAYER_DASH_GDD = "Player_dash_side"; // Ground Dash Down Diagonal
    const string PLAYER_DASH_AS = "Player_dash_side"; // Air Dash Side
    const string PLAYER_DASH_AU = "Player_dash_side"; // Air Dash Up
    const string PLAYER_DASH_AUD = "Player_dash_side"; // Air Dash Up Diagonal
    const string PLAYER_DASH_AD = "Player_dash_side"; // Air Dash Down
    const string PLAYER_DASH_ADD = "Player_dash_side"; // Air Dash Down Diagonal

    // Dash Attack
    const string PLAYER_DASH_GM = "Player_DashAttack"; // Ground Dash Melee
    const string Player_DASH_GB = "Player_DashAttack"; // Ground Dash Bullet
    const string PLAYER_DASH_AM = "Player_DashAttack"; // Air Dash Melee
    const string PLAYER_DASH_AB = "Player_DashAttack"; // Air Dash Bullet

    // Damage
    const string PLAYER_HURT_GLO = "Player_walk"; // Ground Light One
    const string PLAYER_HURT_GLT = "Player_walk"; // Ground Light Two
    const string PLAYER_HURT_GHO = "Player_walk"; // Ground Heavy One
    const string PLAYER_HURT_ALO = "Player_walk"; // Air Light One
    const string PLAYER_HURT_ALT = "Player_walk"; // Air Light Two
    const string PLAYER_HURT_AHO = "Player_walk"; // Air Heavy One
    const string PLAYER_HURT_KNOCKUP = "Player_walk"; // Knock Up

    const string PLAYER_DEAD = "Player_walk"; // Dead
    #endregion

    private void Awake() 
    {
        RB = GetComponent<Rigidbody2D>();

    }

    public void Initialize()
    {
        anim = GetComponent<Animator>();
        vertical = Animator.StringToHash("Vertical");
        horizontal = Animator.StringToHash("Horizontal");   
    }

    void ChangeAnimationState(string newState)
    {

        // stop the same animation from interrupting itself
        if (currentState == newState) return;

        //play the animation
        anim.Play(newState);

        //reassign the current state
        currentState = newState;
    }

    public void UpdateAnimatorValues(float verticalMovement, float horizontalMovement)
    {
        #region Vertical
        float v = 0;

        if (verticalMovement > 0 && verticalMovement < 0.55f)
        {
            v = 0.5f;
        }
        else if (verticalMovement > 0.55f)
        {
            v = 1;
        }
        else if (verticalMovement < 0 && verticalMovement> -0.55f)
        {
            v = -0.5f;
        }
        else if (verticalMovement < -0.55f)
        {
            v = -1f;
        }
        else if (verticalMovement == 0)
        {
            v = 0;
        }
        #endregion

        #region Horizontal
        float h = 0;

        if (horizontalMovement > 0 && horizontalMovement < 0.55f)
        {
            h = 0.5f;
        }
        else if (horizontalMovement > 0.55f)
        {
            h = 1;
        }
        else if (horizontalMovement < 0 && horizontalMovement > -0.55f)
        {
            h = -0.5f;
        }
        else if (horizontalMovement < -0.55f)
        {
            h = -1f;
        }
        else if(horizontalMovement == 0)
        {
            h = 0;
        }
        #endregion

        anim.SetFloat("Vertical", v, 0, Time.deltaTime);
        anim.SetFloat("Horizontal", h, 0, Time.deltaTime);
    }

    void Update()
    {
        #region TIMERS
        idleTime += Time.deltaTime;
        #endregion

        UpdateAnimatorValues(inputHandler_.MoveInput.y, inputHandler_.MoveInput.x);
        anim.SetFloat("Velocity", RB.velocity.x);
        anim.SetFloat("Input X", inputHandler_.MoveInput.x);
    }

    public void HandleMovementAnim()
    {
        if (velocity == 0 && vertical == 0 && horizontal == 0 && grounded && !isInteractions)
        {
            ChangeAnimationState(PLAYER_IDLE);
        }
        else if (velocity == 0 && vertical == 0 && horizontal == 0 && !grounded && !isInteractions)
        {
            ChangeAnimationState(PLAYER_AIRBORNE);
        }
        else if (velocity == 0 && vertical == 1 && horizontal == 0 && grounded && !isInteractions)
        {
            ChangeAnimationState(PLAYER_LOOK_UP);
        }
        else if (velocity == 0 && vertical == -1 && horizontal == 0 && grounded && !isInteractions)
        {
            ChangeAnimationState(PLAYER_CROUCH);
        }
        else if (velocity >= 0 && velocity <= 5 && grounded && !isInteractions || velocity <= 0 && velocity >= -5 && grounded && !isInteractions)
        {
            ChangeAnimationState(PLAYER_WALK);
        }
        else if (velocity > 5 && velocity <= 9 && grounded && !isInteractions || velocity < -5 && velocity >= -9 && grounded && !isInteractions)
        {
            ChangeAnimationState(PLAYER_RUN);
        }
        else if (velocity > 9 && grounded && !isInteractions || velocity < -9 && grounded && !isInteractions)
        {
            ChangeAnimationState(PLAYER_SPRINT);
        }
    }

    public void Jump()
    {

    }

    public void Turn()
    {
        if (velocity <= 5 && velocity >= -5)
        {
            anim.Play("Turn Low");
        }
        else if (velocity > 5 || velocity < -5)
        {
            anim.Play("Turn Fast");
        }
    }
    
    public void TurnFinish()
    {
        Vector3 scale = transform.localScale; //stores scale and flips x axis, "flipping" the entire gameObject around. (could rotate the player instead)
		scale.x *= -1;
		transform.localScale = scale;
    }
}
