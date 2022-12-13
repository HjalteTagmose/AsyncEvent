using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IdentityModel.Claims;
using System.Linq;
using System;

namespace AsyncEvent.Editor
{
    [CustomPropertyDrawer(typeof(AsyncEvent))]
    public class AsyncEventEditor : PropertyDrawer
    {
        private ReorderableList list;
        private bool initialized = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Properties
            var callsProp    = property.FindPropertyRelative("calls");
            var callTypeProp = property.FindPropertyRelative("type");
            float width = position.width;

            // Initialize
            if (!initialized)
            {
                this.list = new ReorderableList(property.serializedObject, callsProp, true, true, true, true);
                initialized = true;
            }

            // Begin
            var gui = EditorGUI.BeginProperty(position, label, property);
            var labelWidth = EditorStyles.label.CalcSize(label).x + 30;

            // List
            list.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, label);
                };
            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    rect.height += 20;
                    EditorGUIUtility.labelWidth = 1;
                    EditorGUI.PropertyField(rect, callsProp.GetArrayElementAtIndex(index));
                };
            list.elementHeightCallback = _ => 40f;
            list.DoList(position);

            // Dropdown
            float w = position.width; float y = position.y;
            position.x += w - 101; position.y += 1;
            position.height = 20; position.width = 100;
            EditorGUI.PropertyField(position, callTypeProp, GUIContent.none);

            // Dropdown tip
            string tip = "NaN";
            switch ((AsyncEventType)callTypeProp.enumValueIndex)
            {
                case AsyncEventType.WhenAll:     tip = "Calls all at once. Continues when all finished."; break;
                case AsyncEventType.Sequence:    tip = "Calls one after another. Continues when all finished."; break;
                case AsyncEventType.Synchronous: tip = "Calls all at once. Continues instantly."; break;
            }
            GUIStyle tipStyle = new GUIStyle();
            tipStyle.normal.textColor = Color.gray;
            tipStyle.alignment = TextAnchor.MiddleRight;
            tipStyle.clipping = TextClipping.Clip;
            position.y -= 1; position.height = 20; 
            position.width = width - labelWidth - 90;
            position.x = labelWidth;
            GUI.Label(position, tip, tipStyle);

            // End
            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return initialized ? list.GetHeight() : 40;
        }
    }
}