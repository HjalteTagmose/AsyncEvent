using System.Threading.Tasks;
using UnityEngine;

public class Test : MonoBehaviour
{
    public async Task TestTask() { }
    public async Task TestTask(float t) { }
    public void TestVoid() { }
    public void TestVoid(string t) { }
    public void TestVoid2(string s) { }
    public void TestVoid3(int s) { }
    public void TestVoid4(float f) { }
    public void TestVoid5(Transform t) { }
    public void TestVoid6(GameObject go) { }
}
