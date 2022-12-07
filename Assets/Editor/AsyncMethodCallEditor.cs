using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

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

			//need type
			if (objProp.objectReferenceValue == null)
			{
				EditorGUI.EndProperty();
				return;
			}

			var objValue = (GameObject)objProp.objectReferenceValue;
			if (!gotMethods)
				GetMethods(objValue);

			if (compProp.objectReferenceValue == null)
				idx = old = 0;
			else
				idx = old = methods.FindIndex(m =>
					m.ReflectedType == compProp.objectReferenceValue.GetType() &&
					m.Name == methodProp.stringValue);

			var options = methods.Select(m => GetFormattedName(m)).ToArray();
			idx = EditorGUI.Popup(position, idx, options);

			if (idx != old)
			{
				Component component = null;
                var selected = methods[idx];
				
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

				old = idx;
			}

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return 18f;
		}

		private void GetMethods(GameObject obj)
		{
			methods = new List<MethodInfo>();
			var comps = obj.GetComponents<Component>();
            var flags = BindingFlags.Public | BindingFlags.Instance;

			// Add all methods
			methods.AddRange(typeof(GameObject).GetMethods(flags));
            foreach (var comp in comps)
				methods.AddRange(comp.GetType().GetMethods(flags));

			// Filter methods
			methods = methods.Where(m =>
				m.GetParameters().Length == 0 && (
				m.ReturnType == typeof(void) || 
				m.ReturnType == typeof(Task))).ToList();

			// Add 'None'
			methods.Prepend(null);
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