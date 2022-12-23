using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using Component = UnityEngine.Component;

namespace AsyncEvent.Editor
{
	[CustomPropertyDrawer(typeof(AsyncMethodCall))]
	public class AsyncMethodCallEditor : PropertyDrawer
	{
		private MethodComparer methodComparer = new MethodComparer();

		#region ViewData
		private class ViewData
        {
            public int index;
            public string[] options;
            public List<MethodInfo> methods;

			public MethodInfo CurMethod => methods[index];

			public ViewData()
			{
				index   = 0;
				methods = new List<MethodInfo>() { null };
				options = new[] { "None" };
			}
		}

		private static Dictionary<string, ViewData> propertyData = new Dictionary<string, ViewData>();
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
			// Save rect
	 		var orig = position;

			// Get all properties
			GetProperties(property);
			GetParamProperties(property);

			// Get ViewData
			var data = GetViewData(property.propertyPath);

            // Label
			label = EditorGUI.BeginProperty(position, label, property);
			position = EditorGUI.PrefixLabel(position, label);

			// Object
			EditorGUI.BeginChangeCheck();
			position.height = 18f;
			position.width = orig.width * 0.3f;
			EditorGUI.indentLevel = 0;
			EditorGUI.PropertyField(position, objProp);

			// Update data when object is changed
			var obj = (GameObject)objProp.objectReferenceValue;
			if (EditorGUI.EndChangeCheck())
			{
				UpdateViewData(data, obj);
				data.index = 0;
			}

			// Disable if obj is null
			EditorGUI.BeginDisabledGroup(obj == null);

			// Dropdown
			position.x += position.width + 5;
			position.width = orig.width * 0.7f;
			position.width -= 8;
			int newIndex = EditorGUI.Popup(position, data.index, data.options);

			// Select method
			if (newIndex != data.index)
			{
				data.index = newIndex;
				SelectMethod(obj, data.CurMethod);
			}

			// Show parameter GUI if there are any
			var param = data.CurMethod?.GetParameters();
			if (param?.Length > 0)
			{
				position.y += 20;
				Type pType = param[0].ParameterType;
				ShowParamGUI(position, pType);
			}

			EditorGUI.EndDisabledGroup();
			EditorGUI.EndProperty();
		}

		#region Get Properties
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
		#endregion

		#region Get View Data
		private ViewData GetViewData(string key)
		{
			ViewData data;

            if (!propertyData.TryGetValue(key, out data))
			{
				var obj = (GameObject)objProp.objectReferenceValue;
				data = new ViewData();

				if (obj != null)
					UpdateViewData(data, obj);

                propertyData.Add(key, data);
            }

			return data;
		} 

		private void UpdateViewData(ViewData data, GameObject obj)
		{
			data.methods = GetMethods(obj);
			data.options = GetOptions(data.methods);
			data.index	 = GetIndex(data.methods);
		}

		public static void Added(string key)
		{
			propertyData[key] = new ViewData();
		}

		public static void ClearDatas()
		{
            propertyData.Clear();
		}

		private List<MethodInfo> GetMethods(GameObject obj)
		{
			// Null check
			if (obj == null) 
				return new List<MethodInfo>() { null };

			// Setup
			var methods = new List<MethodInfo>();
			var comps = obj.GetComponents<Component>();
			var flags = BindingFlags.Public | BindingFlags.Instance;

			// Add all methods
			methods.AddRange(typeof(GameObject).GetMethods(flags));
			foreach (var comp in comps)
				methods.AddRange(comp.GetType().GetMethods(flags));

			// Filter methods
			methods = methods.Where(m => ShouldShowMethod(m)).ToList();
			methods = methods.OrderBy(m => m.Name, methodComparer).ToList();

			// Add 'None'
			methods = methods.Prepend(null).ToList();

			// Return
			return methods;

			bool ShouldShowMethod(MethodInfo m)
			{
				// Setup
				var parameters = m.GetParameters();
				int paramLen = parameters.Length;
				bool result = true;

				// Attributes
				var editorAttr = typeof(EditorBrowsableAttribute);
				var serialAttr = typeof(SerializeField);
				var obsoleAttr = typeof(ObsoleteAttribute);

				// Method is void or task
				result &= m.ReturnType == typeof(void) || m.ReturnType == typeof(Task);

				// Check if property method
				if (m.Name.Contains("set_"))
				{
					var propInfo = m.DeclaringType.GetProperty(m.Name.Replace("set_", ""));
					result &= !HasAttribute(propInfo, obsoleAttr);
				}

				// Method is public or serialized
				result &= m.IsPublic || HasAttribute(m, serialAttr);

				// Method is not obsolete
				result &= !HasAttribute(m, obsoleAttr);

				// Method either has no EditorBrowsable attribute or is Always browsable
				result &= !HasAttribute(m, editorAttr) || m.GetCustomAttribute(editorAttr).Match(EditorBrowsableState.Always);

				// Method has 0 or 1 parameters
				result &= paramLen <= 1;

				// If method has parameters, they should be of these types
				result &= paramLen == 0 ? true :
					parameters[0].ParameterType == typeof(string) ||
					parameters[0].ParameterType == typeof(float) ||
					parameters[0].ParameterType == typeof(int) ||
					parameters[0].ParameterType == typeof(bool) ||
					parameters[0].ParameterType == typeof(GameObject) ||
					parameters[0].ParameterType.IsSubclassOf(typeof(Component));

				return result;
			}
		}

		private string[] GetOptions(List<MethodInfo> methods)
		{
			return methods.Select(m => GetFormattedName(m)).ToArray();

			string GetFormattedName(MethodInfo m)
			{
				if (m == null)
					return "None";

				string newName = $"{m.ReflectedType.Name}/{m.Name}{(m.ReturnType == typeof(Task) ? "*" : "")}";
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
		}

		private int GetIndex(List<MethodInfo> methods)
		{
			for (int i = 1; i < methods.Count; i++)
			{
				var m = methods[i];
				var pCount = m.GetParameters().Length;

				// same params?
				if (pCount > 0)
				{
					var param = m.GetParameters()[0];
					if (param.ParameterType.Name != paramTypeProp.stringValue)
						continue;
				}
				else if (paramCountProp.intValue != pCount)
					continue;

				// same name?
				if (m.Name == methodProp.stringValue)
				{
					bool isObj = m.ReflectedType == typeof(GameObject);
					bool hasComp = compProp.objectReferenceValue != null;
					bool isThisComp = hasComp && m.ReflectedType == compProp.objectReferenceValue.GetType();

					if (isObj && !hasComp) return i;
					if (isThisComp) return i;
				}
			}

			return 0;
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

		#region Helpers
		bool HasAttribute(MethodInfo m, Type attr) => m.GetCustomAttributes().FirstOrDefault(x => x.GetType() == attr) != null;
		bool HasAttribute(PropertyInfo p, Type attr) => p.GetCustomAttributes().FirstOrDefault(x => x.GetType() == attr) != null;
		private class MethodComparer : IComparer<string>
		{
			public int Compare(string x, string y)
			{
				if (x.StartsWith("set_") && !y.StartsWith("set_")) return -1;
				else if (!x.StartsWith("set_") && y.StartsWith("set_")) return 1;
				else return x.CompareTo(y);
			}
		}
		#endregion

		private void SelectMethod(GameObject obj, MethodInfo method)
		{
			// Reset parameter
			var paramsArr = method.GetParameters();
			ResetParameter(paramsArr);

			// Stop if 'None'-method
			if (method == null)
			{
				methodProp.stringValue = "None";
				compProp.objectReferenceValue = null;
				EditorGUI.EndProperty();
				return;
			}

			// Figure out types
			Component component = null;
			var methodType = method.ReflectedType;
			bool isObj = methodType == typeof(GameObject);
			bool hasComp = !isObj && obj.TryGetComponent(methodType, out component);

			// Assign method properties
			compProp.objectReferenceValue = hasComp ? component : null;
			methodProp.stringValue = method.Name;
			isAsyncProp.boolValue = method.ReturnType == typeof(Task);
		}

		private void ResetParameter(ParameterInfo[] paramInfos)
		{
			int paramCount = paramInfos == null ? 0 : paramInfos.Length;

			paramIntProp.intValue = 0;
			paramFloatProp.floatValue = 0f;
			paramStringProp.stringValue = "";
			paramBoolProp.boolValue = false;
			paramComponentProp.objectReferenceValue = null;
			paramGameObjProp.objectReferenceValue = null;
			paramCountProp.intValue = paramCount;
			paramTypeProp.stringValue = paramCount > 0 ? paramInfos[0].ParameterType.Name : "";
		}
	}
}