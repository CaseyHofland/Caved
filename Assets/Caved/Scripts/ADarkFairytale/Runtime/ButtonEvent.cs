#nullable enable
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

using Input = UnityExtras.InputSystem.Input;

public class ButtonEvent : MonoBehaviour
{
    [field: SerializeField] public Input input { get; set; }
    [field: SerializeField] public UnityEvent<bool> onPerformed { get; set; } = new();

    private void OnEnable()
    {
        if (input.action != null)
        {
            input.action.performed += OnPerformed;
        }
    }

    private void OnDisable()
    {
        if (input.action != null)
        {
            input.action.performed -= OnPerformed;
        }
    }

    private void OnPerformed(InputAction.CallbackContext context)
    {
        onPerformed?.Invoke(context.ReadValueAsButton());
    }
}
