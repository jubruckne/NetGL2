namespace NetGL.Vectors;

public class Test {
    public void test() {
        vec3<half> v1, v2, v3;

        v1 = ((half)1, (half)1, (half)1);
        v2 = ((half)1, (half)2, (half)3);

        v3 = v1 - v2;

        vec3<int> vi = new vec3<int>();

        Console.WriteLine($"v3 length = {v3.normalize()}");
    }
}