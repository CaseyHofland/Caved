using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityExtras;
using Input = UnityExtras.InputSystem.Input;

[Obsolete]
public class SlopeClimbTrigger : MonoBehaviour
{
    [Tag] public string playerTag;
    [field: SerializeField] public Input climbInput { get; set; }
    [field: SerializeField] public Canvas canvas { get; set; }
    [field: SerializeField] public float climbingSlopeCheck { get; set; } = 45f;
    [field: SerializeField] public float climbingSlopeLimit { get; set; } = 60f;

    private CharacterController characterController;

    private void Awake()
    {
        canvas.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)
            || other is not CharacterController characterController)
        {
            return;
        }

        this.characterController = characterController;
        EnableAction();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        DisableAction();
    }

    void EnableAction()
    {
        climbInput.action.performed += ClimbSlope;
        canvas.enabled = true;
    }

    void DisableAction()
    {
        climbInput.action.performed -= ClimbSlope;
        canvas.enabled = false;
    }

    private void ClimbSlope(InputAction.CallbackContext context)
    {
        StartCoroutine(Coroutine());

        IEnumerator Coroutine()
        {
            yield return null;

            var slopeClimber = characterController.gameObject.AddComponent<SlopeClimber>();
            slopeClimber.slopeLimit = climbingSlopeCheck;
            slopeClimber.onDestroy += () => gameObject.SetActive(true);

            characterController.slopeLimit = climbingSlopeLimit;
            characterController.transform.SetPositionAndRotation(transform.position, transform.rotation);

            DisableAction();
            gameObject.SetActive(false);
        }
    }

    [DisallowMultipleComponent]
    private class SlopeClimber : MonoBehaviour
    {
        public event Action onDestroy;
        public float slopeLimit;

        private CharacterController controller => croucher.characterMover.characterController;
        private CharacterCroucher croucher;

        private void Awake()
        {
            croucher = GetComponent<CharacterCroucher>();
            croucher.SetCrouching(true);
        }

        private void FixedUpdate()
        {
            bool isSlopeWalking;
            controller.enabled = false;
            if (isSlopeWalking = Physics.Raycast(transform.position, -transform.up, out var hit, controller.radius + controller.skinWidth + Vector3.kEpsilon, ExtraPhysics.GetLayerCollisionMask(gameObject.layer), QueryTriggerInteraction.Ignore))
            {
                var angle = Vector3.Angle(Vector3.up, hit.normal);
                isSlopeWalking = (angle > slopeLimit && angle <= controller.slopeLimit);
            }
            controller.enabled = true;

            if (!isSlopeWalking)
            {
                Destroy(this);
            }
        }

        private void OnDestroy()
        {
            croucher.SetCrouching(false);
            onDestroy?.Invoke();
        }
    }
}
