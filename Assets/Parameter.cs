using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AsyncEvent
{
    [System.Serializable]
    public class Parameter
    {
        [SerializeField] public string stringValue;
        [SerializeField] public float floatValue;
        [SerializeField] public bool boolValue;
        [SerializeField] public int intValue;
        [SerializeField] public Component componentValue;
        [SerializeField] public GameObject gameObjValue;
        [SerializeField] public string typeValue;

        public object GetValue()
        {
            switch (typeValue.ToLower())
            {
                case "int32"     : return intValue;
                case "single"    : return floatValue;
                case "string"    : return stringValue;
                case "boolean"   : return boolValue;
                case "gameobject": return gameObjValue;
                default: return componentValue; 
            }
        }
    }
}