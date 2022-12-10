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
            var jsonProp = property.FindPropertyRelative("json");
            var typeProp = property.FindPropertyRelative("type");

            return;
            string json = jsonProp.stringValue;
            string type = typeProp.stringValue;

            return;
            object value = GetValue(json, type);

            switch (value)
            {
                case int i:         value = EditorGUI.IntField(position, "Value", i);   break;
                case float f:       value = EditorGUI.FloatField(position, "Value", f); break;
                case string s:      value = EditorGUI.TextField(position, "Value", s);  break;
                case bool b:        value = EditorGUI.Toggle(position, "Value", b);     break;
                case GameObject go: value = EditorGUI.ObjectField(position, "Value", go, typeof(GameObject), true);       break;
                case Component c:   value = EditorGUI.ObjectField(position, "Value", c, typeof(Component), true);         break;
                default:                    EditorGUI.LabelField(position, "Unsupported value type: " + value.GetType()); break;
            }

            type = value.GetType().ToString();
            json = JsonUtility.ToJson(value);

            jsonProp.stringValue = json;
            typeProp.stringValue = type;
        }

        private object GetValue(string json, string type) => 
            JsonUtility.FromJson(json, Type.GetType(type));
    }
}