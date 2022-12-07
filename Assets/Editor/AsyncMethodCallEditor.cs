using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;
using Component = UnityEngine.Component;
using Unity.VisualScripting;

namespace AsyncEvent
{
	[CustomPropertyDrawer(typeof(AsyncMethodCall))]
	public class AsyncMethodCallEditor : PropertyDrawer
	{
		private bool gotMethods = false;
		private List<MethodInfo> methods;
		private int idx = 0, old = 0;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var objProp = property.FindPropertyRelative("obj");
			var compProp = property.FindPropertyRelative("component");
			var methodProp = property.FindPropertyRelative("method");
			var isAsyncProp = property.FindPropertyRelative("isAsync");

			label = EditorGUI.BeginProperty(position, label, property);

			position = EditorGUI.PrefixLabel(position, label);
			position.height = 18f;
			position.width /= 3f;

			EditorGUI.indentLevel = 0;
			EditorGUI.PropertyField(position, objProp, GUIContent.none);

			position.x += position.width + 5;
			position.width *= 2f;
			position.width -= 10;

			// Stop if there is no object
			if (objProp.objectReferenceValue == null)
			{
				EditorGUI.EndProperty();
				return;
			}

			// Get object and methods
			var objValue = (GameObject)objProp.objectReferenceValue;
			if (!gotMethods)
				GetMethods(objValue);

			// Find idx
			idx = old = GetIndex();

			// Dropdown
			var options = methods.Select(m => GetFormattedName(m)).ToArray();
			idx = EditorGUI.Popup(position, idx, options);

			// If picked new option, update call
			if (idx != old)
			{
				Component component = null;
                var selected = methods[idx];
                old = idx;

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

			EditorGUI.EndProperty();

			int GetIndex()
            {
				for (int i = 1; i < methods.Count; i++)
				{
					var m = methods[i];
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
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return 18f;
		}

		private void GetMethods(GameObject obj)
		{
			methods = new List<MethodInfo>();
			var comps = obj.GetComponents<Component>();
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var attr  = typeof(EditorBrowsableAttribute);

            // Add all methods
            methods.AddRange(typeof(GameObject).GetMethods(flags));
            foreach (var comp in comps)
				methods.AddRange(comp.GetType().GetMethods(flags));

			// Filter methods
            methods = methods.Where(m =>
				m.ReturnType == typeof(void) || 
				m.ReturnType == typeof(Task))
							.Where(m =>
                !m.HasAttribute(attr) ||
				m.GetCustomAttribute(attr).Match(EditorBrowsableState.Always))
							.Where(m =>
                m.GetParameters().Length <= 1)
							.Where(m => m.GetParameters().Length > 0 ?
                m.GetParameters()[0].ParameterType == typeof(string)||
                m.GetParameters()[0].ParameterType == typeof(float)	||
                m.GetParameters()[0].ParameterType == typeof(int)	||
                m.GetParameters()[0].ParameterType == typeof(bool)	||
                m.GetParameters()[0].ParameterType.IsSubclassOf(typeof(Component)) ||
                m.GetParameters()[0].ParameterType.IsSubclassOf(typeof(GameObject)) 
				: true)
							.ToList();

			// Add 'None'
			methods = methods.Prepend(null).ToList();
			gotMethods = true;
		}

		private string GetFormattedName(MethodInfo m)
		{
			if (m == null) return "None";
			return $"{(m.ReturnType == typeof(Task) ? "async" : "sync")}: {m.ReflectedType.Name}/{m.Name}";
		}
    }



	/*
	if (objValue != null)
	{
		var compValue = (Component)compProp.objectReferenceValue;
		if (compValue != null)
		{
			string pre = compValue.GetType().ToString();
			string methodValue = methodProp.stringValue;
			var methods = AsyncMethodCall.GetMethodsOfComponent(compValue);

			var syncMethods = methods.Where(m => m.ReturnType == typeof(void));
			var asyncMethods= methods.Where(m => m.ReturnType == typeof(Task));

			var methodNames = syncMethods.Select(m => m.Name)
										 .Concat(asyncMethods
											.Select(m => m.Name))
										 .ToArray();
			string[] options= syncMethods.Select(m => $"{pre}/{m.Name} (sync)")
										 .Concat(asyncMethods
											 .Select(m => $"{pre}/{m.Name} (async)"))
										 .Prepend(pre + ".<None>")
										 .Append("Choose Component")
										 .ToArray();

			int midx = Array.IndexOf(methodNames, methodValue);
			int idx  = midx < 0 ? 0 : midx+1;

			idx = EditorGUI.Popup(position, idx, options);
			midx = idx-1;

			isAsyncProp.boolValue = options[idx].Contains("async");

			if (midx >= methodNames.Length) compProp.objectReferenceValue = null;
			else if (midx >= 0)				methodProp.stringValue = methodNames[midx];
			else							methodProp.stringValue = null;
		}
		else
		{
			var comps = objValue.GetComponents<Component>();
			string[] options = comps.Select(c => c.GetType().ToString())
									.Prepend("None")
									.ToArray();

			int idx = Array.IndexOf(comps, compValue);
			if (idx < 0) idx = 0;

			idx = EditorGUI.Popup(position, idx, options);
			if (idx > 0) compProp.objectReferenceValue = comps[idx-1];

			methodProp.stringValue = null;
		}
	}
	else
	{
		compProp.objectReferenceValue = null;
		methodProp.stringValue = null;
	}

	EditorGUI.EndProperty();
	*/
}