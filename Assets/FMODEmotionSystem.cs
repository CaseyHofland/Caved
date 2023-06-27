using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation.Samples;
using UnityEngine;
using UnityEngine.UI;

public class FMODEmotionSystem : MonoBehaviour
{
    public FMOD.Studio.EventInstance instance;

    public FMODUnity.EventReference fmodEvent;

    [Header("Memory count")]
    public int PositiveMemoriesScore;
    public int NegativeMemoriesScore;

    [SerializeField] [Range (-10f, 10f)]
    public float FMODEmotion;


    void Start()
    {
        //instance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);
        //instance.setParameterByName("FMODEmotion", FMODEmotion);
       // FMODUnity.RuntimeManager.StudioSystem.setParameterByName("FMODEmotion", FMODEmotion);
        //i//nstance.start();
    }

    void Update()
    {
        FMODEmotion = PositiveMemoriesScore - NegativeMemoriesScore;
        //instance.setParameterByName("FMODEmotion", FMODEmotion);
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("FMODEmotion", FMODEmotion);
        //Debug.Log(FMODEmotion);
    }

  
}
    