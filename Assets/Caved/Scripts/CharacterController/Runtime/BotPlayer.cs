#nullable enable
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityExtras;
using UnityExtras.InputSystem;

[RequireComponent(typeof(BotCharacterController))]
public class BotPlayer : MonoBehaviour
{
    private const float turnMultiplier = 1f / 180f;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [field: SerializeField, HideInInspector] public BotCharacterController bot { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [field: SerializeField] public Transform? orientationPoint { get; set; }

    [field: Header("Input")]
    [field: SerializeField] public InputReaction moveReaction { get; set; }

    protected virtual void InitializeComponents()
    {
        bot = GetComponent<BotCharacterController>();
    }

    private void Reset()
    {
        InitializeComponents();
        orientationPoint = Camera.main.transform;
    }

    private void Awake()
    {
        InitializeComponents();
    }

    private void OnEnable()
    {
        if (moveReaction.reaction != null)
        {
            moveReaction.reaction.performed += MoveReaction;
            moveReaction.input.action!.canceled += MoveCanceled;
        }
    }

    private void OnDisable()
    {
        if (moveReaction.reaction != null)
        {
            moveReaction.reaction.performed -= MoveReaction;
            moveReaction.input.action!.canceled -= MoveCanceled;
        }
    }

    private void MoveReaction(InputAction.CallbackContext context)
    {
        if (!bot.isMoving.value)
        {
            bot.animator.SetFloat(bot.move.parameterName, 0f);
            bot.animator.SetFloat(bot.turn.parameterName, 0f);
        }

        bot.isMoving.value = true;
        //bot.animator.SetBool(bot.isMovingHash, true);

        var movement = context.ReadRevalue<Vector2>();
        bot.move.value = movement.magnitude;
        //bot.animator.SetFloat(bot.moveHash, movement.magnitude, bot.moveDampTime, Time.deltaTime);

        var turnAngle = ExtraMath.Angle(new Vector2(movement.y, movement.x));
        var turnRotation = Quaternion.AngleAxis(turnAngle, Vector3.up);

        CalculatePlanarOrientation(orientationPoint != null ? orientationPoint.transform.rotation : Quaternion.identity, bot.motor.CharacterUp, out var planarDirection, out var planarRotation);
        var targetRotation = planarRotation * turnRotation;
        turnAngle = Vector3.SignedAngle(bot.motor.TransientRotation * bot.motor.CharacterForward, targetRotation * bot.motor.CharacterForward, bot.motor.CharacterUp);

        bot.turn.value = turnAngle * turnMultiplier;
    }

    private void MoveCanceled(InputAction.CallbackContext context)
    {
        bot.isMoving.value = false;
    }

    [Obsolete("Find better place for this method", false)]
    private static void CalculatePlanarOrientation(in Quaternion rotation, in Vector3 planeNormal, out Vector3 planarDirection, out Quaternion planarRotation)
    {
        // Calculate camera direction and rotation on the character plane
        planarDirection = Vector3.ProjectOnPlane(rotation * Vector3.forward, planeNormal).normalized;
        if (planarDirection.sqrMagnitude == 0f)
        {
            planarDirection = Vector3.ProjectOnPlane(rotation * Vector3.up, planeNormal).normalized;
        }
        planarRotation = Quaternion.LookRotation(planarDirection, planeNormal);
    }
}
