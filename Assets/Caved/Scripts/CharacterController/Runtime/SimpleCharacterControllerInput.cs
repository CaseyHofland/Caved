using KinematicCharacterController;
using UnityEngine;
using UnityEngine.InputSystem;

using Input = UnityExtras.InputSystem.Input;

[RequireComponent(typeof(SimpleCharacterController))]
public class SimpleCharacterControllerInput : MonoBehaviour
{
    private SimpleCharacterController _myCharacterController;
    private KinematicCharacterMotor _motor;

    public Input moveInput;
    public Input sprintInput;
    public Input jumpInput;
    public Transform orbitter;

    private bool _isJumping;

    public SimpleCharacterController GetMyCharacterController() => GetComponent<SimpleCharacterController>();

    private void Reset()
    {
        orbitter = Camera.main.transform;
    }

    private void Start()
    {
        _myCharacterController = GetMyCharacterController();
        _motor = _myCharacterController.GetKinematicCharacterMotor();
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        HandleCharacterMovement();
        HandleCharacterSprint();
        HandleCharacterJump();
    }

    private void HandleCharacterJump()
    {
        if (jumpInput.action.WasReleasedThisFrame())
        {
            _isJumping = false;
        }

        if (_isJumping)
        {
            _myCharacterController.Jump();
        }

        if (jumpInput.action.WasPressedThisFrame())
        {
            _isJumping = true;
            _myCharacterController.ForcedJump();
        }
    }

    private void HandleCharacterSprint()
    {
        if (sprintInput.action.WasReleasedThisFrame())
        {
            _myCharacterController.sprintInput = false;
        }

        if (sprintInput.action.WasPressedThisFrame())
        {
            _myCharacterController.sprintInput = true;
        }
    }

    private void HandleCharacterMovement()
    {
        // Collect input data.
        var movementRaw = moveInput.action.ReadValue<Vector2>();
        var movement = new Vector3(movementRaw.x, 0f, movementRaw.y);
        var orbit = orbitter.rotation;

        // Calculate camera direction and rotation on the character plane.
        var cameraPlanarDirection = Vector3.ProjectOnPlane(orbit * Vector3.forward, _motor.CharacterUp).normalized;
        if (cameraPlanarDirection.sqrMagnitude == 0f)
        {
            cameraPlanarDirection = Vector3.ProjectOnPlane(orbit * Vector3.up, _motor.CharacterUp).normalized;
        }
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, _motor.CharacterUp);

        // Apply inputs.
        _myCharacterController.moveInput = cameraPlanarRotation * movement;
        _myCharacterController.lookInput = cameraPlanarDirection;
    }
}
