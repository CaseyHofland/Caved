using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lookAtCam : MonoBehaviour
{
    [SerializeField] GameObject _object;


    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - _object.transform.position);
    }
}
