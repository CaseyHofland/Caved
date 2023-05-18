using FMODUnity;
using UnityEngine;

public class FmodDistanceParameter : MonoBehaviour
{
    [field: Header("Fmod")]
    [field: SerializeField] public StudioEventEmitter emitter { get; set; }
    [field: SerializeField] public string parameter { get; set; }

    [field: Header("Target")]
    [field: SerializeField] public Transform target { get; set; }
    [field: SerializeField] public float minDistance { get; set; }
    [field: SerializeField] public float maxDistance { get; set; }

    void Update()
    {
        if (emitter == null || !emitter.EventInstance.isValid())
        {
            return;
        }

        var distance = (target.position - transform.position).magnitude;
        var value = Mathf.InverseLerp(minDistance, maxDistance, distance);
        emitter.EventInstance.setParameterByName(parameter, value);
    }
}
