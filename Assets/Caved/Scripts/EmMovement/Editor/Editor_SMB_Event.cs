using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.Build.Content;

[CustomEditor(typeof(SMB_Event))]
public class Editor_SMB_Event : Editor
{
    private SerializedProperty m_totalFrames;
    private SerializedProperty m_currentFrames;
    private SerializedProperty m_normalizedTime;
    private SerializedProperty m_normalizedTimeUncapped;
    private SerializedProperty m_motionTime;
    private SerializedProperty m_events;
    private ReorderableList m_eventList;

    private void OnEnable()
    {
        m_totalFrames = serializedObject.FindProperty("m_totalFrames");
        m_currentFrames = serializedObject.FindProperty("m_currentFrames");
        m_normalizedTime = serializedObject.FindProperty("m_normalizedTime");
        m_normalizedTimeUncapped = serializedObject.FindProperty("m_normalizedTimeUncapped");
        m_motionTime = serializedObject.FindProperty("m_motionTime");
        m_events = serializedObject.FindProperty("Events");
        m_eventList = new ReorderableList(serializedObject, m_events, true, true, true, true);

        m_eventList.drawHeaderCallback = DrawHeaderCallback;
        m_eventList.drawElementCallback = DrawElementCallback;
        m_eventList.elementHeightCallback = ElementHeightCallback;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        using (new EditorGUI.IndentLevelScope(1))
        {
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject((SMB_Event)target), typeof(SMB_Event), false);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(m_totalFrames);
                EditorGUILayout.PropertyField(m_currentFrames);
                EditorGUILayout.PropertyField(m_normalizedTime);
                EditorGUILayout.PropertyField(m_normalizedTimeUncapped);
            }

            EditorGUILayout.PropertyField(m_motionTime);

        }

        m_eventList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHeaderCallback(Rect rect)
    {
        EditorGUI.LabelField(rect, "Events");

    }

    private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty _element = m_eventList.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty _eventName = _element.FindPropertyRelative("eventName");
        SerializedProperty _timing = _element.FindPropertyRelative("timing");

        string _elementTitle;
        int _timingIndex = _timing.enumValueIndex;
        _elementTitle = string.IsNullOrEmpty(_eventName.stringValue) ?
            $"Event *Name* ({_timing.enumDisplayNames[_timingIndex]})" :
            $"Event {_eventName.stringValue} ({_timing.enumDisplayNames[_timingIndex]})";

        EditorGUI.PropertyField(rect, _element, new GUIContent(_elementTitle), true);
    }

    private float ElementHeightCallback(int index)
    {
        SerializedProperty _element = m_eventList.serializedProperty.GetArrayElementAtIndex(index);
        float _propertyHeight = EditorGUI.GetPropertyHeight(_element, true);
        return _propertyHeight;
    }
}
