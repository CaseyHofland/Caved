#nullable enable
using KinematicCharacterController;
using UnityEngine;

[RequireComponent(typeof(KinematicCharacterMotor))]
public class GenericCharacterController : MonoBehaviour, ICharacterController
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [field: SerializeField, HideInInspector] public KinematicCharacterMotor motor { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public Vector3 deltaMovement { get; set; }
    public Quaternion deltaRotation { get; set; } = Quaternion.identity;

    protected virtual void InitializeComponents()
    {
        motor = GetComponent<KinematicCharacterMotor>();
    }

    protected void Reset()
    {
        InitializeComponents();
    }

    protected void Awake()
    {
        InitializeComponents();
    }

    protected void Start()
    {
        motor.CharacterController = this;
    }

    public virtual void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        currentRotation = deltaRotation * currentRotation;
    }

    public virtual void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        // Ground movement
        if (motor.GroundingStatus.IsStableOnGround)
        {
            if (deltaTime > 0f)
            {
                // The final velocity is the velocity from root motion reoriented on the ground plane
                currentVelocity = deltaMovement / deltaTime;
                currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;
            }
            else
            {
                // Prevent division by zero
                currentVelocity = Vector3.zero;
            }
        }
    }

    public virtual void BeforeCharacterUpdate(float deltaTime)
    {
    }

    public virtual void PostGroundingUpdate(float deltaTime)
    {
    }

    public virtual void AfterCharacterUpdate(float deltaTime)
    {
        deltaMovement = Vector3.zero;
        deltaRotation = Quaternion.identity;
    }

    public virtual bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public virtual void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public virtual void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public virtual void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    public virtual void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }
}
