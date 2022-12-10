using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

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

            // Initialize
            if (!initialized)
            {
                this.list = new ReorderableList(property.serializedObject, callsProp, true, true, true, true);
                initialized = true;
            }

            // Begin
            EditorGUI.BeginProperty(position, label, property);

            // List
            list.drawHeaderCallback =
                (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, label);
                };
            list.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    EditorGUIUtility.labelWidth = 1;
                    EditorGUI.PropertyField(rect, callsProp.GetArrayElementAtIndex(index));
                };
            list.DoList(position);

            // Dropdown
            float w = position.width; float y = position.y;
            position.x += w - 101; position.width = 100; position.y += 1;
            EditorGUI.PropertyField(position, callTypeProp, GUIContent.none);
            //EditorGUI.EnumPopup(position, (AsyncEventType)callTypeProp.enumValueIndex);

            // Dropdown tip
            string tip = "NaN";
            switch ((AsyncEventType)callTypeProp.enumValueIndex)
            {
                case AsyncEventType.WaitAll: tip = "Calls all at once. Continues when all finished."; break;
                case AsyncEventType.Sequenced: tip = "Calls one after another. Continues when all finished."; break;
                case AsyncEventType.Synchronous: tip = "Calls all at once. Continues instantly."; break;
            }
            GUIStyle tipStyle = new GUIStyle();
            tipStyle.normal.textColor = Color.gray;
            tipStyle.alignment = TextAnchor.MiddleRight;
            position.height = 20; position.y -= 1; position.width -= 105;
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

        public static void DrawUIBox(Color borderColor, Color backgroundColor, Rect rect, RectOffset border)
        {
            Rect outer = new Rect(rect);
            Rect inner = new Rect(rect.x + border.left,
                                  rect.y + border.top,
                                  rect.width - (border.left + border.right),
                                  rect.height - (border.top + border.bottom));
            EditorGUI.DrawRect(outer, borderColor);
            EditorGUI.DrawRect(inner, backgroundColor);
        }
    }
}