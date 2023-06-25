using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBSO
{
    [Serializable]
    public class SettingsFile
    {
        //Video Settings
        public int screenWidth;
        public int screenHeight;
        public string displayType;
        public float brightness;

        public SettingsFile(SO_Settings set) 
        {
            screenWidth = set.widthValue; 
            screenHeight = set.heightValue; 
            displayType = set.displayTypeValue;
            brightness = set.brightnessValue;
        }

    }
}

