using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace AsyncEvent.Editor
{
    [CustomPropertyDrawer(typeof(AsyncEventType))]
    public class AsyncEventTypeEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, GUIContent.none);
        }
    }
}