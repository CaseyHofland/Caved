#nullable enable
using UnityEngine;
using UnityExtras;

[RequireComponent(typeof(Animator))]
public class BotCharacterController : GenericCharacterController
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [field: SerializeField, HideInInspector] public Animator animator { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [field: Header("Animator Parameters")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [field: SerializeField] public FloatParameter move { get; private set; }
    [field: SerializeField] public FloatParameter turn { get; private set; }
    [field: SerializeField] public BoolParameter isMoving { get; private set; }
    [field: SerializeField] public AnimatorHash isGrounded { get; private set; }
    [field: SerializeField] public AnimatorHash isCrouching { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [field: Header("Prototype Settings")]
    [field: SerializeField, Min(0.001f)] public float halfRotationTime { get; set; } = 1f;
    [field: SerializeField, Min(0.001f)] public float turnMin { get; set; } = 0.125f;
    [field: SerializeField, Min(0.001f)] public float halfRotationTimeQuick { get; set; } = 0.1f;
    [field: SerializeField, Min(0.001f)] public float turnMax { get; set; } = 0.925f;
    [field: SerializeField] public uint turnPrecision { get; set; } = 2;

    [field: Header("Crouch Settings")]
    [field: SerializeField, Range(0f, 180f)] public float crouchAngle { get; set; } = 30f;
    [field: SerializeField, Min(0f)] public float crouchOrientationSharpness { get; set; } = 20f;

    private Coroutine? moveCoroutine;
    private Coroutine? turnCoroutine;

    protected override void InitializeComponents()
    {
        base.InitializeComponents();
        animator = GetComponent<Animator>();
    }

    protected void OnEnable()
    {
        move.onValueChanged += MoveChanged;
        isMoving.onValueChanged += IsBoolChanged;
        turn.onValueChanged += TurnChanged;

        move.value = 0f;
        isMoving.value = false;
        turn.value = 0f;
    }

    protected void OnDisable()
    {
        move.onValueChanged -= MoveChanged;
        isMoving.onValueChanged -= IsBoolChanged;
        turn.onValueChanged -= TurnChanged;
    }

    private void MoveChanged(FloatParameter parameter)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(animator.SetFloatCoroutine(parameter));
    }

    private void TurnChanged(FloatParameter parameter)
    {
        if (turnCoroutine != null)
        {
            StopCoroutine(turnCoroutine);
        }
        turnCoroutine = StartCoroutine(animator.SetFloatCoroutine(parameter));
    }

    private void IsBoolChanged(BoolParameter parameter)
    {
        animator.SetBool(parameter);
    }

    public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        base.UpdateRotation(ref currentRotation, deltaTime);

        if (animator.GetBool(isCrouching))
        {
            // Multiply the rotation of the ground by the current upwards rotation.
            var groundRotation = Quaternion.LookRotation(Vector3.forward, motor.GroundingStatus.InnerGroundNormal); 
            groundRotation *= Quaternion.AngleAxis(currentRotation.eulerAngles.y, Vector3.up);

            currentRotation = Quaternion.Slerp(currentRotation, groundRotation, 1f - Mathf.Exp(-crouchOrientationSharpness * deltaTime));
        }
    }

    public override void BeforeCharacterUpdate(float deltaTime)
    {
        base.BeforeCharacterUpdate(deltaTime);

        motor.gravityOrientationSharpness = animator.GetBool(isCrouching) ? 0f : 20f;
        crouchOrientationSharpness = (!animator.GetBool(isCrouching) || animator.GetBool(isMoving)) ? 20f : 0f;
    }

    public override void PostGroundingUpdate(float deltaTime)
    {
        animator.SetBool(isGrounded, motor.GroundingStatus.IsStableOnGround);

        if (!animator.GetBool(isMoving))
        {
            return;
        }

        var gravityUp = motor.gravityUpOverride != null
            ? Quaternion.FromToRotation(-Physics.gravity.normalized, motor.gravityUpOverride.up) * Vector3.forward
            : -Physics.gravity.normalized;

        var angle = Vector3.Angle(motor.GroundingStatus.InnerGroundNormal, gravityUp);
        animator.SetBool(isCrouching, motor.GroundingStatus.IsStableOnGround && angle >= crouchAngle && motor.BaseVelocity.y > 0f);
    }

    protected void OnAnimatorMove()
    {
        // Accumulate rootMotion deltas between character updates
        deltaMovement += animator.deltaPosition;
        deltaRotation = animator.deltaRotation * deltaRotation;

        // Add Prototype Rotating to the character
        if (deltaMovement.sqrMagnitude > 0f
            && animator.deltaRotation.eulerAngles.sqrMagnitude == 0f)
        {
            var currentTurn = animator.GetFloat(turn);
            currentTurn = ExtraMath.Round(currentTurn, turnPrecision);

            float turnAngle;
            if (Mathf.Abs(currentTurn) >= turnMax)
            {
                turnAngle = Mathf.Sign(currentTurn) * 180f * Time.deltaTime / halfRotationTimeQuick;
            }
            else
            {
                turnAngle = Mathf.Sign(currentTurn) * Mathf.Min(Mathf.Abs(currentTurn) / turnMin, 1f);
                turnAngle *= 180f * Time.deltaTime / halfRotationTime;
            }

            var turnRotation = Quaternion.AngleAxis(turnAngle, motor.CharacterUp);
            deltaRotation = turnRotation * deltaRotation;
        }
    }
}
