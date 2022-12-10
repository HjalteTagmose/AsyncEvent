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
        [SerializeField] private GameObject obj;
        [SerializeField] private Component component;
        [SerializeField] private string method = "";
        [SerializeField] private bool isAsync;
        [SerializeField] private Parameter param = new Parameter();

        public async Task Invoke()
        {
            bool isObj = component == null;
            Type type = isObj ? typeof(GameObject) : component.GetType();
            MethodInfo methodInfo = type.GetMethod(method);

            // Create params
            int paramCount = methodInfo.GetParameters().Length;
            object[] p = new object[0];
            if (paramCount > 0)
                p = new object[] { param };

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