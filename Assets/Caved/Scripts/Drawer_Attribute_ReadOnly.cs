using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MBSO
{
    //[CustomPropertyDrawer(typeof(Attribute_ReadOnly))]
    public class Drawer_Attribute_ReadOnly : MonoBehaviour
    {
        /*public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }*/
    }
}

