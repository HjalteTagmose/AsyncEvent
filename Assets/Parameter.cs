using System;
using UnityEngine;

namespace AsyncEvent
{
    [System.Serializable]
    public class Parameter
    {
        [SerializeField] public string json = "";
        [SerializeField] public string type = "";

        public void SetValue(object value)
        {
            this.type = value.GetType().ToString();
            this.json = JsonUtility.ToJson(value);
        }

        public object GetValue()
        {
            return JsonUtility.FromJson(json, Type.GetType(type));
        }
    }
}