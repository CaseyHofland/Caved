using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class faceCam : MonoBehaviour
{
    private Transform _mLookAt;
    public GameObject _findCamera;
    private Transform _localTrans;

    private void Start()
    {
        _localTrans = GetComponent<Transform>();
        _findCamera = GameObject.FindGameObjectWithTag("Main Camera");
        _mLookAt = _findCamera.transform;
    }

    private void Update()
    {
        if(_mLookAt)
        {
            _localTrans.LookAt(2*_localTrans.position - _mLookAt.position);
        }
    }
}
