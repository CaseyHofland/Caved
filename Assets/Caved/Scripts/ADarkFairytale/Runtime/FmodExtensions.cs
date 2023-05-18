using FMOD;
using FMOD.Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

using Debug = UnityEngine.Debug;

public static class FmodExtensions
{
    public static bool TryGetUserData<T>(this EventInstance instance, out T userData)
    {
        try
        {
            instance.getUserData(out var userDataPtr);
            var userDataHandle = GCHandle.FromIntPtr(userDataPtr);
            userData = (T)userDataHandle.Target;
            return true;
        }
        catch (Exception e)
        {
            if (e is not ArgumentException && e is not InvalidOperationException)
            {
                Debug.LogError(e.Message);
            }

            userData = default;
            return false;
        }
    }
}
