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

        public async Task Invoke()
        {
            bool isObj = component == null;
            Type type = isObj ? typeof(GameObject) : component.GetType();
            MethodInfo methodInfo = type.GetMethod(method);

            Debug.Log("start invoke: " + method);
            if (method == "None") { }
            else if (!isAsync)
            {
                methodInfo.Invoke(isObj ? obj : component, new object[0]);
            }
            else
            {
                dynamic awaitable = methodInfo.Invoke(component, new object[0]);
                await awaitable;
            }
            Debug.Log("end invoke: " + method);
        }
    }
}