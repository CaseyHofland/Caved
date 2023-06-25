using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class triggerCanvas : MonoBehaviour
{
    private MemoryTrigger _thisMemoryObject;
    [SerializeField] private GameObject _ZanaLines;

    private void Update()
    {
        if(_thisMemoryObject._lookClicked)
        {
            _ZanaLines.SetActive(true);

        }
    }
}
