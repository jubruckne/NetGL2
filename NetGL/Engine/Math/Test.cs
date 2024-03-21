namespace NetGL.Vectors;

public class Test {
    public void test() {
        Vector3<System.Half> v1, v2, v3;

        v1 = ((Half)1, (Half)1, (Half)1);
        v2 = ((Half)1, (Half)2, (Half)3);

        v3 = v1 - v2;

        Vector3<int> vi = new Vector3<int>();

        Console.WriteLine($"v3 length = {v3.normalize()}");
    }
}