using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;
using UnityExtras;

public enum CharacterStance { Standing, Crouching }
public class EmMovement : MonoBehaviour
{
    [Header("Assets")]
    public CharacterController _characterController;
    public Animator _animator;
    private CharacterStance _stance;

    [Header("Movement")]
    private float yVelocity = 0.0f;
    private float smoothTime = 0.3f;
    public bool _canIMove = true;

    private Vector3 playerVelocity;

    [Header("Speed")]
    public float _speed; //movement speed
    [SerializeField] private Vector2 _standingSpeed = new Vector2(0,0);
    public float _sprintingSpeed;
    public float _crawlingSpeed;

    public float _turnSmoothTime = 0.1f;
    float _turnSmoothVelocity;

    public AnimatorHash _move;

    [Header("Capsule (Radius, Height, YOffset")]
    [SerializeField]private Vector3 _standingCapsule = Vector3.zero;
    [SerializeField] private Vector3 _crouchingCapsule = Vector3.zero;

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
    [SerializeField] private LayerMask _layerMaskClimb;

    private bool _climbing;

    private Vector3 _endPosition;

    private RaycastHit _downRaycastHit;
    private RaycastHit _forwardRaycastHit;

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

    [Header("Animation settings")]
    public CrossFadeSettingsEm _standToFreeHandSettings;
    public CrossFadeSettingsEm _climbUpSettings;
    public CrossFadeSettingsEm _vaultSettings;
    public CrossFadeSettingsEm _dropSettings;
    public CrossFadeSettingsEm _dropToAirSettings;
    public CrossFadeSettingsEm _stepUpSettings;

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

        //set defaults
        SetCapsuleDimensions(_standingCapsule);

        int _mask = 0;
        for (int i = 0; i < 32; i++)
            if(!(Physics.GetIgnoreLayerCollision(gameObject.layer, i)))
                _mask |= 1 << i;

        _layerMask = _mask;
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
        Debug.Log(_stance);

        if (!_climbing)
        {
            //WALKING
            Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized; //walking around
            if (move.magnitude > 0)
                _characterController.Move(move * Time.deltaTime * _speed);
            else
                _characterController.Move(Vector3.zero);

            float vertical = Input.GetAxis("Vertical");
            float horizontal = Input.GetAxis("Horizontal");
            var characterMovement = new Vector3(horizontal, 0, vertical); //walking around


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
            else if (_characterCrouching)
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

            /*
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
            }*/

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


    }

    RaycastHit downRaycastHit;
    RaycastHit forwardRaycastHit;
    Vector3 endPosition;

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

        Vector3 _endPosition;
        Vector3 _forwardDirectionXZ;
        Vector3 _forwardNormalXZ;

        Vector3 _downDirection = Vector3.down;
        Vector3 _downOrigin = transform.TransformPoint(_climbOriginDown);

        _downHit = Physics.Raycast(_downOrigin, _downDirection, out _downRaycastHit, _climbOriginDown.y - _stepHeight, _layerMaskClimb);
        
        //Debug.DrawLine(_downOrigin.transform.position, downRaycastHit.point, Color.yellow);

        if (_downHit)
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
            else
            {
                return false;
            }

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
        _characterController.enabled = false;
        //Rigidbody.isKinematic = true;

        float _climbHeight = downRaycastHit.point.y - transform.position.y;
        Vector3 _forwardNormalXZ = Vector3.ProjectOnPlane(_forwardRaycastHit.normal, Vector3.up);
        _forwardNormalXZRotation = Quaternion.LookRotation(-_forwardNormalXZ, Vector3.up);

        if (_climbHeight > _hangHeight)
        {
            _matchTargetPosition = forwardRaycastHit.point + _forwardNormalXZRotation * _hangOffset;
            _matchTargetRotation = _forwardNormalXZRotation;
            _animator.CrossFadeInFixedTimeEm(_standToFreeHandSettings);
            Debug.Log("hanging");//never called


        }
        else if (_climbHeight > _climbUpHeight)
        {
            _matchTargetPosition = _endPosition;
            _matchTargetRotation = _forwardNormalXZRotation;
            _animator.CrossFadeInFixedTimeEm(_climbUpSettings);

            Debug.Log("climb up");
        }
        else if (_climbHeight > _vaultHeight)
        {
            _matchTargetPosition = _endPosition;
            _matchTargetRotation = _forwardNormalXZRotation;

            _animator.CrossFadeInFixedTimeEm(_vaultSettings);
            Debug.Log("vault");
        }
        else if (_climbHeight > _stepHeight)
        {
            _matchTargetPosition = _endPosition;
            _matchTargetRotation = _forwardNormalXZRotation;

            _animator.CrossFadeInFixedTimeEm(_stepUpSettings);
            Debug.Log("steppng");
        }
        else
        {
            _characterController.enabled = true;
            _climbing = false;
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
        //forward is being pressed
        if (CanClimb())
        {
            Debug.Log("am climbing");
            InitiateClimb();
        }
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
        /*Vector3 crouchPos = new Vector3(0, _crouchingHeight, 0);
        Vector3 normalPos = new Vector3(0, _normalHeight, 0);*/
        Debug.Log("a");
        switch (_stance)
        {
            case CharacterStance.Standing:
                RequestStanceChange(CharacterStance.Crouching);
               break;

            case CharacterStance.Crouching:
                RequestStanceChange(CharacterStance.Standing);
                break;
        }

        /*if (!_characterCrouching)
        {
            _animator.SetBool("IsCrouching", true);

            _characterCrouching = true;

            _characterController.height = _crouchingHeight; //height character controller
            _characterController.center = _crouchingCenter; //center character controller
        }
        else if (_characterCrouching && !_shouldBeCrouching)
        {
            _animator.SetBool("IsCrouching", false);
            _characterCrouching = false;

            _characterController.height = _normalHeight; //height character controller
            _characterController.center = _normalCenter; //center character controller
        }*/
    }

    public bool RequestStanceChange(CharacterStance newStance)
    {
        if (_stance == newStance)
            return true;

        switch(_stance)
        {
            case CharacterStance.Standing:
                if(newStance == CharacterStance.Crouching)
                {
                        _animator.SetBool("IsCrouching", true);

                        _characterCrouching = true;

                        SetCapsuleDimensions(_crouchingCapsule);

                        /*_characterController.height = _crouchingHeight; //height character controller
                        _characterController.center = _crouchingCenter; //center character controller
                        */
                        Debug.Log("crouch");
                        _stance = CharacterStance.Crouching;

                        return true;
                }
                break;

            case CharacterStance.Crouching:
                if (newStance == CharacterStance.Standing)
                {
                    if (!CharacterOverlap())
                    {
                        _animator.SetBool("IsCrouching", false);

                        _characterCrouching = false;

                        SetCapsuleDimensions(_standingCapsule);

                        /*_characterController.height = _normalHeight; //height character controller
                        _characterController.center = _normalCenter; //center character controller
                        */
                        Debug.Log("stand");
                        _stance = CharacterStance.Standing;

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
        /*
        float _radius = dimensions.x;
        float _height = dimensions.y;
        Vector3 _center = new Vector3(_characterController.center.x, dimensions.z, _characterController.center.z);

        Vector3 _point0;
        Vector3 _point1;
        if(_height<_radius * 2)
        {
            _point0 = transform.position + _center;
            _point1 = transform.position + _center;
        }
        else
        {
            _point0= transform.position + _center + (transform.up * (_height * .5f - _radius));
            _point1 = transform.position + _center - (transform.up * (_height * .5f - _radius));
        }

        int _numOverlaps = Physics.OverlapCapsuleNonAlloc(_point0, _point1, _radius, _obstructions, _layerMask);
        for(int i = 0; i<_numOverlaps; i++)
        {
            if (_obstructions[1] == _characterController)
            {
                _numOverlaps--;
                
            }
        }
        return _numOverlaps > 0;
        */

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
}
