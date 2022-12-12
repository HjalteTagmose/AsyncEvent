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
        [SerializeField] private string method = "";
        [SerializeField] private bool isAsync;
        [SerializeField] private int paramCount;
        [SerializeField] private Parameter param;

        public async Task Invoke()
        {
            if (method == "None")
            {
                Debug.LogWarning("No method specified");
                return;
            }

            // Create params
            object[] ps = new object[paramCount];
            Type[] ptypes = new Type[paramCount];
            if (paramCount > 0)
            {
                var val = param.GetValue();
                ps = new object[] { val };
                ptypes = new Type[] { val.GetType() };
            }

            // Create method
            bool isObj = component == null;
            Type type = isObj ? typeof(GameObject) : component.GetType();
            MethodInfo methodInfo = type.GetMethod(method, ptypes);

            Debug.Log("start invoke: " + method);
            if (method == "None") 
            { }
            else if (!isAsync)
            {
                methodInfo.Invoke(isObj ? obj : component, ps);
            }
            else
            {
                dynamic awaitable = methodInfo.Invoke(component, ps);
                await awaitable;
            }
            Debug.Log("end invoke: " + method);
        }
    }
}