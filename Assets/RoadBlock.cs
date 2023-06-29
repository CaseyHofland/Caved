using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RoadBlock : MonoBehaviour
{
    public int count;
    public int countNeeded;
    public UnityEvent events;

    public void AddCount()
    {
        count++;
        CheckState();
    }

    public void CheckState()
    {
        if(count >= countNeeded)
        {
            events.Invoke();
        }
    } 
}
