using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class countingMemories : MonoBehaviour
{
    public int _count = 0;
    [SerializeField] private int _max;
    [SerializeField] 
    Animator _animator;

    private void Start()
    {
        _max = FindObjectsOfType<MemoryTrigger>().Count();
    }

    // Update is called once per frame
    void Update()
    {
        if (_count >= _max) _animator.SetBool("isClear", true);
    }
}
