using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace AsyncEvent
{
    [Serializable]
    public class AsyncMethodCall
    {
        public GameObject obj;
        public Component component;
        public string method = "";
        public bool isAsync;
        public object param;

        public async Task Invoke()
        {
            bool isObj = component == null;
            Type type = isObj ? typeof(GameObject) : component.GetType();
            MethodInfo methodInfo = type.GetMethod(method);
            ParameterInfo[] parameters = methodInfo.GetParameters();

            object[] p = new object[parameters.Length];
            if (p.Length > 0) p[0] = param;

            Debug.Log("start invoke: " + method);
            if (method == "None") 
            { }
            else if (!isAsync)
            {
                methodInfo.Invoke(isObj ? obj : component, p);
            }
            else
            {
                dynamic awaitable = methodInfo.Invoke(component, p);
                await awaitable;
            }
            Debug.Log("end invoke: " + method);
        }
    }
}