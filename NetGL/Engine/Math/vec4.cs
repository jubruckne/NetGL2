using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using Spectre.Console.Cli.Unsafe;

namespace NetGL.Vectors;

public partial struct vec4<T>:
    ivec4<T> ,
    IEquatable<vec4<T>>
    where T: unmanaged, INumber<T>, IBinaryNumber<T> {

    public T x;
    public T y;
    public T z;
    public T w;

    public vec4(vec3<T> xyz, T w) {
        this.x = xyz.x;
        this.y = xyz.y;
        this.z = xyz.z;
        this.w = w;
    }

    public vec4(T x, T y, T z, T w) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public vec4<T> set(T x, T y, T z, T w) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public vec4<T> set<P>(P x, P y, P z, P w)
        where P: unmanaged, INumberBase<P> {
        this.x = T.CreateSaturating(x);
        this.y = T.CreateSaturating(y);
        this.z = T.CreateSaturating(z);
        this.w = T.CreateSaturating(w);
        return this;
    }

    public static implicit operator vec4<T>((T x, T y, T z, T w) vector) =>
        new vec4<T>(vector.x, vector.y, vector.z, vector.w);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec4<T> operator+(vec4<T> left, vec4<T> right)
        => new vec4<T>().set(left.x + right.x, left.y + right.y, left.z + right.z, left.w + right.w);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec4<T> operator+(vec4<T> left, T right)
        => new vec4<T>().set(left.x + right, left.y + right, left.z + right, left.w + right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec4<T> operator-(vec4<T> left, vec4<T> right)
        => new vec4<T>().set(left.x - right.x, left.y - right.y, left.z - right.z, left.w - right.w);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool4 operator<(vec4<T> left, vec4<T> right) {
        if (left is float4 vl && right is float4 vr) {
            var l = Vector128.Create<float>(vl.as_span());
            var r = Vector128.Create<float>(vr.as_span());
            return uint4(AdvSimd.CompareLessThan(l, r).As<float, uint>());
        }
        Error.invalid_argument(left);
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool4 operator<(vec4<T> left, T right) {
        if (left is float4 vl && right is float vr) {
            var l = Vector128.Create<float>(vl.as_span());
            var r = Vector128.Create<float>(vr);
            return uint4(AdvSimd.CompareLessThan(l, r).As<float, uint>());
        }
        Error.invalid_argument(left);
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool4 operator>(vec4<T> left, T right) {
        if (left is float4 vl && right is float vr) {
            var l = Vector128.Create<float>(vl.as_span());
            var r = Vector128.Create<float>(vr);
            return uint4(AdvSimd.CompareGreaterThan(l, r).As<float, uint>());
        }
        Error.invalid_argument(left);
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool4 operator<=(vec4<T> left, vec4<T> right) {
        if (left is float4 vl && right is float4 vr) {
            var l = Vector128.Create<float>(vl.as_span());
            var r = Vector128.Create<float>(vr.as_span());
            return uint4(AdvSimd.CompareLessThanOrEqual(l, r).As<float, uint>());
        }
        Error.invalid_argument(left);
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool4 operator>=(vec4<T> left, vec4<T> right) {
        if (left is float4 vl && right is float4 vr) {
            var l = Vector128.Create<float>(vl.as_span());
            var r = Vector128.Create<float>(vr.as_span());
            return uint4(AdvSimd.CompareGreaterThanOrEqual(l, r).As<float, uint>());
        }
        Error.invalid_argument(left);
        return default;
    }

    public static bool4 operator>(vec4<T> left, vec4<T> right) {
        if (left is float4 vl && right is float4 vr) {
            var l = Vector128.Create<float>(vl.as_span());
            var r = Vector128.Create<float>(vr.as_span());
            return uint4(AdvSimd.CompareGreaterThan(l, r).As<float, uint>());
        }
        Error.invalid_argument(left);
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec4<T> operator*(vec4<T> left, vec4<T> right)
        => new vec4<T>().set(left.x - right.x, left.y - right.y, left.z - right.z, left.w - right.w);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec4<T> operator!(vec4<T> left)
        => new vec4<T>().set(
                             left.x == T.Zero ? T.One : T.Zero,
                             left.y == T.Zero ? T.One : T.Zero,
                             left.z == T.Zero ? T.One : T.Zero,
                             left.w == T.Zero ? T.One : T.Zero
                            );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec4<T> operator-(vec4<T> left, T right)
        => new vec4<T>().set(left.x - right, left.y - right, left.z - right, left.w - right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec4<T> operator-(T left, vec4<T> right)
        => new vec4<T>().set(left - right.x, left - right.y, left - right.z, left - right.w);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec4<T> operator-(vec4<T> left, int4 right)
        => new vec4<T>().set(
                             left.x - T.CreateTruncating(right.x),
                             left.y - T.CreateTruncating(right.y),
                             left.z - T.CreateTruncating(right.z),
                             left.w - T.CreateTruncating(right.w)
                            );



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec4<T> operator&(vec4<T> left, vec4<T> right)
        => new vec4<T>().set(left.x & right.x, left.y & right.y, left.z & right.z, left.w & right.w);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec4<T> operator&(vec4<T> left, T right)
        => new vec4<T>().set(left.x & right, left.y & right, left.z & right, left.w & right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec4<T> operator^(vec4<T> left, vec4<T> right)
        => new vec4<T>().set(left.x ^ right.x, left.y ^ right.y, left.z ^ right.z, left.w ^ right.w);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec4<T> operator|(vec4<T> left, vec4<T> right)
        => new vec4<T>().set(left.x | right.x, left.y | right.y, left.z | right.z, left.w | right.w);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec4<T> operator*(vec4<T> left, T right)
        => new vec4<T>().set(left.x * right, left.y * right, left.z * right, left.w * right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec4<T> operator>> (vec4<T> left, int right)
        => new vec4<T>().set(
                             Unsafe.BitCast<uint, T>(Unsafe.BitCast<T, uint>(left.x) >> right),
                             Unsafe.BitCast<uint, T>(Unsafe.BitCast<T, uint>(left.y) >> right),
                             Unsafe.BitCast<uint, T>(Unsafe.BitCast<T, uint>(left.z) >> right),
                             Unsafe.BitCast<uint, T>(Unsafe.BitCast<T, uint>(left.w) >> right)
                            );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static vec4<T> operator<<(vec4<T> left, int right)
        => new vec4<T>().set(
                             Unsafe.BitCast<uint, T>(Unsafe.BitCast<T, uint>(left.x) << right),
                             Unsafe.BitCast<uint, T>(Unsafe.BitCast<T, uint>(left.y) << right),
                             Unsafe.BitCast<uint, T>(Unsafe.BitCast<T, uint>(left.z) << right),
                             Unsafe.BitCast<uint, T>(Unsafe.BitCast<T, uint>(left.w) << right)
                            );

    T ivec2<T>.x => x;
    T ivec2<T>.y => y;
    T ivec3<T>.z => z;
    T ivec4<T>.w => w;
}

public partial struct vec4<T> {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(vec4<T> other)
        => x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> as_span()
        => MemoryMarshal.CreateSpan(ref x, 4);

    public override bool Equals(object? obj)
        => obj is vec3<T> other && Equals(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine(x, y, z, w);

    public override string ToString() => $"({x}, {y}, {z}, {w})";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out T x, out T y, out T z, out T w) {
        x = this.x;
        y = this.y;
        z = this.z;
        w = this.w;
    }
}