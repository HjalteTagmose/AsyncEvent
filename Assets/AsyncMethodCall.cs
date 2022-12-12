using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace AsyncEvent
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
            object mObj = isObj ? obj : component;
            Type type = mObj.GetType();
            MethodInfo methodInfo = type.GetMethod(method, paramTypes);

            if (debugOn) 
                Debug.Log("start invoke: " + method);
            
            if (!isAsync)
            {
                methodInfo.Invoke(mObj, paramObjs);
            }
            else
            {
                dynamic awaitable = methodInfo.Invoke(mObj, paramObjs);
                await awaitable;
            }
    
            if (debugOn)
                Debug.Log("end invoke: " + method);
        }
    }
}