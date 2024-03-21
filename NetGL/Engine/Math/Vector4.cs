using System.Numerics;

namespace NetGL.Vectors;

public struct Vector4<T>: IVector4 where T: unmanaged, INumberBase<T> {
    public T x;
    public T y;
    public T z;
    public T w;

    public Vector4(T x, T y, T z, T w) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public void set(T x, T y) {
        this.x = x;
        this.y = y;
    }

    public void set(T x, T y, T z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public void set(T x, T y, T z, T w) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public static implicit operator Vector4<T>((T x, T y, T z, T w) vector) =>
        new Vector4<T>(vector.x, vector.y, vector.z, vector.w);

    public override string ToString() => $"({x}, {y}, {z}, {w})";
}