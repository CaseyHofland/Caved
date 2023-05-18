#nullable enable
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;
using UnityExtras;
using UnityExtras.InputSystem;

/// <summary>Handle spline controlled movement through input.</summary>
[AddComponentMenu("Physics/Spline Controlled Character")]
[RequireComponent(typeof(CharacterMover))]
[DisallowMultipleComponent]
public class SplineControlledCharacter : CharacterInputBase<CharacterMover>
{
    protected override void MovePerformed(InputAction.CallbackContext context)
    {
        if (container == null)
        {
            return;
        }

        var direction2D = context.ReadRevalue<Vector2>();
        var sprint = sprintReaction.reaction ?? false;
        var length = container.CalculateLength();
        var targetSpeed = characterMover.moveSpeed + (sprint ? characterMover.sprintBoost : 0f);
        time += targetSpeed / length * Mathf.Sign(direction2D.y) * Time.deltaTime;
    }

    [field: Header("Spline")]
    [field: SerializeField] public SplineContainer? container { get; set; }

    [SerializeField, HideInInspector] private float _time;
    public float time
    {
        get => _time;
        set
        {
            if (container == null)
            {
                return;
            }

            var currentPosition = container.EvaluatePosition(_time);
            characterMover.transform.position = currentPosition;

            var newPosition = container.EvaluatePosition(_time = Mathf.Clamp01(value));
            var direction = math.normalizesafe(newPosition - currentPosition);

            characterMover.Move(direction, sprintReaction.reaction ?? false);
            characterMover.TurnTowards(direction);
        }
    }
}
