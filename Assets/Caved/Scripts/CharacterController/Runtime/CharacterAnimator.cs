using System;
using UnityEngine;
using UnityExtras;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterMover))]
[RequireComponent(typeof(CharacterCroucher))]
[Obsolete]
public class CharacterAnimator : MonoBehaviour
{
    [field: SerializeField, HideInInspector] public Animator animator { get; private set; }
    [field: SerializeField, HideInInspector] public CharacterMover characterMover { get; private set; }
    [field: SerializeField, HideInInspector] public CharacterCroucher characterCroucher { get; private set; }

    public AnimatorHash move;
    public AnimatorHash jump;
    public AnimatorHash isGrounded;
    public AnimatorHash isCrouching;
    public AnimatorHash isOnSlope;
    public float onSlopeLimit = 45f;

    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        characterMover = GetComponent<CharacterMover>();
        characterCroucher = GetComponent<CharacterCroucher>();
    }

    private void Reset()
    {
        InitializeComponents();
    }

    private void Awake()
    {
        InitializeComponents();
    }

    private void LateUpdate()
    {
        var xzVelocity = characterMover.characterController.velocity;
        xzVelocity.y = 0;
        var speed = xzVelocity.magnitude;
        var moveSpeed = characterMover.moveSpeed;

        animator.SetFloat(move, moveSpeed > 0f ? ExtraMath.Round(speed / moveSpeed, 6) : 0f);
        if (characterMover.isJumping)
        {
            animator.SetTrigger(jump);
        }
        else
        {
            animator.ResetTrigger(jump);
        }
        animator.SetBool(isGrounded, characterMover.characterController.isGrounded);
        animator.SetBool(isCrouching, characterCroucher.isCrouching);

        bool isSlopeWalking;
        if (isSlopeWalking = Physics.Raycast(transform.position, -transform.up, out var hit, characterMover.characterController.radius + characterMover.characterController.skinWidth + Vector3.kEpsilon, ExtraPhysics.GetLayerCollisionMask(gameObject.layer), QueryTriggerInteraction.Ignore))
        {
            var angle = Vector3.Angle(Vector3.up, hit.normal);
            isSlopeWalking = (angle > onSlopeLimit && angle <= characterMover.characterController.slopeLimit);
        }
        animator.SetBool(isOnSlope, isSlopeWalking);
    }
}
