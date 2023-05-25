using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityExtras;

public class EmMovement : MonoBehaviour
{
    [Header("Movement")]
    public CharacterController _characterController;
    public Animator _animator;

    private float yVelocity = 0.0f;
    private float smoothTime = 0.3f;
    public bool _canIMove = true;

    private Vector3 playerVelocity;

    public float _speed; //movement speed
    public float _turnSmoothTime = 0.1f;
    float _turnSmoothVelocity;

    public AnimatorHash _move;

    [Header("Jumping")]
    EmInput _jumpControls;
    private bool _jumpPressed = false;
    private bool _isGrounded;
    private bool _isJumping;
    private bool _isFalling;
    [SerializeField]
    private float _jumpForce = 1f;

    [SerializeField] private float _yVelocity;
    [SerializeField] private float _gravity = -5f;

    [Header("Sprinting")]
    public bool _isSprinting = false;
    public float _sprintingSpeed;
    [SerializeField] private float _walkingSpeed;

    [Header("Crouching")]
    private bool _characterCrouching;
    [SerializeField] private float _currentHeight;
    [SerializeField] private float _normalHeight;
    [SerializeField] private float _crouchingHeight;

    private void Start()
    {
        //// calculate the correct vertical position:
        //float correctHeight = _characterController.center.y + _characterController.skinWidth;
        //// set the controller center vector:
        //_characterController.center = new Vector3(0, correctHeight, 0);
    }

    private void Awake()
    {
        _speed = _walkingSpeed;

        _jumpControls = new EmInput();

        _characterController = GetComponent<CharacterController>();
        _yVelocity = _gravity;

        _isGrounded = false;
    }

    Vector3 _prevPosition;
    float _speed2;
    [SerializeField]
    LayerMask mask;
    [SerializeField]
    float maxCast = 2;
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, maxCast, mask))
        {
            Debug.DrawLine(transform.position, hit.point, Color.red);
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }

        //WALKING
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized; //walking around
        if(move.magnitude > 0)
            _characterController.Move(move * Time.deltaTime * _speed);
        else
            _characterController.Move(Vector3.zero);

        Debug.Log(_characterController.isGrounded);

        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        var characterMovement = new Vector3(horizontal, 0, vertical); //walking around

        //Check animation
        var _xzVelocity = _characterController.velocity;
        _xzVelocity.y = 0;
        var _velSpeed = _xzVelocity.magnitude;
        //if (_isSprinting)
        //{
        //    _speedFinal += 0.375f;
        //}
        //_animator.SetFloat(_move, _speed > 0f ? ExtraMath.Round(_velSpeed / _speedFinal, 6) : 0f);

        //Speed stuff
        float distance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(_prevPosition.x, 0, _prevPosition.z));
        float time = Time.deltaTime;
        _speed2 = distance / time;

        // Update prevPosition
        _prevPosition = transform.position;

        float blendValue = Unity.Mathematics.math.round(_speed2 / (_speed * 2) * 100) / 100;
        Debug.Log(blendValue);
        if (_speed == _walkingSpeed)
        {
            if (blendValue > 0.5f)
            {
                blendValue = math.lerp(blendValue, 0.5f, Time.deltaTime);
            }
            _animator.SetFloat(_move, blendValue);
        }
        else
        {
            blendValue = blendValue * 2;
            _animator.SetFloat(_move, blendValue);
        }


        //JUMPING
        MovementJump();


        //TURNING CHARACTER
        if (move != Vector3.zero) //If we're not standing still
        {
            float targetAngle = Mathf.Atan2(characterMovement.x, characterMovement.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }


        //SPRINTING
        if (_isSprinting)
        {
            //change speed to running
            _speed = _sprintingSpeed;
        }
        else
        {
            //change speed to walking
            _speed = _walkingSpeed;
        }

        //ANIMATIONS
        if (_isGrounded)
        {
            _isFalling = false;
            _animator.SetBool("IsGrounded", true);
            _animator.SetBool("IsFalling", false);
        }
        else
        {
            _animator.SetBool("IsGrounded", false);
        }

        if (_isFalling)
        {
            _isJumping = false;
            _animator.SetBool("IsJumping", false);
            _animator.SetBool("IsFalling", true);
        }
        else if (_isJumping)
        {
            //_isGrounded = false;
            _animator.SetBool("IsGrounded", false);
            _animator.SetBool("IsJumping", true);
        }
    }


    //IS THE PLAYER STANDING ON THE FLOOR?
    //public void OnTriggerEnter(Collider collision)
    //{
    //    if (collision.gameObject.tag == "Floor")
    //    {
    //        _isGrounded = true;
    //        Debug.Log("smack");
    //    }
    //}

    //public void OnTriggerExit(Collider collision)
    //{
    //    if (collision.gameObject.tag == "Floor")
    //    {
    //        _isGrounded = false;
    //        Debug.Log("yeet");
    //    }
    //}

    private void MovementJump()
    {
        //_isGrounded = _characterController.isGrounded;

        if (_isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        playerVelocity.y += _gravity * Time.deltaTime;

        _characterController.Move(playerVelocity * Time.deltaTime);
    }

    public void OnJump()
    {
        Debug.Log("am trying to jumping");
        if (_isGrounded)
        {
            playerVelocity.y = _jumpForce;
            _jumpPressed = true;
            Debug.Log("am jumping");
        }
    }

    public void OnSprintStart()
    {
        if(!_characterCrouching) _isSprinting = true;
        Debug.Log("am sprinting");
    }

    public void OnSprintStop()
    {
        _isSprinting = false;
        Debug.Log("stopped sprinting");
    }

    public void OnCrouch()
    {
        if(!_characterCrouching)
        {
            _animator.SetBool("IsCrouching", true);
            _characterCrouching = true;
            _currentHeight = _crouchingHeight;
            Debug.Log("I'm crouching");
        }
        else
        {
            _animator.SetBool("IsCrouching", false);
            _characterCrouching = false;
            _currentHeight = _normalHeight;
            Debug.Log("I stopped crouching");
        }
    }
}
