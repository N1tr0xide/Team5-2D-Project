using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Angel
{
    public class PlayerAnimationController : MonoBehaviour
    {
        private PlayerController _playerController;
        private Animator _animator;
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;

        private bool _isGrounded;
        private static readonly int Velocity = Animator.StringToHash("Velocity");
        private static readonly int InputX = Animator.StringToHash("Input X");

        private string _currentState;

        // Start is called before the first frame update
        void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _playerController = GetComponent<PlayerController>();
            _sr = GetComponentInChildren<SpriteRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            _animator.SetFloat(Velocity, _rb.velocity.x);
            _animator.SetFloat(InputX, Input.GetAxis("Horizontal"));
            _isGrounded = _playerController.Grounded;
            HandleAnimation();
        }
        
        void ChangeAnimationState(string newState)
        {
            // stop the same animation from interrupting itself
            if (_currentState == newState) return;
            
            //play the animation
            _animator.Play(newState);

            //reassign the current state
            _currentState = newState;
        }

        void HandleAnimation()
        {
            if (_rb.velocity.x == 0 && _isGrounded)
            {
                ChangeAnimationState("Grounded Idle");
            }
            else if (_rb.velocity.x != 0 && _isGrounded)
            {
                CheckDirection();
                ChangeAnimationState("Run");
            }
        }

        /// <summary>
        /// Check input to determine direction to look towards
        /// </summary>
        void CheckDirection()
        {
            if (Input.GetAxis("Horizontal") > .1f) { _sr.flipX = false; }
            else if (Input.GetAxis("Horizontal") < -.1f) { _sr.flipX = true; }
        }
    }
}
