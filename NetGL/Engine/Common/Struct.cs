namespace NetGL;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct Struct<A, B> where A: struct where B: struct {
    public A a;
    public B b;

    public Struct(A a, B b)
    {
        this.a = a;
        this.b = b;
    }

    public static explicit operator (A, B)(Struct<A, B> s) => (s.a, s.b);

    public static implicit operator Struct<A, B>((A, B) valueTuple) {
        return new Struct<A, B>(valueTuple.Item1, valueTuple.Item2);
    }

    public override string ToString() {
        return $"({a}, {b})";
    }
}