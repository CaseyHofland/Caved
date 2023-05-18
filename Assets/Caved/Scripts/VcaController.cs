using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VcaController : MonoBehaviour
{
    private FMOD.Studio.VCA _VcaController;

    public string _VcaName; //write the name of the needed VCA in the inspector in Unity

    private Slider _slider;

    [SerializeField] private float _VcaVolume;


    void Start()
    {
        _VcaController = FMODUnity.RuntimeManager.GetVCA("vca:/" + _VcaName);
        _slider = GetComponent<Slider>();
        _VcaController.getVolume(out _VcaVolume);
    }

    public void OnEnable()
    {
        Debug.Log("slider selected");
    }

    public void SetVolume(float volume)
    {
        _VcaController.setVolume(volume);
        _VcaController.getVolume(out _VcaVolume);
    }
}
