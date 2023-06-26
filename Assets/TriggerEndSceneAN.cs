using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEndSceneAN : MonoBehaviour
{
    public Animator _zimAnimator;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) _zimAnimator.SetTrigger("canWake");

        Debug.Log("waking up");
    }
}
