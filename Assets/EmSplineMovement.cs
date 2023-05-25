using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;
using UnityExtras;
using UnityExtras.InputSystem;
using Unity.Mathematics;

public class EmSplineMovement : MonoBehaviour
{
    private EmMovement _characterMovementScript;

    private void Start()
    {
        _characterMovementScript = GetComponent<EmMovement>();
    }
    
    /*protected override void MovePerformed2(InputAction.CallbackContext context)
    {
        if (container2 == null)
        {
            return;
        }

        var direction2D = context.ReadRevalue<Vector2>();
        var sprint = sprintReaction2.reaction ?? false;
        var length = container2.CalculateLength();
        var targetSpeed = _characterMovementScript._speed + (sprint ? _characterMovementScript._sprintingSpeed : 0f);
        time += targetSpeed / length * Mathf.Sign(direction2D.y) * Time.deltaTime;
    }*/

    [field: Header("Spline")]
    [field: SerializeField] public SplineContainer? container2 { get; set; }

    [SerializeField, HideInInspector] private float _time;
    public float time
    {
        get => _time;
        set
        {
            if (container2 == null)
            {
                return;
            }

            var currentPosition = container2.EvaluatePosition(_time);
            _characterMovementScript._characterController.transform.position = currentPosition;

            var newPosition = container2.EvaluatePosition(_time = Mathf.Clamp01(value));
            var direction = math.normalizesafe(newPosition - currentPosition);

            //_characterMovementScript._characterController.Move(direction, sprintReaction2.reaction ?? false);

            //characterMover.Move(direction, sprintReaction.reaction ?? false);
            //_characterMovementScript._characterController.TurnTowards(direction);
        }
    }
}
