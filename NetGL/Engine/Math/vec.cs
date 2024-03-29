global using float2 = NetGL.Vectors.vec2<float>;
global using float3 = NetGL.Vectors.vec3<float>;
global using double3 = NetGL.Vectors.vec3<double>;
global using int3 = NetGL.Vectors.vec3<int>;
global using ushort3 = NetGL.Vectors.vec3<ushort>;
global using short3 = NetGL.Vectors.vec3<short>;
global using byte3 = NetGL.Vectors.vec3<byte>;
global using half3 = NetGL.Vectors.vec3<System.Half>;

namespace NetGL.Vectors;

using System.Numerics;

#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters.
                               // Such names may become reserved for the language.

public static partial class vec {
    public static vec3<T> create<T>(in vec3<T> other) where T: unmanaged, INumber<T>
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
    public static double3 normalize(in double3 vector) => vector / vector.length();
    public static float3 normalize(in float3 vector) => vector / vector.length();
    public static half3 normalize(in half3 vector) => vector / vector.length();

    public static double length(this double3 vector)
        => Math.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);

    public static float length(this float3 vector)
        => MathF.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);

    public static half length(this half3 vector)
        => half.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
}

public static partial class vec {
    public static half dot(in half3 left, in half3 right) {
        return (half)((float)left.x * (float)right.x + (float)left.y * (float)right.y + (float)left.z * (float)right.z);
    }

    public static float dot(in float3 left, in float3 right) {
        return left.x * right.x + left.y * right.y + left.z * right.z;
    }

    public static double dot(in double3 left, in double3 right) {
        return left.x * right.x + left.y * right.y + left.z * right.z;
    }

    public static half3 cross(in half3 left, in half3 right) {
        return new half3(
                          (half)((float)left.y * (float)right.z - (float)left.z * (float)right.y),
                          (half)((float)left.z * (float)right.x - (float)left.x * (float)right.z),
                          (half)((float)left.x * (float)right.y - (float)left.y * (float)right.x)
                         );
    }

    public static float3 cross(in float3 left, in float3 right) {
        return new float3(
                          left.y * right.z - left.z * right.y,
                          left.z * right.x - left.x * right.z,
                          left.x * right.y - left.y * right.x
                         );
    }

    public static double3 cross(in double3 left, in double3 right) {
        return new double3(
                          left.y * right.z - left.z * right.y,
                          left.z * right.x - left.x * right.z,
                          left.x * right.y - left.y * right.x
                         );
    }
}