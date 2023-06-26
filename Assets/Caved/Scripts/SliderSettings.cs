using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MBSO
{
    [Serializable]
    public class SliderSettings
    {
        public Slider _slider;
        public TMP_Text _txtSlider;
        [Tooltip("The actual maximum value for your settins. The user will not see this!")]
        public float _maxSettingsValue;
        [Tooltip("The actual minimum value for your settins. The user will not see this!")]
        public float _minSettingsValue;

    }
}

