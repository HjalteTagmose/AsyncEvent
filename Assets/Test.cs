using System.Threading.Tasks;
using UnityEngine;

public class Test : MonoBehaviour
{
    public async Task TestTask() { }
    public async Task TestTask(float t) { }
    public void TestVoid()              => print("void");
    public void TestVoid(string t)      => print(t);
    public void TestVoid2(string s)     => print(s);
    public void TestVoid3(int s)        => print(s);
    public void TestVoid4(float f)      => print(f);
    public void TestVoid5(Transform t)  => print(t);
    public void TestVoid6(GameObject go)=> print(go);
}
