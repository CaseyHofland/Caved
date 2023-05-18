using System;
using Unity.RuntimeSceneSerialization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityExtras;
using Input = UnityExtras.InputSystem.Input;

[RequireComponent(typeof(CharacterMover))]
public class CharacterCroucher : MonoBehaviour
{
    [field: SerializeField, HideInInspector] public CharacterMover characterMover { get; private set; }
    private CharacterController characterController => characterMover.characterController;

    [field: Header("Default State")]
    [field: SerializeField] public TextAsset controllerDefaultState { get; set; }
    [field: SerializeField] public TextAsset moverDefaultState { get; set; }

    [field: Header("Crouch State")]
    [field: SerializeField] public TextAsset controllerCrouchState { get; set; }
    [field: SerializeField] public TextAsset moverCrouchState { get; set; }

    [field: Header("Input")]
    [field: SerializeField] public Input crouchInput { get; set; }

    public bool isCrouching { get; private set; }

    private void InitializeComponents()
    {
        characterMover = GetComponent<CharacterMover>();
    }

    private void Reset()
    {
        InitializeComponents();
    }

    private void Awake()
    {
        InitializeComponents();
    }

    public bool TrySetCrouching(bool crouch)
    {
        if (!characterController.isGrounded)
        {
            return false;
        }

        var text = crouch ? controllerCrouchState.text : controllerDefaultState.text;
        var radius = Parse<float>(text, nameof(CharacterController.radius));
        var height = Parse<float>(text, nameof(CharacterController.height));
        var center = height * 0.5f * Vector3.up;
        var start = transform.position + center - (height * 0.5f - radius) * Vector3.up;
        var end = transform.position + center + (height * 0.5f - radius) * Vector3.up;

        characterController.enabled = false;
        bool canCrouch = !Physics.CheckCapsule(start, end, radius - Physics.defaultContactOffset, ExtraPhysics.GetLayerCollisionMask(gameObject.layer), QueryTriggerInteraction.Ignore);
        characterController.enabled = true;

        if (canCrouch)
        {
            SetCrouching(crouch);
        }
        return canCrouch;

        // This is a BAAAAAD fixer script, do not reuse!!!!!
        T Parse<T>(string text, string line)
        {
            var startIndex = text.IndexOf($"\"{line}\": ");
            startIndex += line.Length + 4;
            var endIndex = text.IndexOfAny(new[] { ',', '\n' }, startIndex);

            text = text[startIndex..endIndex];
            return (T)Convert.ChangeType(text, typeof(T));
        }
    }

    public void SetCrouching(bool crouch)
    {
        var controller = this.characterController;
        var mover = characterMover;
        if (crouch)
        {
            SceneSerialization.FromJsonOverride(controllerCrouchState.text, ref controller);
            SceneSerialization.FromJsonOverride(moverCrouchState.text, ref mover);
        }
        else
        {
            SceneSerialization.FromJsonOverride(controllerDefaultState.text, ref controller);
            SceneSerialization.FromJsonOverride(moverDefaultState.text, ref mover);
        }
        controller.center = controller.height * 0.5f * Vector3.up;

        isCrouching = crouch;
    }

    private void OnEnable()
    {
        if (crouchInput.action != null)
        {
            crouchInput.action.performed += ToggleCrouching;
        }
    }

    private void OnDisable()
    {
        if (crouchInput.action != null)
        {
            crouchInput.action.performed -= ToggleCrouching;
        }
    }

    private void ToggleCrouching(InputAction.CallbackContext context)
    {
        TrySetCrouching(!isCrouching);
    }
}
