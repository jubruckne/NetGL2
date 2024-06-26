using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetGL.Vectors;

public struct vec2<T>:
    ivec2<T>,
    IEquatable<vec2<T>>,
    IComparable<vec2<T>>
    where T: unmanaged, INumber<T>, ISubtractionOperators<T, T, T> {
    public T x;
    public T y;

    public vec2(T x, T y) {
        this.x = x;
        this.y = y;
    }

    public vec2(vec2<T> other) {
        this.x = other.x;
        this.y = other.y;
    }

    public vec2<T> set(T x, T y) {
        this.x = x;
        this.y = y;
        return this;
    }

    public vec2<T> set<P>(P x, P y)
        where P: unmanaged, INumberBase<P>, IRootFunctions<P> {
        this.x = T.CreateSaturating(x);
        this.y = T.CreateSaturating(y);
        return this;
    }

    public vec2<T> set(vec2<T> other) {
        x = other.x;
        y = other.y;
        return this;
    }

    public vec2<T> set<P>(vec2<P> other)
        where P: unmanaged, INumber<P> {
        x = T.CreateSaturating(other.x);
        y = T.CreateSaturating(other.y);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec2<T> operator+(vec2<T> left, vec2<T> right) {
        left.x += right.x;
        left.y += right.y;
        return left;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec2<T> operator-(vec2<T> left, vec2<T> right)
        => new (left.x - right.x, left.y - right.y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec2<T> operator*(vec2<T> left, T right)
        => new (left.x * right, left.y * right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec2<T> operator/(vec2<T> left, T right)
        => new (left.x / right, left.y / right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator==(vec2<T> left, vec2<T> right)
        => left.x == right.x && left.y == right.y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator!=(vec2<T> left, vec2<T> right)
        => left.x != right.x || left.y != right.y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator half2(vec2<T> other) =>
        new half2().set(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator float2(vec2<T> other) =>
        new float2().set(other);

    public static explicit operator double2(vec2<T> other) =>
        new double2().set(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator vec2<T>((T x, T y) vector)
        => new vec2<T>(vector.x, vector.y);

    public static explicit operator OpenTK.Mathematics.Vector2i(vec2<T> vector)
        => new(int.CreateChecked(vector.x), int.CreateChecked(vector.y));

    public static explicit operator int2(vec2<T> vector)
        => new(int.CreateChecked(vector.x), int.CreateChecked(vector.y));

    public readonly T length() {
        var v = (float2)this;
        return T.CreateSaturating(MathF.Sqrt(v.x * v.x + v.y * v.y));
    }

    public bool Equals(vec2<T> other)
        => x == other.x && y == other.y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(vec2<T> other) {
        if (x < other.x) return -1;
        if (x > other.x) return 1;

        if (y < other.y) return -1;
        if (y > other.y) return 1;

        return 0;
    }

    public override string ToString() => $"({x}, {y})";

    public static vec2<T> zero => new(T.Zero, T.Zero);
    public static vec2<T> unit_x => new(T.One, T.Zero);
    public static vec2<T> unit_y => new(T.Zero, T.One);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> as_span()
        => MemoryMarshal.CreateSpan(ref x, 2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine(x, y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out T x, out T y) {
        x = this.x;
        y = this.y;
    }

    T ivec2<T>.x => x;
    T ivec2<T>.y => y;
}