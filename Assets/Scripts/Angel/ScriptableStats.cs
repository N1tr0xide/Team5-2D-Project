using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class ScriptableStats : ScriptableObject
{
    [Header("INPUT")] 
    [Tooltip("Makes all Input snap to an integer. Prevents gamepads from walking slowly.")]
    public bool snapInput = true;
    
    [Tooltip("Minimum input required before a left or right is recognized. Avoids drifting with sticky controllers"), Range(0.01f, 0.99f)]
    public float horizontalDeadZoneThreshold = 0.1f;

    [Header("HEALTH")] 
    [Tooltip("Maximum amount of health a player can have at a given time")]
    public int maxHealth = 12;

    [Header("MOVEMENT")]
    [Tooltip("The top horizontal movement speed")]
    public float maxSpeed = 14;

    [Tooltip("The player's capacity to gain horizontal speed")]
    public float acceleration = 120;

    [Tooltip("The pace at which the player comes to a stop")]
    public float groundDeceleration = 60;

    [Tooltip("Deceleration in air only after stopping input mid-air")]
    public float airDeceleration = 30;
    
    [Tooltip("A constant downward force applied while grounded. Helps on slopes"), Range(0f, -10f)]
    public float groundedDownforce = -1.5f;
    
    
    [Header("JUMP")] 
    [Tooltip("The maximum vertical movement speed")]
    public float maxFallSpeed = 40;
    
    [Tooltip("The immediate velocity applied when jumping")]
    public float jumpPower = 5;
    
    [Tooltip("The amount gravity is divided by when jump is being held")]
    public float jumpBeingHeldGravityModifier = 3;
    
    [Tooltip("The fixed frames before coyote jump becomes unusable. Coyote jump allows jump to execute even after leaving a ledge")]
    public int coyoteFrames = 7;

    [Tooltip("The amount of fixed frames we buffer a jump. This allows jump input before actually hitting the ground")]
    public int jumpBufferFrames = 7;

    [Header("DASH")] 
    [Tooltip("Force applied in x-direction when dashing")]
    public float dashingPower = 20f;
    
    [Tooltip("duration of dash in seconds ")]
    public float dashingTime = .2f;
    
    [Tooltip("time in seconds before the player can dash again")]
    public float dashingCooldown = 1f;
    
    [Header("KNOCK BACK")] 
    [Tooltip("Amount of frames the player is being pushed away from the enemy"), Range(0f, 50f)] 
    public float knockBackFrames = 7f;

    [Tooltip("amount of invincibility frames the player has after being pushed"), Range(0f,120f)]
    public float iFrames = 60f;

    [Tooltip("the horizontal force applied when the player collides with an enemy"), Range(0f,100f)]
    public float knockBackForceHorizontal = 10f;
    
    [Tooltip("the vertical force applied when the player collides with an enemy"), Range(0f,100f)]
    public float knockBackForceVertical = 8f;

    [Header("BULLETS")] 
    [Tooltip("Maximum amount of bullets the player can have at a given time")]
    public int maxAmmo = 12;
}
