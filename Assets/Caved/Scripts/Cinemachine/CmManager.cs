using UnityEngine;
using Unity.Cinemachine;

public class CmManager : MonoBehaviour
{
    public CinemachineCamera[] _vCams;

    private void Start()
    {
        _vCams[0].Priority.Value = 0;
    }
}
