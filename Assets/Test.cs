using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Test : MonoBehaviour
{
    public string publicVar;
    private string privateVar;

    public async Task TestTask() { }
    public async Task TestTask(float t) { }
    public void TestVoid() { }
    public void TestVoid2(Transform t) { }
    public void TestVoid3(string s) { }
}
