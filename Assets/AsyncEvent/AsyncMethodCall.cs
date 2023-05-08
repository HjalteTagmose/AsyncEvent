using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace AsyncEvents
{
	[Serializable]
	public class AsyncMethodCall
	{
		[SerializeField] private GameObject obj;
		[SerializeField] private Component component;
		[SerializeField] private bool isAsync;
		[SerializeField] private string method = "None";
		[SerializeField] private int paramCount;
		[SerializeField] private Parameter param;
		public Action action;

		private bool debugOn = false;

		public AsyncMethodCall(Action action)
		{
			this.action = action;

			//         if (action.Target is GameObject) 
			//             obj = action.Target as GameObject;
			//         else 
			//             component = action.Target as Component;

			//isAsync = action.Method.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null;
			//         method = action.Method.Name;
			//         paramCount = 0;
		}

		public async Task Invoke()
		{
			// If no method, do nothing
			if (method == "None")
			{
				Debug.LogWarning("No method specified");
				return;
			}

			// Create params
			object[] paramObjs = new object[paramCount];
			Type[] paramTypes = new Type[paramCount];
			if (paramCount > 0)
			{
				var val = param.GetValue();
				paramObjs[0] = val;
				paramTypes[0] = val.GetType();
			}

			// Create method
			bool isObj = component == null;
			object mObj;
			if (isObj) mObj = obj;
			else mObj = component;
			Type type = mObj.GetType();
			MethodInfo methodInfo = type.GetMethod(method, paramTypes);

			// Check if async
			isAsync = methodInfo?.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null;

			// Invoke it!
			if (debugOn) Debug.Log("start invoke: " + method);
			if (debugOn && methodInfo == null) Debug.LogWarning("Async call doesn't have method");
			var awaitable = methodInfo?.Invoke(mObj, paramObjs);
			if (isAsync) await (awaitable as Task);
			if (debugOn) Debug.Log("end invoke: " + method);
		}

		public void InvokeAction()
		{
			action?.Invoke();
		}

		public bool HasAction(Action action)
		{
			return this.action == action;
		}
	}
}