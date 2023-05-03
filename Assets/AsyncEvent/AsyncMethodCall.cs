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
        
        private bool debugOn = false;

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

            // Invoke it!
            if (debugOn) Debug.Log("start invoke: " + method);
			if (debugOn && methodInfo == null) Debug.LogWarning("Async call doesn't have method");
			var awaitable = methodInfo?.Invoke(mObj, paramObjs); 
            if (isAsync) await (awaitable as Task);
            if (debugOn) Debug.Log("end invoke: " + method);
        }
    }
}