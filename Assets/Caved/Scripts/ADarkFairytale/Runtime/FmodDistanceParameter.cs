using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FmodDistanceParameter : MonoBehaviour
{
    public Transform Target;
    public EmitterRef Ref;
    public float min;
    public float max;

    void Update()
    {
        var distance = (Target.position - transform.position).magnitude;
        var value = Mathf.InverseLerp(min,max,distance);
        if (Ref.Target.EventInstance.isValid())
        {
            Ref.Target.EventInstance.setParameterByName(Ref.Params[0].Name,value);
        }
    }

}
