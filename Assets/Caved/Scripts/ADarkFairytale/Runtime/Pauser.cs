#nullable enable
using System;
using UnityEngine;

[Obsolete("Fixer Script", false)]
public class Pauser : MonoBehaviour
{
    public void SetTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
    }
}
