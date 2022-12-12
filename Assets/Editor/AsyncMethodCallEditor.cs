using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;
using Component = UnityEngine.Component;
using Unity.VisualScripting;
using System;
using static PlasticPipe.PlasticProtocol.Messages.NegotiationCommand;

namespace AsyncEvent
{
	[CustomPropertyDrawer(typeof(AsyncMethodCall))]
	public class AsyncMethodCallEditor : PropertyDrawer
	{
		private List<MethodInfo> methods;
		private int idx = 0, old = 0;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// Properties
			var objProp				= property.FindPropertyRelative("obj");
			var compProp			= property.FindPropertyRelative("component");
			var methodProp			= property.FindPropertyRelative("method");
            var isAsyncProp			= property.FindPropertyRelative("isAsync");
            var paramProp			= property.FindPropertyRelative("param");
            var paramCountProp		= property.FindPropertyRelative("paramCount");

            // Parameter types
            var paramIntProp		= paramProp.FindPropertyRelative("intValue");
			var paramFloatProp		= paramProp.FindPropertyRelative("floatValue");
			var paramStringProp		= paramProp.FindPropertyRelative("stringValue");
			var paramBoolProp		= paramProp.FindPropertyRelative("boolValue");
			var paramGameObjProp	= paramProp.FindPropertyRelative("gameObjValue");
            var paramComponentProp	= paramProp.FindPropertyRelative("componentValue");
            var paramTypeProp		= paramProp.FindPropertyRelative("typeValue");

            // Label
            float width = position.width;
			label = EditorGUI.BeginProperty(position, label, property);
			position = EditorGUI.PrefixLabel(position, label);
			
			// Object field
			position.height = 18f;
			position.width = width * 0.3f;
			EditorGUI.indentLevel = 0;
			EditorGUI.PropertyField(position, objProp, GUIContent.none);

			// Stop if there is no object
			if (objProp.objectReferenceValue == null)
			{
				EditorGUI.EndProperty();
				return;
			}

			// Get object, methods & options
			var objValue = (GameObject)objProp.objectReferenceValue;
			GetMethods(objValue);
            var options = methods.Select(m => GetFormattedName(m)).ToArray();

			// Find idx
			idx = old = GetIndex();

            // Dropdown
            position.x += position.width + 5;
            position.width = width * 0.7f;
            position.width -= 8;
			idx = EditorGUI.Popup(position, idx, options);

            // If picked new option, update call
            if (idx != old)
            {
                Component component = null;
                var selected = methods[idx];
                old = idx;

				// Get param info
                var paramsArr = methods[idx]?.GetParameters();
                int paramCount = paramsArr == null ? 0 : paramsArr.Length;

                // Reset all param props here
                paramIntProp.intValue = 0;
                paramFloatProp.floatValue = 0f;
                paramStringProp.stringValue = "";
                paramBoolProp.boolValue = false;
                paramComponentProp.objectReferenceValue = null;
                paramGameObjProp.objectReferenceValue = null;
                paramCountProp.intValue = paramCount;
                paramTypeProp.stringValue = paramCount > 0 ? paramsArr[0].ParameterType.Name : "";

                if (selected == null)
                {
                    methodProp.stringValue = "None";
                    compProp.objectReferenceValue = null;
                    EditorGUI.EndProperty();
                    return;
                }

				bool isObj   = selected.ReflectedType == typeof(GameObject);
				bool hasComp = !isObj && objValue.TryGetComponent(selected.ReflectedType, out component);

				if (hasComp)
				{
					compProp.objectReferenceValue = component;
					methodProp.stringValue = selected.Name;
					isAsyncProp.boolValue = selected.ReturnType == typeof(Task);
				}
				else if (isObj)
				{
					compProp.objectReferenceValue = null;
					methodProp.stringValue = selected.Name;
					isAsyncProp.boolValue = false;
				}
			}

			// Params
			if (HasParams())
			{
                position.y += 20;
                Type pType = methods[idx].GetParameters()[0].ParameterType;
				ShowParamGUI(position, pType);

                /*
                position.y += 20; //this doesnt extend size of actual item?
				string paramType = methods[idx].GetParameters()[0].ParameterType.ToString();
				if (paramTypeProp.stringValue != paramType)
				{
					paramTypeProp.stringValue = paramType;
					paramJsonProp.stringValue = GetDefaultValueJson(paramType);
                }

                string json = paramJsonProp.stringValue;
				string type = paramTypeProp.stringValue;
                object value = JsonUtility.FromJson(json, Type.GetType(type));

				value = ShowParamGUI(position, value);
				json = EditorJsonUtility.ToJson(value);

				if (value is string && !string.IsNullOrEmpty((string)value))
					Debug.Break();

				paramJsonProp.stringValue = EditorJsonUtility.ToJson(value);
				*/
            }

            EditorGUI.EndProperty();

			int GetIndex()
            {
				for (int i = 1; i < methods.Count; i++)
				{
					var m = methods[i];
					var pCount = m.GetParameters().Length;

                    if (pCount > 0)
					{
						var param = m.GetParameters()[0];
						if (param.ParameterType.Name != paramTypeProp.stringValue)
							continue;
					}
					else if (paramCountProp.intValue != pCount)
						continue;

					if (m.Name == methodProp.stringValue)
					{
                        bool isObj = m.ReflectedType == typeof(GameObject);
						bool hasComp = compProp.objectReferenceValue != null;
						bool isThisComp = hasComp && m.ReflectedType == compProp.objectReferenceValue.GetType();

                        if (isObj && !hasComp) return i;
						if (isThisComp)		   return i;
                    }
                }

				return 0;
			}

            void ShowParamGUI(Rect position, Type type)
            {
				if (type.IsSubclassOf(typeof(Component)))
				{
					paramComponentProp.objectReferenceValue = EditorGUI.ObjectField(position, "Value", paramComponentProp.objectReferenceValue, type, true); 
                    return;
				}

                string t = type.Name.ToLower();
                switch (t)
                {
                    case "int32":		paramIntProp	  .intValue				= EditorGUI.IntField(	position, "Value", paramIntProp	     .intValue		); break;
                    case "single":		paramFloatProp	  .floatValue			= EditorGUI.FloatField( position, "Value", paramFloatProp    .floatValue	); break;
                    case "string":		paramStringProp	  .stringValue		    = EditorGUI.TextField(  position, "Value", paramStringProp   .stringValue	); break;
                    case "boolean":		paramBoolProp	  .boolValue			= EditorGUI.Toggle(		position, "Value", paramBoolProp	 .boolValue		); break;
                    case "gameobject":	paramGameObjProp  .objectReferenceValue = EditorGUI.ObjectField(position, "Value", paramGameObjProp  .objectReferenceValue, typeof(GameObject), true); break;
                    default: EditorGUI.LabelField(position, "Unsupported value type: " + t); break;
                }
            }
        }

        private string GetDefaultValueJson(string paramType)
        {
			object instance = null;
            Type t = Type.GetType(paramType);
			switch (paramType.ToLower())
            {
                case "system.string":	instance = "";	  break;
                case "int":		instance = 0;	  break;
                case "float":	instance = 0;	  break;
                case "bool":	instance = false; break;
                default: instance = Activator.CreateInstance(t); break;
			}
            return EditorJsonUtility.ToJson(instance);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return HasParams() ? 36f : 18f;
		}

		private void GetMethods(GameObject obj)
		{
			methods = new List<MethodInfo>();
			var comps = obj.GetComponents<Component>();
            var flags = BindingFlags.Public | BindingFlags.Instance;
			
			// attributes
            var editorAttr = typeof(EditorBrowsableAttribute);
            var serialAttr = typeof(SerializeField);
            var obsoleAttr = typeof(ObsoleteAttribute);

            // Add all methods
            methods.AddRange(typeof(GameObject).GetMethods(flags));
            foreach (var comp in comps)
				methods.AddRange(comp.GetType().GetMethods(flags));

			// Filter methods
            methods = methods.Where(m => MethodFilter(m)).ToList();
			methods = methods.OrderBy(m => m.Name, new CustomComparer()).ToList();

			// Add 'None'
			methods = methods.Prepend(null).ToList();

			bool MethodFilter(MethodInfo m)
			{
				var parameters = m.GetParameters();
                int paramLen = parameters.Length;
				bool result = true;

				// Method is void or task
				result &= m.ReturnType == typeof(void) || m.ReturnType == typeof(Task);

				if (m.Name.Contains("set_"))
				{
					var propInfo = m.DeclaringType.GetProperty(m.Name.Replace("set_", ""));
					result &= !propInfo.HasAttribute(obsoleAttr);
				}

                // Method is public or serialized
                result &= m.IsPublic || m.HasAttribute(serialAttr);

				// Method is not obsolete
				result &= !m.HasAttribute(obsoleAttr);

				// Method either has no EditorBrowsable attribute or is Always browsable
				result &= !m.HasAttribute(editorAttr) || m.GetCustomAttribute(editorAttr).Match(EditorBrowsableState.Always);

				// Method has 0 or 1 parameters
				result &= paramLen <= 1;

				if (paramLen > 0 && m.Name.Contains("TestVoid6")) 
				{
				}

				// If method has parameters, they should be of these types
				result &= paramLen == 0 ? true :
					parameters[0].ParameterType == typeof(string)	  ||
					parameters[0].ParameterType == typeof(float)	  ||
					parameters[0].ParameterType == typeof(int)		  ||
					parameters[0].ParameterType == typeof(bool)		  ||
					parameters[0].ParameterType == typeof(GameObject) ||
                    parameters[0].ParameterType.IsSubclassOf(typeof(Component));

				return result;
			}	
		}

		private string GetFormattedName(MethodInfo m)
		{
			if (m == null) 
				return "None";
			
			string newName = $"{(m.ReturnType == typeof(Task) ? "async" : "sync")}: {m.ReflectedType.Name}/{m.Name}";
			string paramType = "";

            if (m.GetParameters().Length > 0)
                paramType = m.GetParameters()[0].ParameterType.Name;

            switch (paramType.ToLower())
            {
                case "int32": paramType = "int"; break;
                case "boolean": paramType = "bool"; break;
                case "string": paramType = "string"; break;
                case "single": paramType = "float"; break;
            }

            if (newName.Contains("set_"))
				newName = newName.Replace("set_", $"{paramType} ");
			else
				newName = $"{newName} ({paramType})";

			return newName;
		}

		private bool HasParams()
		{
			return methods[idx]?.GetParameters().Length > 0;
		}

		//private object ShowParamGUI(Rect position, object value)
		//{
  //          switch (value)
		//	{
  //              case int i:			value = EditorGUI.IntField(position, "Value", i); break;
  //              case float f:		value = EditorGUI.FloatField(position, "Value", f); break;
  //              case string s:		value = EditorGUI.TextField(position, "Value", s); break;
  //              case bool b:		value = EditorGUI.Toggle(position, "Value", b); break;
  //              case GameObject go: value = EditorGUI.ObjectField(position, "Value", go, typeof(GameObject), true); break;
  //              case Component c:	value = EditorGUI.ObjectField(position, "Value", c, typeof(Component), true); break;
  //              default: EditorGUI.LabelField(position, "Unsupported value type: " + value.GetType()); break;
  //          }

		//	if (value == "test")
		//		Debug.Break();

		//	return value;
  //      }
        
        public class CustomComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x.StartsWith("set_") && !y.StartsWith("set_"))
                    return -1;
                else if (!x.StartsWith("set_") && y.StartsWith("set_"))
                    return 1;
                else
                    return x.CompareTo(y);
            }
        }
    }
}