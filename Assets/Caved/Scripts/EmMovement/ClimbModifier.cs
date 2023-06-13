using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbModifier : MonoBehaviour
{
    [SerializeField] private bool _climable;

    public bool Climable { get=>_climable; set=>_climable = value; }
}
