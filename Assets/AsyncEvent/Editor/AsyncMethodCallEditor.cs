using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;
using Component = UnityEngine.Component;

namespace AsyncEvent.Editor
{
	[CustomPropertyDrawer(typeof(AsyncMethodCall))]
	public class AsyncMethodCallEditor : PropertyDrawer
	{
		#region ViewData
		private class ViewData
        {
            public int index;
            public string[] options;
            public List<MethodInfo> methods;
        }

		private static Dictionary<string, ViewData> propertyData = new ();
		#endregion

		#region Properties
		// Properties
		private SerializedProperty objProp;
        private SerializedProperty compProp;
        private SerializedProperty methodProp;
        private SerializedProperty isAsyncProp;
        private SerializedProperty paramProp;
        private SerializedProperty paramCountProp;
		
        // Parameter types
        private SerializedProperty paramIntProp;
        private SerializedProperty paramFloatProp;
        private SerializedProperty paramStringProp;
        private SerializedProperty paramBoolProp;
        private SerializedProperty paramGameObjProp;
        private SerializedProperty paramComponentProp;
        private SerializedProperty paramTypeProp;
        #endregion

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// Get ViewData
			var data = GetViewData(property.propertyPath);

			// Get all properties
			GetProperties(property);

            // Label
			label = EditorGUI.BeginProperty(position, label, property);
			position = EditorGUI.PrefixLabel(position, label);
            position.height = 20;
			data.index = EditorGUI.IntField(position, data.index);
            EditorGUI.EndProperty();
		}

		#region Get Data
		private void GetProperties(SerializedProperty property)
        {			
			objProp				= property.FindPropertyRelative("obj");
			compProp			= property.FindPropertyRelative("component");
			methodProp			= property.FindPropertyRelative("method");
            isAsyncProp			= property.FindPropertyRelative("isAsync");
            paramProp			= property.FindPropertyRelative("param");
            paramCountProp		= property.FindPropertyRelative("paramCount");
        }

        private void GetParamProperties(SerializedProperty property)
        {
            paramIntProp		= paramProp.FindPropertyRelative("intValue");
			paramFloatProp		= paramProp.FindPropertyRelative("floatValue");
			paramStringProp		= paramProp.FindPropertyRelative("stringValue");
			paramBoolProp		= paramProp.FindPropertyRelative("boolValue");
			paramGameObjProp	= paramProp.FindPropertyRelative("gameObjValue");
            paramComponentProp	= paramProp.FindPropertyRelative("componentValue");
            paramTypeProp		= paramProp.FindPropertyRelative("typeValue");
        }

        private ViewData GetViewData(string key)
        {
            ViewData data;
            if (!propertyData.TryGetValue(key, out data))
            {
                data = new ViewData();
                propertyData.Add(key, data);
            }
            return data;
		}

		public static void ClearDatas()
		{
            propertyData.Clear();
		}
		#endregion

		#region GUI
		private void ShowParamGUI(Rect position, Type type)
        {
			if (type.IsSubclassOf(typeof(Component)))
			{
				paramComponentProp.objectReferenceValue = EditorGUI.ObjectField(position, "Value", paramComponentProp.objectReferenceValue, type, true); 
                return;
			}

            string t = type.Name.ToLower();
            switch (t)
            {
                case "int32":		paramIntProp	.intValue			  = EditorGUI.IntField   (position, "Value", paramIntProp	 .intValue   ); break;
                case "single":		paramFloatProp	.floatValue			  = EditorGUI.FloatField (position, "Value", paramFloatProp  .floatValue ); break;
                case "string":		paramStringProp	.stringValue		  = EditorGUI.TextField  (position, "Value", paramStringProp .stringValue); break;
                case "boolean":		paramBoolProp	.boolValue			  = EditorGUI.Toggle     (position, "Value", paramBoolProp   .boolValue  ); break;
                case "gameobject":	paramGameObjProp.objectReferenceValue = EditorGUI.ObjectField(position, "Value", paramGameObjProp.objectReferenceValue, typeof(GameObject), true); break;
                default: EditorGUI.LabelField(position, "Unsupported value type: " + t); break;
            }
        }

		#endregion
	}
}