using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;
using UnityExtras;

public enum CharacterStance { Standing, Crouching, Hurt_Standing, Hurt_Crouching, Broken_Standing, Broken_Crouching}
public class EmMovement : MonoBehaviour
{
    [Header("Assets")]
    public CharacterController _characterController;
    public Animator _animator;
    public Animator _maskAnimator;
    private CharacterStance _stance;
    private EmEventCurrator _eventCurrator;
    InventorySystem _trackMemoryState;

    [Header("Movement")]
    public bool _canIMove = true;

    private Vector3 playerVelocity;

    [Header("Speed")]
    private float _targetSpeed;
    public float _speed; //movement speed
    private float _runSpeed;
    private float _sprintingSpeed;
    public float _crawlingSpeed;

    private Vector3 _newVelocity;
    private float _newSpeed;

    public float _turnSmoothTime = 0.1f;
    float _turnSmoothVelocity;

    public AnimatorHash _move;

    [Header("Speed (Normal, Sprinting")]
    [SerializeField] private Vector2 _standingSpeed = new Vector2(0, 0);
    [SerializeField] private Vector2 _crouchingSpeed = new Vector2(0, 0);

    [Header("Capsule (Radius, Height, YOffset")]
    [SerializeField] private Vector3 _standingCapsule = Vector3.zero;
    [SerializeField] private Vector3 _crouchingCapsule = Vector3.zero;

    EmInput _jumpControls;

    [Header("Gravity")]
    [SerializeField] private float _gravity = -5f;
    [SerializeField]
    float _groundedGravity;

    private bool _isGrounded;
    private bool _isFalling;

    [SerializeField] private float _yVelocity;

    [Header("Sprinting")]
    public bool _isSprinting = false;
    [SerializeField] private float _walkingSpeed;

    [Header("Crouching")]
    private bool _characterCrouching;
    [SerializeField] private float _currentHeight;
    [SerializeField] private float _normalHeight;
    [SerializeField] private float _crouchingHeight;
    [SerializeField] Vector3 _currentCenter;
    [SerializeField] Vector3 _normalCenter;
    [SerializeField] Vector3 _crouchingCenter;
    [SerializeField] private bool _isCrouching;

    private LayerMask _layerMask;
    private Collider[] _obstructions = new Collider[8];

    [Header("Climbing")]
    //tutorial: https://www.youtube.com/watch?v=opj5NdqsVWM
    [SerializeField] private float _wallAngleMax;
    [SerializeField] private float _groundAngleMax;
    [SerializeField] private float _dropCheckDistance;
    [SerializeField] private LayerMask _layerMaskClimb;

    //Animator state names
    private const string _standToCrouch = "Base Layer.Base_Crouching";
    private const string _crouchToStand = "Base Layer.Base_Standing";
    private const string _hurtStandToCrouch = "Base Layer.Base_Crouching Hurt";
    private const string _hurtCrouchToStand = "Base Layer.Base_Standing Hurt";
    private const string _brokenStandToCrouch = "Base Layer.Base_Crouching Broken";
    private const string _brokenCrouchToStand = "Base Layer.Base_Standing Broken";

    [Header("Heights")]
    [SerializeField] private float _overPassHeight;
    [SerializeField] private float _stepHeight;
    [SerializeField] private float _climbUpHeight;
    [SerializeField] private float _hangHeight;
    [SerializeField] private float _vaultHeight;

    [Header("Offsets")]
    [SerializeField] private Vector3 _climbOriginDown;
    [SerializeField] private Vector3 _endOffset;
    [SerializeField] private Vector3 _hangOffset;
    [SerializeField] private Vector3 _dropOffset;

    [Header("Raycasts")]
    public GameObject _headRay;
    public GameObject _headRay2;
    public GameObject _footRay;
    public GameObject _footRay2;

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
        _stance = CharacterStance.Standing;

        _runSpeed = _standingSpeed.x;
        _sprintingSpeed = _standingSpeed.y;
        _canIMove = true;
        _isCrouching = false;

        //_eventCurrator.Event.AddListener(OnSMBEvent);


        //set defaults
        SetCapsuleDimensions(_standingCapsule);

        int _mask = 0;
        for (int i = 0; i < 32; i++)
            if (!(Physics.GetIgnoreLayerCollision(gameObject.layer, i)))
                _mask |= 1 << i;

        _layerMask = _mask;

        _trackMemoryState = FindObjectOfType<InventorySystem>();
    }

    private void Awake()
    {
        //_speed = _walkingSpeed;

        _jumpControls = new EmInput();

        _characterController = GetComponent<CharacterController>();
        _characterController.height = _normalHeight; //height character controller
        _characterController.center = _normalCenter; //center character controller

        //SetJumpVariables();
        _isGrounded = false;
        _yVelocity = _gravity; //new
    }

    void Update()
    {
        //SPEED
        float blendValue = Unity.Mathematics.math.round(_newSpeed / (_runSpeed * 2) * 100) / 100;

        //WALKING
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized; //walking around

        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        var characterMovement = new Vector3(horizontal, 0, vertical); //walking around

        if(_canIMove)
        {
        //new movement
        _characterController.Move(move * Time.deltaTime * _newSpeed);

        /*if (move.magnitude > 0)
                _characterController.Move(move * Time.deltaTime * _newSpeed);
            else
                _characterController.Move(Vector3.zero);*/

            if (move.magnitude > 0)
            {
                if (_isSprinting)
                {
                    _targetSpeed = _sprintingSpeed;

                    blendValue = blendValue * 2;
                    _animator.SetFloat(_move, blendValue);
                    _maskAnimator.SetFloat(_move, blendValue);
                }
                else
                {
                    _targetSpeed = _runSpeed;

                    if (blendValue > 0.5f)
                    {
                        blendValue = math.lerp(blendValue, 0.5f, Time.deltaTime * 2);
                    }

                    _animator.SetFloat(_move, blendValue);
                    _maskAnimator.SetFloat(_move, blendValue);
                }
            }
            else
            {
                blendValue = 0f; // math.lerp(blendValue, 0f, Time.deltaTime*2);
                _animator.SetFloat(_move, blendValue);
                _maskAnimator.SetFloat(_move, blendValue);
            }
            _newSpeed = Mathf.Lerp(_newSpeed, _targetSpeed, Time.deltaTime * 2);

            //Velocity
            /*_targetSpeed = move != Vector3.zero ? _runSpeed : 0f;
            _newVelocity = move * _newSpeed;
            transform.Translate(_newVelocity * Time.deltaTime, Space.World);*/


            //TURNING CHARACTER
            if (move != Vector3.zero) //If we're not standing still
            {
                float targetAngle = Mathf.Atan2(characterMovement.x, characterMovement.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }

        MovementJump();
            
            //RAYCASTS
            //Raycast floor for jumping
            /*RaycastHit hit;
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
            
            //JUMPING
            HandleGravity();
            //HandleJump();*/
        }

            //Check memorystate
            if(_trackMemoryState!= null)
            {
                if(_trackMemoryState._hurtCanTrigger)
            {
                SwitchToHurt();
            }
            else if(_trackMemoryState._traumaCanTrigger)
            {
                //start coroutine to switch trees
                SwitchToBroken();
            }

            }
    }

    public void AllowMovement()
    {
        if(_canIMove)
            _canIMove = false;
        else
            _canIMove= true;
    }

    private void MovementJump()
    {
        _isGrounded = _characterController.isGrounded;

        if (_isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }


        playerVelocity.y += _gravity * Time.deltaTime;

        _characterController.Move(playerVelocity * Time.deltaTime);
    }

    public void forgetAnimation()
    {
        _animator.Play("ANI_Forget_Memory");
        _maskAnimator.Play("ANI_Mask_Forget_Memory");
    }

    public void rememberSadAnimation()
    {
        _animator.Play("ANI_Remember_Negative");
        _maskAnimator.Play("ANI_Mask_Remember_Negative");
    }

    public void rememberHappyAnimation()
    {
        _animator.Play("ANI_Remember_GoodMemory");
        _maskAnimator.Play("ANI_Mask_Remember_GoodMemory");
    }

    private void SwitchToHurt()
    {
        switch (_stance)
        {
            case CharacterStance.Standing:
                RequestStanceChange(CharacterStance.Hurt_Standing);
                break;

            case CharacterStance.Crouching:
                RequestStanceChange(CharacterStance.Hurt_Crouching);
                break;
        }
    }

    private void SwitchToBroken()
    {
        switch (_stance)
        {
            case CharacterStance.Hurt_Standing:
                RequestStanceChange(CharacterStance.Broken_Standing);
                break;

            case CharacterStance.Hurt_Crouching:
                RequestStanceChange(CharacterStance.Broken_Crouching);
                break;
        }
    }

    private bool CharacterSweep(Vector3 position, Quaternion rotation, Vector3 direction, float distance, LayerMask layerMask, float inflate)
    {
        //Assuming capsule is on y axis
        float _heightScale = Mathf.Abs(transform.lossyScale.y);
        float _radiusScale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));

        float _radius = _characterController.radius * _radiusScale;
        float _totalHeight = Mathf.Max(_characterController.height * _heightScale, _radius * 2);

        Vector3 _capsuleUp = rotation * Vector3.up;
        Vector3 _center = position + rotation * _characterController.center;
        Vector3 _top = _center + _capsuleUp * (_totalHeight / 2 - _radius);
        Vector3 _bottom = _center - _capsuleUp * (_totalHeight / 2 - _radius);

        bool _sweepHit = Physics.CapsuleCast(
            point1: _bottom,
            point2: _top,
            radius: _radius,
            direction: direction,
            maxDistance: distance,
            layerMask: _layerMaskClimb);

        return _sweepHit;
    }

    public void OnSprintStart()
    {
        if (!_characterCrouching) _isSprinting = true;
    }

    public void OnSprintStop()
    {
        _isSprinting = false;
    }

    public void OnCrouch()
    {
        if(!_isCrouching)
        {
            if(_trackMemoryState._hurtCanTrigger)
                    RequestStanceChange(CharacterStance.Hurt_Crouching);
                else if(_trackMemoryState._traumaCanTrigger)
                    RequestStanceChange(CharacterStance.Broken_Crouching);
            else
                RequestStanceChange(CharacterStance.Crouching);

            _isCrouching = true;
            Debug.Log("Crouching");
        }
    }

    public void OnCrouchStop()
    {
        if (_isCrouching)
        {
            if (_trackMemoryState._hurtCanTrigger)
                RequestStanceChange(CharacterStance.Hurt_Standing);
            else if (_trackMemoryState._traumaCanTrigger)
                RequestStanceChange(CharacterStance.Broken_Standing);
            else
                RequestStanceChange(CharacterStance.Standing);
            
            _isCrouching = false;
            Debug.Log("Standing");
        }
    }

    public bool RequestStanceChange(CharacterStance newStance)
    {
        if (_stance == newStance)
            return true;

        switch (_stance)
        {
            case CharacterStance.Standing:
                if (newStance == CharacterStance.Crouching)
                {
                        _animator.SetBool("IsCrouching", true);
                    _maskAnimator.SetBool("IsCrouching", true);

                    _characterCrouching = true;

                        _runSpeed = _crouchingSpeed.x;
                        _sprintingSpeed = _crouchingSpeed.y;
                        _stance = newStance;
                        _animator.CrossFadeInFixedTime(_standToCrouch, 0.1f);
                        _maskAnimator.CrossFadeInFixedTime(_standToCrouch, 0.1f);
                        SetCapsuleDimensions(_crouchingCapsule);

                        return true;
                }
                else if(newStance == CharacterStance.Hurt_Standing)
                {
                    _animator.SetBool("IsCrouching", false);
                    _maskAnimator.SetBool("IsCrouching", false);

                    _characterCrouching = true;

                    _runSpeed = _standingSpeed.x;
                    _sprintingSpeed = _standingSpeed.y;
                    _stance = newStance;
                    _animator.CrossFadeInFixedTime(_hurtCrouchToStand, 0.1f);
                    _maskAnimator.CrossFadeInFixedTime(_hurtCrouchToStand, 0.1f);
                    SetCapsuleDimensions(_standingCapsule);

                    return true;
                }
                break;

            case CharacterStance.Crouching:
                if (newStance == CharacterStance.Standing)
                {
                    _animator.SetBool("IsCrouching", false);
                    _maskAnimator.SetBool("IsCrouching", false);

                    _characterCrouching = false;

                            _runSpeed = _standingSpeed.x;
                            _sprintingSpeed = _standingSpeed.y;
                            _stance = newStance;
                            _animator.CrossFadeInFixedTime(_crouchToStand, 0.5f);
                            _maskAnimator.CrossFadeInFixedTime(_crouchToStand, 0.5f);
                            SetCapsuleDimensions(_standingCapsule);

                            return true;
                }
                else if (newStance == CharacterStance.Hurt_Crouching)
                {
                    _animator.SetBool("IsCrouching", true);
                    _maskAnimator.SetBool("IsCrouching", true);

                    _characterCrouching = true;

                    _runSpeed = _crouchingSpeed.x;
                    _sprintingSpeed = _crouchingSpeed.y;
                    _stance = newStance;
                    _animator.CrossFadeInFixedTime(_hurtStandToCrouch, 0.1f);

                    _maskAnimator.CrossFadeInFixedTime(_hurtStandToCrouch, 0.1f);
                    SetCapsuleDimensions(_crouchingCapsule);

                    return true;
                }
                break;

            case CharacterStance.Hurt_Standing:
                if (newStance == CharacterStance.Hurt_Crouching)
                {
                    _animator.SetBool("IsCrouching", true);
                    _maskAnimator.SetBool("IsCrouching", true);

                    _characterCrouching = true;

                    _runSpeed = _crouchingSpeed.x;
                    _sprintingSpeed = _crouchingSpeed.y;
                    _stance = newStance;
                    _animator.CrossFadeInFixedTime(_hurtStandToCrouch, 0.1f);

                    _maskAnimator.CrossFadeInFixedTime(_hurtStandToCrouch, 0.1f);
                    SetCapsuleDimensions(_crouchingCapsule);

                    return true;
                }
                else if (newStance == CharacterStance.Broken_Standing)
                {
                    _animator.SetBool("IsCrouching", false);
                    _maskAnimator.SetBool("IsCrouching", false);

                    _characterCrouching = true;

                    _runSpeed = _crouchingSpeed.x;
                    _sprintingSpeed = _crouchingSpeed.y;
                    _stance = newStance;
                    _animator.CrossFadeInFixedTime(_brokenCrouchToStand, 0.1f);

                    _maskAnimator.CrossFadeInFixedTime(_brokenCrouchToStand, 0.1f);
                    SetCapsuleDimensions(_standingCapsule);

                    return true;
                }
                break;

            case CharacterStance.Hurt_Crouching:
                if (newStance == CharacterStance.Hurt_Standing)
                {

                    _animator.SetBool("IsCrouching", false);
                    _maskAnimator.SetBool("IsCrouching", false);

                    _characterCrouching = false;

                        _runSpeed = _standingSpeed.x;
                        _sprintingSpeed = _standingSpeed.y;
                        _stance = newStance;
                        _animator.CrossFadeInFixedTime(_hurtCrouchToStand, 0.5f);

                        _maskAnimator.CrossFadeInFixedTime(_hurtCrouchToStand, 0.5f);
                        SetCapsuleDimensions(_standingCapsule);

                        return true;
                }
                else if (newStance == CharacterStance.Broken_Crouching)
                {
                    _animator.SetBool("IsCrouching", true);
                    _maskAnimator.SetBool("IsCrouching", true);

                    _characterCrouching = true;

                    _runSpeed = _crouchingSpeed.x;
                    _sprintingSpeed = _crouchingSpeed.y;
                    _stance = newStance;
                    _animator.CrossFadeInFixedTime(_brokenStandToCrouch, 0.1f);

                    _maskAnimator.CrossFadeInFixedTime(_brokenStandToCrouch, 0.1f);
                    SetCapsuleDimensions(_crouchingCapsule);

                    return true;
                }
                break;

            case CharacterStance.Broken_Standing:
                if (newStance == CharacterStance.Broken_Crouching)
                {
                    _animator.SetBool("IsCrouching", false);
                    _maskAnimator.SetBool("IsCrouching", true);

                    _characterCrouching = true;

                    _runSpeed = _crouchingSpeed.x;
                    _sprintingSpeed = _crouchingSpeed.y;
                    _stance = newStance;
                    _animator.CrossFadeInFixedTime(_brokenStandToCrouch, 0.1f);

                    _maskAnimator.CrossFadeInFixedTime(_brokenStandToCrouch, 0.1f);
                    SetCapsuleDimensions(_crouchingCapsule);

                    return true;
                }
                break;

            case CharacterStance.Broken_Crouching:
                if (newStance == CharacterStance.Broken_Standing)
                {

                    _animator.SetBool("IsCrouching", false);
                    _maskAnimator.SetBool("IsCrouching", false);

                    _characterCrouching = false;

                        _runSpeed = _standingSpeed.x;
                        _sprintingSpeed = _standingSpeed.y;
                        _stance = newStance;
                        _animator.CrossFadeInFixedTime(_brokenCrouchToStand, 0.5f);

                        _maskAnimator.CrossFadeInFixedTime(_brokenCrouchToStand, 0.5f);
                        SetCapsuleDimensions(_standingCapsule);

                        return true;
                }
                break;
        }

        return false;
    }

    private void SetCapsuleDimensions(Vector3 dimensions)
    {
        _characterController.center = new Vector3(_characterController.center.x, dimensions.z, _characterController.center.z);
        _characterController.radius = dimensions.x;
        _characterController.height = dimensions.y;
    }
}
