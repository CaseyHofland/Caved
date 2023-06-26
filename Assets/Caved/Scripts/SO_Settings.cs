using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBSO
{
    [CreateAssetMenu(menuName = "SO/Settings")]
    public class SO_Settings : ScriptableObject
    {
        [Header("Video Settings")]
        [Attribute_ReadOnly] public int widthValue;
        [Attribute_ReadOnly] public int heightValue;
        [Attribute_ReadOnly] public string displayTypeValue;
        public float brightnessValue;
    }

}
