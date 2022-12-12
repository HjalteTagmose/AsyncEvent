using UnityEngine;

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
    }
}