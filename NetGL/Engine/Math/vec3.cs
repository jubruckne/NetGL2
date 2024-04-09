using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetGL.Vectors;

using System.Numerics;

[StructLayout(LayoutKind.Sequential), SkipLocalsInit]
public partial struct vec3<T>:
    ivec3<T>,
    IComparable<vec3<T>>,
    IEquatable<vec3<T>>
    where T: unmanaged, INumber<T> {

    private static readonly T t_one = T.One;
    private static readonly T t_zero = T.Zero;

    public T x;
    public T y;
    public T z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public vec3() {}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public vec3(in vec3<T> other) {
        this.x = other.x;
        this.y = other.y;
        this.z = other.z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public vec3(T x, T y, T z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public vec3<T> set(T x, T y, T z) {
        this.x = x;
        this.y = y;
        this.z = z;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public vec3<T> set<P>(P x, P y, P z)
        where P: unmanaged, INumberBase<P> {
        this.x = T.CreateSaturating(x);
        this.y = T.CreateSaturating(y);
        this.z = T.CreateSaturating(z);
        return this;
    }

    public vec3<T> set(in vec3<T> other) {
        x = other.x;
        y = other.y;
        z = other.z;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public vec3<T> set<P>(in vec3<P> other)
        where P: unmanaged, INumber<P> {
        x = T.CreateSaturating(other.x);
        y = T.CreateSaturating(other.y);
        z = T.CreateSaturating(other.z);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator vec3<T>((T x, T y, T z) other) =>
        new(other.x, other.y, other.z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator vec3<T>(OpenTK.Mathematics.Vector3 other) =>
        new vec3<T>().set(other.X, other.Y, other.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator half3(in vec3<T> other) =>
        new half3().set(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator float3(in vec3<T> other)
        => new float3().set(other);

    public static explicit operator double3(in vec3<T> other) =>
        new double3().set(other);

    public static explicit operator OpenTK.Mathematics.Vector3(in vec3<T> vector) {
        return new(
                   float.CreateSaturating(vector.x),
                   float.CreateSaturating(vector.y),
                   float.CreateSaturating(vector.z)
                  );
    }

    public static explicit operator System.Numerics.Vector3(in vec3<T> vector) {
        return new(
                   float.CreateSaturating(vector.x),
                   float.CreateSaturating(vector.y),
                   float.CreateSaturating(vector.z)
                  );
    }

    public static readonly vec3<T> zero = new(t_zero, t_zero, t_zero);

    public static readonly vec3<T> unit_x = new(t_one, t_zero, t_zero);
    public static readonly vec3<T> unit_y = new(t_zero, t_one, t_zero);
    public static readonly vec3<T> unit_z = new(t_zero, t_zero, t_one);

    T[] ivec<T>.array => [x, y, z];
    T ivec2<T>.x => x;
    T ivec2<T>.y => y;
    T ivec3<T>.z => z;
    I[] ivec.get_array<I>() => new T[] { x, y, z }.Cast<I>().ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(vec3<T> other) {
        if (x < other.x) return -1;
        if (x > other.x) return 1;

        if (y < other.y) return -1;
        if (y > other.y) return 1;

        if (z < other.z) return -1;
        if (z > other.z) return 1;

        return 0;
    }
}

public partial struct vec3<T> {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec3<T> operator+(in vec3<T> value) {
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec3<T> operator-(in vec3<T> value) {
        return new vec3<T>().set(-value.x, -value.y, -value.z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec3<T> operator+(vec3<T> left, in vec3<T> right) {
        left.x += right.x;
        left.y += right.y;
        left.z += right.z;
        return left;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec3<T> operator-(vec3<T> left, in vec3<T> right) {
        left.x -= right.x;
        left.y -= right.y;
        left.z -= right.z;
        return left;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec3<T> operator*(vec3<T> left, in vec3<T> right) {
        left.x *= right.x;
        left.y *= right.y;
        left.z *= right.y;
        return left;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec3<T> operator*(vec3<T> left, T right) {
        left.x *= right;
        left.y *= right;
        left.z *= right;
        return left;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec3<T> operator/(vec3<T> left, in vec3<T> right) {
        left.x /= right.x;
        left.y /= right.y;
        left.z /= right.z;
        return left;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec3<T> operator/(vec3<T> left, T right) {
        left.x /= right;
        left.y /= right;
        left.z /= right;
        return left;
    }

    public static bool operator==(in vec3<T> left, in vec3<T> right) {
        return left.x == right.x && left.y == right.y && left.z == right.z;
    }

    public static bool operator!=(in vec3<T> left, in vec3<T> right) {
        return left.x != right.x || left.y != right.y || left.z != right.z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T length() {
        var v = new float3(float.CreateSaturating(x), float.CreateSaturating(y), float.CreateSaturating(z));
        return T.CreateSaturating(MathF.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z));
    }

    public void normalize() => this /= length();
}

public partial struct vec3<T> {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(vec3<T> other) =>
        x == other.x && y == other.y;

    public override bool Equals(object? obj)
        => obj is vec3<T> other && Equals(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => HashCode.Combine(x, y, z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"({x}, {y}, {z})";
}