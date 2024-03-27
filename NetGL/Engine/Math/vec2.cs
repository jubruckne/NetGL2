using System.Numerics;

namespace NetGL.Vectors;

public struct vec2<T>: ivec2<T> where T: unmanaged, INumberBase<T> {
    public T x;
    public T y;

    public vec2(T x, T y) {
        this.x = x;
        this.y = y;
    }

    public void set(T x, T y) {
        this.x = x;
        this.y = y;
    }

    public static implicit operator vec2<T>((T x, T y) vector)
        => new vec2<T>(vector.x, vector.y);

    public override string ToString() => $"({x}, {y})";

    I[] ivec.get_array<I>() => new T[]{x, y}.Cast<I>().ToArray();
    T[] ivec<T>.array => [x, y];
    T ivec2<T>.x => x;
    T ivec2<T>.y => y;
}