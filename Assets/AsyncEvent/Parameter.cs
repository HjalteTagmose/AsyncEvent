using UnityEngine;

namespace AsyncEvents
{
    [System.Serializable]
    public class Parameter
    {
        [SerializeField] public int intValue;
        [SerializeField] public bool boolValue;
        [SerializeField] public float floatValue;
        [SerializeField] public string stringValue;
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
                default          : return componentValue; 
            }
        }
    }
}