using KinematicCharacterController;
using UnityEngine;
using UnityExtras;

[RequireComponent(typeof(KinematicCharacterMotor))]
public class SimpleCharacterController : MonoBehaviour, ICharacterController
{
    [field: Header("Move")]
    [field: SerializeField][field: Tooltip("Move speed of the character in m/s")][field: Min(0f)] public float moveSpeed { get; set; } = 10f;
    [field: SerializeField][field: Tooltip("Sprint boost of the character in m/s")][field: Min(0f)] public float sprintBoost { get; set; } = 10f;
    [field: SerializeField][field: Tooltip("Move speed acceleration and deceleration")][field: Min(0.1f)] public float moveSnapRate { get; set; } = 15f;
    [field: SerializeField][field: Tooltip("Turn speed acceleration and deceleration")][field: Min(0.1f)] public float turnSnapRate { get; set; } = 10f;

    [field: Header("Air Move")]
    [field: SerializeField][field: Tooltip("Move speed of the character in m/s")][field: Min(0f)] public float airMoveSpeed { get; set; } = 4f;
    [field: SerializeField][field: Tooltip("Sprint boost of the character in m/s")][field: Min(0f)] public float airSprintBoost { get; set; } = 0f;
    [field: SerializeField][field: Tooltip("Acceleration and deceleration")][field: Min(0.1f)] public float airSpeedChangeRate { get; set; } = 10f;
    [field: SerializeField, Tooltip("Drag while airborn"), Min(0f)] public float airDrag { get; set; } = 0.1f;

    [field: Header("Jump")]
    [field: SerializeField][field: Tooltip("The height the character can jump in m")][field: Min(0f)] public float jumpHeight { get; set; } = 1.2f;
    [field: SerializeField][field: Tooltip("Time to reach the peak of the jump: if 0, this value is ignored and gravity scale is used instead")][field: Min(0f)] public float peakTime { get; set; } = 0f;
    [field: SerializeField][field: Tooltip("How much this body is affected by gravity")] public float gravityScale { get; set; } = 1f;
    [field: SerializeField][field: Tooltip("How much extra gravity is applied to end the character's jump early")][field: Min(1f)] public float fastFallRatio { get; set; } = 3f;
    [field: SerializeField][field: Tooltip("Delay to allow jumping after becoming ungrounded")][field: Min(0f)] public float coyoteTime { get; set; } = 0.15f;
    [field: SerializeField][field: Tooltip("Buffer to allow jumping before becoming grounded")][field: Min(0f)] public float jumpBuffer { get; set; } = 0.2f;
    [field: SerializeField][field: Tooltip("Allow the character to jump perpetually")] public bool allowWallJump { get; set; }
    [field: SerializeField][field: Tooltip("Allow the character to jump perpetually")] public bool allowPerpetualJump { get; set; }

    public Vector3 moveInput { get; set; }
    public Vector3 lookInput { get; set; }
    public bool sprintInput { get; set; }

    private KinematicCharacterMotor _motor;

    private const float noiseBuffer = 0.05f;
    public const float terminalVelocity = 53.0f;

    private Vector3 _jumpVelocity;
    private float _currentFastFallBuffer;
    private float _currentCoyoteTime;
    private float _currentJumpBuffer;
    private float _jumpBufferJumpHeight;
    private float _currentCanJumpBuffer;

    private bool _fastFalling => _currentFastFallBuffer < 0f;
    private float _jumpGravityScale => peakTime > 0f
        ? (2f * jumpHeight) / (peakTime * peakTime) / _gravityForce
        : gravityScale;
    private bool _hasJumpingSurface => allowWallJump ? _motor.GroundingStatus.FoundAnyGround : _motor.GroundingStatus.IsStableOnGround;
    private float _currentGravityScale => _hasJumpingSurface || !_isJumping
        ? gravityScale
        : _jumpGravityScale * (_fastFalling ? fastFallRatio : 1f);
    private Vector3 _gravity => _currentGravityScale * Physics.gravity;
    private float _currentGravityForce => _currentGravityScale * _gravityForce;
    private bool _isJumping => Vector3.Dot(_jumpVelocity, Physics.gravity) < 0;

    private float _gravityForce => Physics.gravity.magnitude;
    private Vector3 _gravityDirection => Physics.gravity.normalized;

    public KinematicCharacterMotor GetKinematicCharacterMotor() => GetComponent<KinematicCharacterMotor>();

    public virtual void Jump() => Jump(jumpHeight);
    public virtual void Jump(float jumpHeight)
    {
        if (allowPerpetualJump || _currentCanJumpBuffer > 0f)
        {
            // Jump or activate the jump buffer.
            if (_hasJumpingSurface || _currentCoyoteTime > 0f)
            {
                _jumpVelocity = ExtraMath.JumpVelocity(jumpHeight, _gravityDirection, _jumpGravityScale * _gravityForce);
                _motor.ForceUnground();
                _currentFastFallBuffer = 0f;
                _currentCoyoteTime = 0f;
                _currentJumpBuffer = 0f;
            }
            else
            {
                _currentJumpBuffer = jumpBuffer;
                _jumpBufferJumpHeight = jumpHeight;
            }
        }

        // Reset the fast fall buffer and can jump buffer.
        if (!_fastFalling)
        {
            _currentFastFallBuffer = noiseBuffer + Time.deltaTime;
        }
        _currentCanJumpBuffer = -noiseBuffer - Time.deltaTime;
    }

    public virtual void ForcedJump() => ForcedJump(jumpHeight);
    public virtual void ForcedJump(float jumpHeight)
    {
        _currentCanJumpBuffer = float.PositiveInfinity;
        _currentCoyoteTime = float.PositiveInfinity;
        Jump(jumpHeight);
    }

    private void Start()
    {
        _motor = GetComponent<KinematicCharacterMotor>();
        _motor.CharacterController = this;
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (lookInput != Vector3.zero && turnSnapRate > 0f)
        {
            // Smoothly interpolate from current to target look direction
            var smoothedLookInputDirection = Vector3.Slerp(_motor.CharacterForward, lookInput, 1f - Mathf.Exp(-turnSnapRate * deltaTime)).normalized;

            // Set the current rotation (which will be used by the KinematicCharacterMotor)
            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, _motor.CharacterUp);
        }
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        // Handle movement.
        currentVelocity = _motor.GroundingStatus.IsStableOnGround
            ? HandleGroundVelocity(currentVelocity, deltaTime)
            : HandleAirVelocity(currentVelocity, deltaTime);

        // Handle jumping.
        if (_isJumping)
        {
            currentVelocity += _jumpVelocity - Vector3.Project(currentVelocity, _motor.CharacterUp);
        }

        Vector3 HandleGroundVelocity(Vector3 currentVelocity, float deltaTime)
        {
            // Reorient source velocity on current ground slope (this is because we don't want our smoothing to cause any velocity losses in slope changes)
            currentVelocity = _motor.GetDirectionTangentToSurface(currentVelocity, _motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

            // Calculate target velocity
            var inputRight = Vector3.Cross(moveInput, _motor.CharacterUp);
            var reorientedInput = Vector3.Cross(_motor.GroundingStatus.GroundNormal, inputRight).normalized * moveInput.magnitude;
            var targetMovementVelocity = reorientedInput * ((sprintInput ? sprintBoost : default) + moveSpeed);

            // Smooth movement velocity
            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-moveSnapRate * deltaTime));
            return currentVelocity;
        }

        Vector3 HandleAirVelocity(Vector3 currentVelocity, float deltaTime)
        {
            if (moveInput.sqrMagnitude > 0f)
            {
                var targetMovementVelocity = moveInput * ((sprintInput ? airSprintBoost : default) + airMoveSpeed);

                // Prevent climbing on un-stable slopes with air movement
                if (_motor.GroundingStatus.FoundAnyGround)
                {
                    var perpendicularObstructionNormal = Vector3.Cross(Vector3.Cross(_motor.CharacterUp, _motor.GroundingStatus.GroundNormal), _motor.CharacterUp).normalized;
                    targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpendicularObstructionNormal);
                }

                var velocityDifference = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, _gravityDirection);
                currentVelocity += airSpeedChangeRate * deltaTime * velocityDifference;
            }

            // Gravity
            currentVelocity += _gravity * deltaTime;

            // Air drag
            currentVelocity *= (1f / (1f + (airDrag * deltaTime)));
            return currentVelocity;
        }
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
    }

    public void PostGroundingUpdate(float deltaTime)
    {
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        if (_hasJumpingSurface)
        {
            _jumpVelocity = Vector3.zero;
        }
        else
        {
            // Apply gravity over time if under terminal velocity (multiply by delta time twice to linearly speed up over time).
            var gravityDeltaSpeed = _currentGravityForce * Time.deltaTime;
            var factor = Mathf.Clamp(terminalVelocity - _jumpVelocity.magnitude, -gravityDeltaSpeed, gravityDeltaSpeed);
            _jumpVelocity += _gravityDirection * factor;
        }

        UpdateJumpSettings();

        void UpdateJumpSettings()
        {
            // Update coyote time, jump buffer and fast fall buffer.
            if (_hasJumpingSurface)
            {
                _currentCoyoteTime = coyoteTime;
                if (_currentJumpBuffer > 0f)
                {
                    ForcedJump(_jumpBufferJumpHeight);
                }
            }
            else
            {
                _currentCoyoteTime -= Time.deltaTime;
            }

            _currentJumpBuffer -= Time.deltaTime;
            _currentFastFallBuffer -= Time.deltaTime;
            _currentCanJumpBuffer += Time.deltaTime;
        }
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }
}
