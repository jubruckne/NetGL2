using System.Numerics;

namespace NetGL.Vectors;

public struct Vector2<T>: IVector2 where T: unmanaged, INumberBase<T> {
    public T x;
    public T y;

    public Vector2(T x, T y) {
        this.x = x;
        this.y = y;
    }

    public void set(T x, T y) {
        this.x = x;
        this.y = y;
    }

    public static implicit operator Vector2<T>((T x, T y) vector)
        => new Vector2<T>(vector.x, vector.y);

    public override string ToString() => $"({x}, {y})";
}