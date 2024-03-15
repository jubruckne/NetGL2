namespace NetGL;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct Struct<A, B> where A: struct where B: struct {
    public A a;
    public B b;

    public Struct(A a, B b) {
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

[StructLayout(LayoutKind.Sequential)]
public struct Struct<A, B, C> where A: struct where B: struct where C: struct {
    public A a;
    public B b;
    public C c;

    public Struct(A a, B b, C c) {
        this.a = a;
        this.b = b;
        this.c = c;
    }

    public static explicit operator (A, B, C)(Struct<A, B, C> s) => (s.a, s.b, s.c);

    public static implicit operator Struct<A, B, C>((A, B, C) valueTuple) {
        return new Struct<A, B, C>(valueTuple.Item1, valueTuple.Item2, valueTuple.Item3);
    }

    public override string ToString() {
        return $"({a}, {b}, {c})";
    }
}