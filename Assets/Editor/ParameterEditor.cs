using System;
using UnityEditor;
using UnityEngine;

namespace AsyncEvent
{
    [CustomPropertyDrawer(typeof(Parameter))]
    public class ParameterEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
        }
    }
}