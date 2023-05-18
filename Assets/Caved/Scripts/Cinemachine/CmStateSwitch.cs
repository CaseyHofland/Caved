using Unity.Cinemachine;
using UnityEngine;

public class CmStateSwitch : MonoBehaviour
{
    public CinemachineCamera _oldState;
    public CinemachineCamera _newState;
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            _oldState.Priority.Value = 10;
            _newState.Priority.Value = 20;
        }
    }
}
