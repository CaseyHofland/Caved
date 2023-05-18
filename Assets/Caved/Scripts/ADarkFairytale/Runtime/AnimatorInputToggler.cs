using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Obsolete]
public class AnimatorInputToggler : MonoBehaviour
{
    public InputActionAsset inputActions;

    public void EnableInput() => inputActions.Enable();
    public void DisableInput() => inputActions.Disable();
}
