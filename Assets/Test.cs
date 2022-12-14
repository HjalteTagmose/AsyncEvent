using System.Threading.Tasks;
using UnityEngine;

public class Test : MonoBehaviour
{
    public async Task TestTask() { await Task.Delay(12500); print("test async"); }
    public async Task TestTask(float sec) { await Task.Delay((int)(sec*1000)); print("test" + sec); }
    public void TestVoid()              => print("void");
    public void TestVoid(string t)      => print(t);
    public void TestVoid2(string s)     => print(s);
    public void TestVoid3(int s)        => print(s);
    public void TestVoid4(float f)      => print(f);
    public void TestVoid5(Transform t)  => print(t);
    public void TestVoid6(GameObject go)=> print(go);
    public void TestVoidCOMP(Test t)    => print(t);
    public void TestVoidCOMP(Canvas t)  => print(t);
}
