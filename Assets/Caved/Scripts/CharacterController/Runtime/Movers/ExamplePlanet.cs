using KinematicCharacterController;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class ExamplePlanet : MonoBehaviour, IMoverController
{
    public PhysicsMover PlanetMover;
    public SphereCollider GravityField;
    public float gravityScale = 0.4f;
    public Vector3 OrbitAxis = Vector3.forward;
    public float OrbitSpeed = 10;

    public CharacterTeleporter OnPlaygroundTeleportingZone;
    public CharacterTeleporter OnPlanetTeleportingZone;

    private Quaternion _lastRotation;

    private struct SavedMotorData
    {
        public GameObject gravityUpGameObject;
        public Transform gravityUpOverride;
        public float gravityScale;
    }

    private Dictionary<KinematicCharacterMotor, SavedMotorData> savedData = new();

    private void Start()
    {
        OnPlaygroundTeleportingZone.onCharacterTeleport -= ControlGravity;
        OnPlaygroundTeleportingZone.onCharacterTeleport += ControlGravity;

        OnPlanetTeleportingZone.onCharacterTeleport -= UnControlGravity;
        OnPlanetTeleportingZone.onCharacterTeleport += UnControlGravity;

        _lastRotation = PlanetMover.transform.rotation;

        PlanetMover.MoverController = this;
    }

    public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
    {
        goalPosition = PlanetMover.Rigidbody.position;

        // Rotate
        Quaternion targetRotation = Quaternion.Euler(OrbitAxis * OrbitSpeed * deltaTime) * _lastRotation;
        goalRotation = targetRotation;
        _lastRotation = targetRotation;
    }

    void ControlGravity(KinematicCharacterMotor motor)
    {
        GameObject gravityUpOverride = new($"{motor.name} Gravity Up Override");
        gravityUpOverride.transform.SetParent(transform, false);

        var aimConstraint = gravityUpOverride.AddComponent<AimConstraint>();
        aimConstraint.aimVector = Vector3.up;
        aimConstraint.upVector = Vector3.forward;
        aimConstraint.AddSource(new ConstraintSource()
        {
            sourceTransform = motor.transform,
            weight = 1f,
        });
        aimConstraint.constraintActive = true;

        savedData.Add(motor, new SavedMotorData()
        {
            gravityUpGameObject = gravityUpOverride,
            gravityUpOverride = motor.gravityUpOverride,
            gravityScale = motor.gravityScale,
        });

        motor.gravityUpOverride = gravityUpOverride.transform;
        motor.gravityScale = gravityScale;
    }

    void UnControlGravity(KinematicCharacterMotor motor)
    {
        savedData.Remove(motor, out var saveData);

        Destroy(saveData.gravityUpGameObject);
        motor.gravityUpOverride = saveData.gravityUpOverride;
        motor.gravityScale = saveData.gravityScale;
    }
}
