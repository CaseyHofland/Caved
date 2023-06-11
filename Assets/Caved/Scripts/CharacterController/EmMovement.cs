using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder;
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
    Vector3 _prevPosition;
    float _speed2;

    [Header("Crouching")]
    private bool _characterCrouching;
    [SerializeField] private float _currentHeight;
    [SerializeField] private float _normalHeight;
    [SerializeField] private float _crouchingHeight;
    [SerializeField] Vector3 _currentCenter;
    [SerializeField] Vector3 _normalCenter;
    [SerializeField] Vector3 _crouchingCenter;
    private bool _shouldBeCrouching = false;
    public float _crawlingSpeed;

    [Header("Climbing")]
    [SerializeField] private bool _isFloor;
    [SerializeField] private bool _isStep;

    [Header("Raycasts")]
    public GameObject _headRay;
    public GameObject _headRay2;
    public GameObject _footRay;
    public GameObject _footRay2;
    public GameObject _edgeRayDown1;
    public GameObject _edgeRayDown2;
    public GameObject _edgeRayUp1;
    public GameObject _edgeRayUp2;

    [SerializeField] LayerMask mask;
    [SerializeField] float maxCastFloor = 0.01f;
    [SerializeField] float _maxCastCeiling = 4;
    [SerializeField] float _maxStepFloor;

    [SerializeField] bool _left;
    [SerializeField] bool _right;
    [SerializeField] bool _up;
    [SerializeField] bool _down;


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
        _characterController.height = _normalHeight; //height character controller
        _characterController.center = _normalCenter; //center character controller

        _yVelocity = _gravity;

        _isGrounded = false;
    }

    void Update()
    {
        //WALKING
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized; //walking around
        if(move.magnitude > 0)
            _characterController.Move(move * Time.deltaTime * _speed);
        else
            _characterController.Move(Vector3.zero);

        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        var characterMovement = new Vector3(horizontal, 0, vertical); //walking around

        if (characterMovement == Vector3.back) Debug.Log("down");
        else if (characterMovement == Vector3.forward) Debug.Log("up");
        else if (characterMovement == Vector3.left) Debug.Log("left");
        else if(characterMovement == Vector3.right) Debug.Log("right");

        //Debug.Log(characterMovement);
        



        //SPEED
        float distance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(_prevPosition.x, 0, _prevPosition.z));
        float time = Time.deltaTime;
        _speed2 = distance / time;

        //Check animation
        var _xzVelocity = _characterController.velocity;
        _xzVelocity.y = 0;
        var _velSpeed = _xzVelocity.magnitude;

        //UPDATING PREVIOUS POSITION
        _prevPosition = transform.position;

        float blendValue = Unity.Mathematics.math.round(_speed2 / (_speed * 2) * 100) / 100;
        //Debug.Log(blendValue);
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
        if (_isSprinting && !_characterCrouching)
        {
            //change speed to running
            _speed = _sprintingSpeed;
        }
        else if(_characterCrouching)
        {
            _speed = _crawlingSpeed;
        }
        else
        {
            //change speed to walking
            _speed = _walkingSpeed;
        }



        //RAYCASTS
        //Raycast floor for jumping
        RaycastHit hit;
        RaycastHit hit0;
        if (Physics.Raycast(_footRay.transform.position, -Vector3.up, out hit0, maxCastFloor, mask))
        {
            Debug.DrawLine(_footRay.transform.position, hit0.point, Color.yellow);
            _isGrounded = true;
        }
        else if (Physics.Raycast(_footRay2.transform.position, -Vector3.up, out hit, maxCastFloor, mask))
        {
            Debug.DrawLine(_footRay2.transform.position, hit.point, Color.yellow);
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }

        //Raycast ceiling for crouching
        RaycastHit hit2;
        RaycastHit hit3;
        if (Physics.Raycast(_headRay.transform.position, Vector3.up, out hit2, _maxCastCeiling, mask))
        {
            Debug.DrawLine(_headRay.transform.position, hit2.point, Color.red);

            _shouldBeCrouching = true;
        }
        else if (Physics.Raycast(_headRay2.transform.position, Vector3.up, out hit3, _maxCastCeiling, mask))
        {
            Debug.DrawLine(_headRay2.transform.position, hit3.point, Color.red);
            _shouldBeCrouching = true;
        }
        else
        {
            _shouldBeCrouching = false;
        }

        //Raycast floor for edge detection
        RaycastHit hit4;
        RaycastHit hit5;

        if (Physics.Raycast(_edgeRayDown1.transform.position, -Vector3.up, out hit4, maxCastFloor, mask))
        {
            _isFloor = true;
        }
        else
        {
            _isFloor = false;

            if (Physics.Raycast(_edgeRayDown1.transform.position, -Vector3.up, out hit4, _maxStepFloor, mask))
            {
                _isStep = true;
            }
            else
            {
                _isStep = false;
                //stop character from walking
            }
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
            _animator.SetBool("IsFalling", true);
        }
        else if (_isJumping)
        {
            //_isGrounded = false;
            _animator.SetBool("IsGrounded", false);
        }
        
    }

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
        if (_isGrounded && !_characterCrouching)
        {
            playerVelocity.y = _jumpForce;
            _jumpPressed = true;
            Debug.Log("am jumping");
        }
    }

    public void OnSprintStart()
    {
        if(!_characterCrouching) _isSprinting = true;
    }

    public void OnSprintStop()
    {
        _isSprinting = false;
    }

    public void OnCrouch()
    {
        Vector3 crouchPos = new Vector3(0, _crouchingHeight, 0);
        Vector3 normalPos = new Vector3(0, _normalHeight, 0);

        if (!_characterCrouching)
        {
            _animator.SetBool("IsCrouching", true);

            _characterCrouching = true;

            _characterController.height = _crouchingHeight; //height character controller
            _characterController.center = _crouchingCenter; //center character controller
        }
        else if(_characterCrouching && !_shouldBeCrouching)
        {
            _animator.SetBool("IsCrouching", false);
            _characterCrouching = false;

            _characterController.height = _normalHeight; //height character controller
            _characterController.center = _normalCenter; //center character controller
        }
    }
}
