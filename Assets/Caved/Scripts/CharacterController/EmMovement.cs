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
    public InventorySystem _trackMemoryState;

    [Header("Movement")]
    private float yVelocity = 0.0f;
    private float smoothTime = 0.3f;
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
    
    [Header("Jumping")]
    private bool _jumpPressed = false;
    float _initialJumpingVelocity;
    float _maxJumpHeight = 1f;
    float _maxJumpTime = .5f;
    private bool _isJumping;

    [Header("Gravity")]
    [SerializeField] private float _gravity = 9f;
    [SerializeField]
    float _groundedGravity;

    private bool _isGrounded;
    private bool _isFalling;
    [SerializeField]
    private float _jumpForce = 1f;

    [SerializeField] private float _yVelocity;

    [Header("Sprinting")]
    public bool _isSprinting = false;
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

    private LayerMask _layerMask;
    private Collider[] _obstructions = new Collider[8];

    [Header("Climbing")]
    //tutorial: https://www.youtube.com/watch?v=opj5NdqsVWM
    [SerializeField] private float _wallAngleMax;
    [SerializeField] private float _groundAngleMax;
    [SerializeField] private float _dropCheckDistance;
    [SerializeField] private LayerMask _layerMaskClimb;
    private Coroutine _hangRoutine;

    //Animator state names
    private const string _standToCrouch = "Base Layer.Base_Crouching";
    private const string _crouchToStand = "Base Layer.Base_Standing";
    private const string _hurtStandToCrouch = "Base Layer.Base_Crouching Hurt";
    private const string _hurtCrouchToStand = "Base Layer.Base_Standing Hurt";
    private const string _brokenStandToCrouch = "Base Layer.Base_Crouching Broken";
    private const string _brokenCrouchToStand = "Base Layer.Base_Standing Broken";

    //animation settings
    private bool _proning;

    [Header("Climb Settings")]
    private bool _climbing;
    private bool _isHanging;
    private bool _climbingMove;
    private bool _dropDown;

    private RaycastHit _downRaycastHit;
    private RaycastHit _forwardRaycastHit;

    private Vector3 _endPosition;
    private Vector3 _matchTargetPosition;

    private Quaternion _matchTargetRotation;
    private Quaternion _forwardNormalXZRotation;

    private MatchTargetWeightMask _weightMask = new MatchTargetWeightMask(Vector3.one, 1);

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

    [Header("Animation settings")]
    public CrossFadeSettingsEm _standToFreeHandSettings;
    public CrossFadeSettingsEm _climbUpSettings;
    public CrossFadeSettingsEm _vaultSettings;
    public CrossFadeSettingsEm _dropSettings;
    public CrossFadeSettingsEm _dropToAirSettings;
    public CrossFadeSettingsEm _stepUpSettings;
    public CrossFadeSettingsEm _jumpSettings;

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

        if (!_climbing)
        {
            if (move.magnitude > 0)
                //_characterController.Move(move * Time.deltaTime * _runSpeed);
                _characterController.Move(move * Time.deltaTime * _newSpeed);
            else
                _characterController.Move(Vector3.zero);



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
            _targetSpeed = move != Vector3.zero ? _runSpeed : 0f;
            _newVelocity = move * _newSpeed;
            transform.Translate(_newVelocity * Time.deltaTime, Space.World);


            //TURNING CHARACTER
            if (move != Vector3.zero) //If we're not standing still
            {
                float targetAngle = Mathf.Atan2(characterMovement.x, characterMovement.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
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
            
            //JUMPING
            HandleGravity();
            //HandleJump();

            //Check memorystate
            if(_trackMemoryState._hurtCanTrigger)
            {
                SwitchToHurt();
            }
            else if(_trackMemoryState._traumaCanTrigger)
            {
                //start coroutine to switch trees
            }
        }
        else if (_climbing && _isHanging)
        {
            if (vertical < 0)
                _dropDown = true;
            Debug.Log("falling");
        }


    }


    RaycastHit downRaycastHit;
    RaycastHit forwardRaycastHit;
    Vector3 endPosition;

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

    private bool CanClimb()
    {
        endPosition = Vector3.zero;
        downRaycastHit = new RaycastHit();
        forwardRaycastHit = new RaycastHit();

        bool _downHit;
        bool _forwardHit;
        bool _overpassHit;
        float _climbHeight;
        float _groundAngle;
        float _wallAngle;

        RaycastHit _downRaycastHit;
        RaycastHit _forwardRaycastHit;
        RaycastHit _overpassRaycastHit;
        ClimbModifier _climbModifier;

        Vector3 _endPosition;
        Vector3 _forwardDirectionXZ;
        Vector3 _forwardNormalXZ;

        Vector3 _downDirection = Vector3.down;
        Vector3 _downOrigin = transform.TransformPoint(_climbOriginDown);

        _downHit = Physics.Raycast(_downOrigin, _downDirection, out _downRaycastHit, _climbOriginDown.y - _stepHeight, _layerMaskClimb);
        _climbModifier = _downHit ? _downRaycastHit.collider.GetComponent<ClimbModifier>() : null;

        //Debug.DrawLine(_downOrigin.transform.position, downRaycastHit.point, Color.yellow);

        if (_downHit)
        {
            if (_climbModifier == null || _climbModifier.Climable)
            {
                //forward + overpass cast
                float _forwardDistance = _climbOriginDown.z;
                Vector3 _forwardOrigin = new Vector3(transform.position.x, _downRaycastHit.point.y - 0.1f, transform.position.z);
                Vector3 _overpassOrigin = new Vector3(transform.position.x, _overPassHeight - 0.1f, transform.position.z);

                _forwardDirectionXZ = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
                _forwardHit = Physics.Raycast(_forwardOrigin, _forwardDirectionXZ, out _forwardRaycastHit, _forwardDistance, _layerMaskClimb);
                _overpassHit = Physics.Raycast(_overpassOrigin, _forwardDirectionXZ, out _overpassRaycastHit, _forwardDistance, _layerMaskClimb);
                _climbHeight = _downRaycastHit.point.y - transform.position.y;

                if (_forwardHit)
                {
                    if (_overpassHit || _climbHeight < _overPassHeight)
                    {
                        //Angles
                        _forwardNormalXZ = Vector3.ProjectOnPlane(_forwardRaycastHit.normal, Vector3.up);
                        _groundAngle = Vector3.Angle(_downRaycastHit.normal, Vector3.up);
                        _wallAngle = Vector3.Angle(-_forwardNormalXZ, _forwardDirectionXZ);

                        if (_wallAngle <= _wallAngleMax)
                        {
                            if (_groundAngle <= _groundAngleMax)
                            {
                                //end offset
                                Vector3 _vectSurface = Vector3.ProjectOnPlane(_forwardDirectionXZ, _downRaycastHit.normal);
                                _endPosition = _downRaycastHit.point + Quaternion.LookRotation(_vectSurface, Vector3.up) * _endOffset;

                                //de-penetration
                                Collider _colliderB = _downRaycastHit.collider;
                                bool _penetrationOverlap = Physics.ComputePenetration(
                                    colliderA: _characterController,
                                    positionA: _endPosition,
                                    rotationA: transform.rotation,
                                    colliderB: _colliderB,
                                    positionB: _colliderB.transform.position,
                                    rotationB: _colliderB.transform.rotation,
                                    direction: out Vector3 _penetrationDirection,
                                    distance: out float _penetrationDistance);
                                if (_penetrationOverlap)
                                {
                                    _endPosition += _penetrationDirection * _penetrationDistance;
                                }

                                //Up sweep
                                float _inflate = -0.05f;
                                float _upsweepDistance = _downRaycastHit.point.y - transform.position.y;
                                Vector3 _upSweepDirection = transform.up;
                                Vector3 _upSweepOrigin = transform.position;
                                bool _upSweepHit = CharacterSweep(
                                    position: _upSweepOrigin,
                                    rotation: transform.rotation,
                                    direction: _upSweepDirection,
                                    distance: _upsweepDistance,
                                    layerMask: _layerMaskClimb,
                                    inflate: _inflate);

                                //Forward sweep
                                Vector3 _forwardSweepOrigin = transform.position + _upSweepDirection * _upsweepDistance;
                                Vector3 _forwardSweepVector = _endPosition - _forwardSweepOrigin;
                                bool _forwardSweepHit = CharacterSweep(
                                    position: _forwardSweepOrigin,
                                    rotation: transform.rotation,
                                    direction: _forwardSweepVector.normalized,
                                    distance: _forwardSweepVector.magnitude,
                                    layerMask: _layerMaskClimb,
                                    inflate: _inflate);

                                if (!_upSweepHit && !_forwardSweepHit)
                                {
                                    endPosition = _endPosition;
                                    downRaycastHit = _downRaycastHit;
                                    forwardRaycastHit = _forwardRaycastHit;

                                    return true;
                                }
                                else return false;

                            }
                            else return false;
                        }
                        else return false;
                    }
                    else return false;
                }
                else return false;
            }
            else
                return false;
        }
        else
            return false;
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

    private void InitiateClimb()
    {
        _climbing = true;
        _speed = 0f;
        _animator.SetFloat(_move, 0);
        _maskAnimator.SetFloat(_move, 0);
        //_characterController.enabled = false;
        //Rigidbody.isKinematic = true;

        float _climbHeight = downRaycastHit.point.y - transform.position.y;
        _endPosition = downRaycastHit.point;
        Vector3 _forwardNormalXZ = Vector3.ProjectOnPlane(_forwardRaycastHit.normal, Vector3.up);
        _forwardNormalXZRotation = Quaternion.LookRotation(-_forwardNormalXZ, Vector3.up);

        if (_climbHeight > _hangHeight)
        {
            _matchTargetPosition = forwardRaycastHit.point + _forwardNormalXZRotation * _hangOffset;
            _matchTargetRotation = _forwardNormalXZRotation;
            _animator.applyRootMotion = true;
            _animator.CrossFadeInFixedTimeEm(_standToFreeHandSettings);
            _maskAnimator.applyRootMotion = true;
            _maskAnimator.CrossFadeInFixedTimeEm(_standToFreeHandSettings);

            _isHanging = true;

            Debug.Log("hanging");//never called
        }
        else if (_climbHeight > _climbUpHeight)
        {
            _matchTargetPosition = _endPosition;
            _matchTargetRotation = _forwardNormalXZRotation;

            _animator.rootPosition = transform.position;
            _animator.rootRotation = transform.rotation;
            _animator.applyRootMotion = true;
            _animator.CrossFadeInFixedTimeEm(_climbUpSettings);

            _maskAnimator.rootPosition = transform.position;
            _maskAnimator.rootRotation = transform.rotation;
            _maskAnimator.applyRootMotion = true;
            _maskAnimator.CrossFadeInFixedTimeEm(_climbUpSettings);

            Debug.Log("climb up");
        }
        else if (_climbHeight > _vaultHeight)
        {
            _matchTargetPosition = _endPosition;
            _matchTargetRotation = _forwardNormalXZRotation;
            _animator.applyRootMotion = true;
            _animator.CrossFadeInFixedTimeEm(_vaultSettings);
            
            _maskAnimator.applyRootMotion = true;
            _maskAnimator.CrossFadeInFixedTimeEm(_vaultSettings);
            Debug.Log("vault");
        }
        else if (_climbHeight > _stepHeight)
        {
            _matchTargetPosition = _endPosition;
            _matchTargetRotation = _forwardNormalXZRotation;
            //_animator.applyRootMotion = true;

            StartCoroutine(stepRoot());
            _animator.CrossFadeInFixedTimeEm(_stepUpSettings);
            _maskAnimator.CrossFadeInFixedTimeEm(_stepUpSettings);
            Debug.Log("steppng");
        }
        else
        {
            _characterController.Move(_animator.rootPosition);
            _characterController.enabled = true;
            _climbing = false;
        }
    }
    
    /*private void SetJumpVariables()
    {
        float _timeToApex = _maxJumpTime / 2;
        //_gravity = (-2 * _maxJumpHeight) / Mathf.Pow(_timeToApex, 2); //ergens schiet de gravity ver het negatief in
        _initialJumpingVelocity = (2 * _maxJumpHeight) / _timeToApex;
    }*/

    private void HandleGravity()
    {
        bool isFalling = playerVelocity.y <= 0.0f || !_jumpPressed;
        float _fallMultiplier = 2f;

        //apply proper gravity if the player is grounded or not
        if(_isGrounded)
        {
            playerVelocity.y = _groundedGravity;
        }
        else
        {
            float _previousYVelocity = playerVelocity.y;
            float _newYVelocity = playerVelocity.y + (_gravity * _fallMultiplier * Time.deltaTime);
            float _nextYVelocity = (_previousYVelocity + _newYVelocity) * .5f;
            playerVelocity.y += _nextYVelocity;
        }
        /*else
        {
            float _previousYVelocity = playerVelocity.y;
            float _newYVelocity = playerVelocity.y + (_gravity * Time.deltaTime);
            float _nextYVelocity = (_previousYVelocity + _newYVelocity) * .5f;
            playerVelocity.y += _nextYVelocity;
        }*/

        _characterController.Move(playerVelocity * Time.deltaTime);
    }

    /*private void HandleJump()
    {
        if(!_isJumping && _isGrounded && _jumpPressed)
        {
            _isJumping = true;
            playerVelocity.y = _initialJumpingVelocity * .5f;
            _animator.CrossFadeInFixedTimeEm(_jumpSettings);

        }
        else if(!_jumpPressed && _isJumping && _isGrounded)
        {
            _isJumping = false;
        }
    }*/

    public void OnJump()
    {
        //forward is being pressed
        if (CanClimb())
        {
            Debug.Log("am climbing");
            InitiateClimb();
        }
        else if (_climbing && _isHanging)
        {
            _climbingMove = true;
        }
        /*else if(_isGrounded && !_characterCrouching)
        {
            //playerVelocity.y = _jumpForce;
            _jumpPressed = true;
            Debug.Log("am jumping");
        }*/
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
        switch (_stance)
        {
            case CharacterStance.Standing:
                if(_trackMemoryState._hurtCanTrigger)
                {
                    RequestStanceChange(CharacterStance.Hurt_Crouching);
                }
                else
                {
                    RequestStanceChange(CharacterStance.Crouching);
                }
                break;

            case CharacterStance.Crouching:
                if (_trackMemoryState._hurtCanTrigger)
                {

                    RequestStanceChange(CharacterStance.Hurt_Standing);
                }
                else
                {
                    RequestStanceChange(CharacterStance.Standing);
                }
                break;

            case CharacterStance.Hurt_Standing:
                if (_trackMemoryState._traumaCanTrigger)
                {
                    RequestStanceChange(CharacterStance.Broken_Crouching);
                }
                else
                {
                    RequestStanceChange(CharacterStance.Hurt_Crouching);
                }
                break;

            case CharacterStance.Hurt_Crouching:
                if (_trackMemoryState._traumaCanTrigger)
                {
                    RequestStanceChange(CharacterStance.Broken_Standing);
                }
                else
                {
                    RequestStanceChange(CharacterStance.Hurt_Standing);
                }
                break;

            case CharacterStance.Broken_Standing:
                    RequestStanceChange(CharacterStance.Broken_Crouching);
                break;

            case CharacterStance.Broken_Crouching:
                    RequestStanceChange(CharacterStance.Broken_Standing);
                break;

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
                        //_animator.SetBool("IsCrouching", true);

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
                    //_animator.SetBool("IsCrouching", true);

                    _characterCrouching = true;

                    _runSpeed = _crouchingSpeed.x;
                    _sprintingSpeed = _crouchingSpeed.y;
                    _stance = newStance;
                    _animator.CrossFadeInFixedTime(_hurtCrouchToStand, 0.1f);

                    _maskAnimator.CrossFadeInFixedTime(_hurtCrouchToStand, 0.1f);
                    SetCapsuleDimensions(_crouchingCapsule);

                    return true;
                }
                break;

            case CharacterStance.Crouching:
                if (newStance == CharacterStance.Standing)
                {
                    if (!CharacterOverlap())
                    {
                            _characterCrouching = false;

                            _runSpeed = _standingSpeed.x;
                            _sprintingSpeed = _standingSpeed.y;
                            _stance = newStance;
                            _animator.CrossFadeInFixedTime(_crouchToStand, 0.5f);

                            _maskAnimator.CrossFadeInFixedTime(_crouchToStand, 0.5f);
                            SetCapsuleDimensions(_standingCapsule);

                            return true;
                    }
                }
                else if (newStance == CharacterStance.Hurt_Crouching)
                {
                    //_animator.SetBool("IsCrouching", true);

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
                    //_animator.SetBool("IsCrouching", true);

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
                    //_animator.SetBool("IsCrouching", true);

                    _characterCrouching = true;

                    _runSpeed = _crouchingSpeed.x;
                    _sprintingSpeed = _crouchingSpeed.y;
                    _stance = newStance;
                    _animator.CrossFadeInFixedTime(_brokenCrouchToStand, 0.1f);

                    _maskAnimator.CrossFadeInFixedTime(_brokenCrouchToStand, 0.1f);
                    SetCapsuleDimensions(_crouchingCapsule);

                    return true;
                }
                break;

            case CharacterStance.Hurt_Crouching:
                if (newStance == CharacterStance.Standing)
                {
                    if (!CharacterOverlap())
                    {
                        _characterCrouching = false;

                        _runSpeed = _standingSpeed.x;
                        _sprintingSpeed = _standingSpeed.y;
                        _stance = newStance;
                        _animator.CrossFadeInFixedTime(_hurtCrouchToStand, 0.5f);

                        _maskAnimator.CrossFadeInFixedTime(_hurtCrouchToStand, 0.5f);
                        SetCapsuleDimensions(_standingCapsule);

                        return true;
                    }
                }
                else if (newStance == CharacterStance.Broken_Crouching)
                {
                    //_animator.SetBool("IsCrouching", true);

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
                    //_animator.SetBool("IsCrouching", true);

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
                if (newStance == CharacterStance.Standing)
                {
                    if (!CharacterOverlap())
                    {
                        _characterCrouching = false;

                        _runSpeed = _standingSpeed.x;
                        _sprintingSpeed = _standingSpeed.y;
                        _stance = newStance;
                        _animator.CrossFadeInFixedTime(_brokenCrouchToStand, 0.5f);

                        _maskAnimator.CrossFadeInFixedTime(_brokenCrouchToStand, 0.5f);
                        SetCapsuleDimensions(_standingCapsule);

                        return true;
                    }
                }
                break;
        }

        return false;
    }

    //character crouch check
    private bool CharacterOverlap()
    {
        RaycastHit hit2;
        RaycastHit hit3;

        if (Physics.Raycast(_headRay.transform.position, Vector3.up, out hit2, _maxCastCeiling, mask))
        {
            Debug.DrawLine(_headRay.transform.position, hit2.point, Color.red);

            return true;

            //_shouldBeCrouching = true;
        }
        else if (Physics.Raycast(_headRay2.transform.position, Vector3.up, out hit3, _maxCastCeiling, mask))
        {
            Debug.DrawLine(_headRay2.transform.position, hit3.point, Color.red);
            //_shouldBeCrouching = true;
            return true;
        }
        else
        {
            return false;
            //_shouldBeCrouching = false;
        }
    }

    private void SetCapsuleDimensions(Vector3 dimensions)
    {
        _characterController.center = new Vector3(_characterController.center.x, dimensions.z, _characterController.center.z);
        _characterController.radius = dimensions.x;
        _characterController.height = dimensions.y;
    }


    private IEnumerator stepRoot()
    {
        yield return new WaitForSeconds(.58f);
        _animator.applyRootMotion = true;
        yield return new WaitForSeconds(.1f);
        _animator.applyRootMotion = true;
    }


    public void OnSMBEvent(string eventName)
    {
        switch (eventName)
        {
            case "StandToFreeHangEnter":
                _animator.MatchTarget(_matchTargetPosition, _matchTargetRotation, AvatarTarget.Root, _weightMask, .3f, .65f);
                _maskAnimator.MatchTarget(_matchTargetPosition, _matchTargetRotation, AvatarTarget.Root, _weightMask, .3f, .65f);
                break;
            case "ClimbUpEnter":
                _animator.MatchTarget(_matchTargetPosition, _matchTargetRotation, AvatarTarget.Root, _weightMask, 0f, .9f);
                _maskAnimator.MatchTarget(_matchTargetPosition, _matchTargetRotation, AvatarTarget.Root, _weightMask, 0f, .9f);
                break;
            case "VaultEnter":
                _animator.MatchTarget(_matchTargetPosition, _matchTargetRotation, AvatarTarget.Root, _weightMask, 0f, .65f);
                _maskAnimator.MatchTarget(_matchTargetPosition, _matchTargetRotation, AvatarTarget.Root, _weightMask, 0f, .65f);
                break;
            case "StepUpEnter":
                _animator.MatchTarget(_matchTargetPosition, _matchTargetRotation, AvatarTarget.Root, _weightMask, .58f, .68f);
                _maskAnimator.MatchTarget(_matchTargetPosition, _matchTargetRotation, AvatarTarget.Root, _weightMask, .58f, .68f);
                break;
            case "DropEnter":
                _animator.MatchTarget(_matchTargetPosition, _matchTargetRotation, AvatarTarget.Root, _weightMask, .2f, .5f);
                _maskAnimator.MatchTarget(_matchTargetPosition, _matchTargetRotation, AvatarTarget.Root, _weightMask, .2f, .5f);
                break;

            case "StandToFreeHangExit":
                _hangRoutine = StartCoroutine(HangingRoutine());
                break;
            case "ClimbUpExit":
            case "VaultExit":
            case "StepUpExit":
            case "DropExit":
                _climbing = false;
                _characterController.enabled = true;
                _animator.applyRootMotion = false;
                _maskAnimator.applyRootMotion = false;
                //rb is kinematic = false;
                break;
            case "DropToAir":
                _climbing = false;
                _characterController.enabled = true;
                _animator.applyRootMotion = false;
                _maskAnimator.applyRootMotion = false;
                //rb is kinematic = false;
                break;

        }
    }

    private IEnumerator HangingRoutine()
    {
        //wait for input
        while (!_isHanging)
            yield return null;

        //climb up
        if (_climbingMove)
        {
            _matchTargetPosition = _endPosition;
            _matchTargetRotation = _forwardNormalXZRotation;

            _animator.rootPosition = transform.position;
            _animator.rootRotation = transform.rotation;
            _animator.applyRootMotion = true;
            _animator.CrossFadeInFixedTimeEm(_climbUpSettings);

            _maskAnimator.rootPosition = transform.position;
            _maskAnimator.rootRotation = transform.rotation;
            _maskAnimator.applyRootMotion = true;
            _maskAnimator.CrossFadeInFixedTimeEm(_climbUpSettings);
        }

        //drop down
        if (_dropDown)
        {
            if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit _hitInfo, _dropCheckDistance, _layerMaskClimb))
            {
                _animator.CrossFadeEm(_dropToAirSettings);

                _maskAnimator.CrossFadeEm(_dropToAirSettings);
            }
            else
            {
                _matchTargetPosition = _hitInfo.point + _forwardNormalXZRotation * _dropOffset;
                _matchTargetRotation = _forwardNormalXZRotation;
                _animator.CrossFadeInFixedTimeEm(_dropSettings);

                _maskAnimator.CrossFadeInFixedTimeEm(_dropSettings);
            }
        }

        _climbingMove = false;
        _dropDown = false;
        _hangRoutine = null;

    }
}
