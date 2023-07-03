using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CrouchTrigger : MonoBehaviour
{
    public UnityEvent _crouchEvent;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            _crouchEvent.Invoke();
            Debug.Log("Triggered");
        }
    }
}
