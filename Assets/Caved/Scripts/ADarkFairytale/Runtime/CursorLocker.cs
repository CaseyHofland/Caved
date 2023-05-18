#nullable enable
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CursorLocker : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void SubsystemRegistration()
    {
        cursorLockers.Clear();
    }

    private static List<CursorLocker> cursorLockers = new();
    public static CursorLocker? current => cursorLockers.Count > 0 ? cursorLockers[0] : null;

    [field: SerializeField] public CursorLockMode lockState { get; set; } = CursorLockMode.Locked;
    [field: SerializeField] public bool visible { get; set; } = true;

    private void OnEnable()
    {
        cursorLockers.Insert(0, this);
        SetCursor();
    }

    private void OnDisable()
    {
        cursorLockers.Remove(this);
        if (current != null)
        {
            current.SetCursor();
        }
    }

    public void SetCursor()
    {
        Cursor.lockState = lockState;
        Cursor.visible = visible;
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus && enabled && current == this)
        {
            SetCursor();
        }
    }
}
