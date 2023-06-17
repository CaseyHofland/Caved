using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class countingMemories : MonoBehaviour
{
    public int _count = 0;
    [SerializeField] private int _max;
    Animator _animator;

    // Update is called once per frame
    void Update()
    {
        if (_count >= _max) _animator.SetBool("isClear", true);
    }
}
