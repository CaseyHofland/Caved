using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventEvoker : MonoBehaviour
{
    public UnityEvent _eventGo;

    public void StartTheEvent()
    {
        _eventGo.Invoke();

    }
}
