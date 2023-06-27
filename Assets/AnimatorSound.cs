using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorSound : MonoBehaviour
{
    public void PlaySound(string eventName)
    {
        FMODUnity.RuntimeManager.PlayOneShot($"event:/{eventName}", transform.position);
    }
}
