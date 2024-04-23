using System.Runtime.CompilerServices;

namespace NetGL.Vectors;

using System.Numerics;

#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters.

public static partial class vec {
    public static vec2<T> create<T>(vec2<T> other) where T: unmanaged, INumber<T>
        => new vec2<T>(other);

    public static vec2<T> create<T>(T x, T y) where T: unmanaged, INumber<T>
        => new vec2<T>(x, y);

    public static vec2<T> create<T>(int2 other) where T: unmanaged, INumber<T>
        => new vec2<T>().set(other);

    public static vec2<T> create<T>(short2 other) where T: unmanaged, INumber<T>
        => new vec2<T>().set(other);

    public static vec2<T> create<T>(float2 other) where T: unmanaged, INumber<T>
        => new vec2<T>().set(other);

    public static vec2<T> create<T>(double2 other) where T: unmanaged, INumber<T>
        => new vec2<T>().set(other);

    public static vec2<T> create<T>(half2 other) where T: unmanaged, INumber<T>
        => new vec2<T>().set(other);


    public static vec3<T> create<T>(vec3<T> other) where T: unmanaged, INumber<T>
        => new vec3<T>(other);

    public static vec3<T> create<T>(T x, T y, T z) where T: unmanaged, INumber<T>
        => new vec3<T>(x, y, z);

    public static vec3<T> create<T>(int3 other) where T: unmanaged, INumber<T>
        => new vec3<T>().set(other);

    public static vec3<T> create<T>(short3 other) where T: unmanaged, INumber<T>
        => new vec3<T>().set(other);

    public static vec3<T> create<T>(float3 other) where T: unmanaged, INumber<T>
        => new vec3<T>().set(other);

    public static vec3<T> create<T>(double3 other) where T: unmanaged, INumber<T>
        => new vec3<T>().set(other);

    public static vec3<T> create<T>(half3 other) where T: unmanaged, INumber<T>
        => new vec3<T>().set(other);
}

public static partial class vec {
    public static float distance_to(this float2 p1, float2 p2) {
        var dx = p1.x - p2.x;
        var dy = p1.y - p2.y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    public static float distance_to(this float3 p1, float3 p2) {
        var dx = p1.x - p2.x;
        var dy = p1.y - p2.y;
        var dz = p1.z - p2.z;
        return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public static double3 normalize(double3 vector) => vector / vector.length();
    public static float3 normalize(float3 vector) => vector / vector.length();
    public static half3 normalize(half3 vector) => vector / vector.length();

    public static double length(this double3 vector)
        => Math.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);

    public static float length(this float3 vector)
        => MathF.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);

    public static half length(this half3 vector)
        => half.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
}

public static partial class vec {
    public static half dot(half3 left, half3 right)
        => (half)((float)left.x * (float)right.x + (float)left.y * (float)right.y + (float)left.z * (float)right.z);

    public static float dot(float3 left, float3 right) =>
        left.x * right.x + left.y * right.y + left.z * right.z;

    public static double dot(double3 left, double3 right)
        => left.x * right.x + left.y * right.y + left.z * right.z;

    public static half3 cross(half3 left, half3 right)
        => new half3(
                     (half)((float)left.y * (float)right.z - (float)left.z * (float)right.y),
                     (half)((float)left.z * (float)right.x - (float)left.x * (float)right.z),
                     (half)((float)left.x * (float)right.y - (float)left.y * (float)right.x)
                    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 cross(float3 left, float3 right)
        => new float3(
                      left.y * right.z - left.z * right.y,
                      left.z * right.x - left.x * right.z,
                      left.x * right.y - left.y * right.x
                     );

    public static double3 cross(double3 left, double3 right)
        => new double3(
                       left.y * right.z - left.z * right.y,
                       left.z * right.x - left.x * right.z,
                       left.x * right.y - left.y * right.x
                      );
}