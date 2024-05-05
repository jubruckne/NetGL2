global using float2 = NetGL.Vectors.vec2<float>;
global using float3 = NetGL.Vectors.vec3<float>;
global using float4 = NetGL.Vectors.vec4<float>;
global using double2 = NetGL.Vectors.vec2<double>;
global using double3 = NetGL.Vectors.vec3<double>;
global using int2 = NetGL.Vectors.vec2<int>;
global using int3 = NetGL.Vectors.vec3<int>;
global using int4 = NetGL.Vectors.vec4<int>;
global using ushort3 = NetGL.Vectors.vec3<ushort>;
global using short2 = NetGL.Vectors.vec2<short>;
global using short3 = NetGL.Vectors.vec3<short>;
global using sbyte2 = NetGL.Vectors.vec2<sbyte>;
global using sbyte3 = NetGL.Vectors.vec3<sbyte>;
global using byte2 = NetGL.Vectors.vec2<byte>;
global using byte3 = NetGL.Vectors.vec3<byte>;
global using byte4 = NetGL.Vectors.vec4<byte>;
global using half2 = NetGL.Vectors.vec2<System.Half>;
global using half3 = NetGL.Vectors.vec3<System.Half>;
global using half4 = NetGL.Vectors.vec4<System.Half>;

global using static NetGL.Vectors.VectorGlobal;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace NetGL.Vectors;

public static class VectorGlobal {

    public static T abs<T>(T number)
        where T: INumber<T>
        => T.Abs(number);

    public static T sqrt<T>(T number)
        where T: IRootFunctions<T>
        => T.Sqrt(number);

    // ------------------------ generic ------------------------------

    public static vec2<T> vec2<T>(T v)
        where T: unmanaged, INumber<T>
        => new vec2<T>(v, v);

    public static vec2<T> vec2<T>(T x, T y)
        where T: unmanaged, INumber<T>
        => new vec2<T>(x, y);

    public static vec3<T> vec3<T>(T v)
        where T: unmanaged, INumber<T>
        => new vec3<T>(v, v, v);

    public static vec3<T> vec3<T>(T x, T y, T z)
        where T: unmanaged, INumber<T>
        => new vec3<T>(x, y, z);

    public static vec4<T> vec4<T>(T x, T y, T z, T w)
        where T: unmanaged, INumber<T>
        => new vec4<T>(x, y, z, w);

    public static vec4<T> vec4<T>(T v)
        where T: unmanaged, INumber<T>
        => new vec4<T>(v, v, v, v);

    // -------------------------- int --------------------------------

    public static int2 int2(int x, int y)
        => new int2(x, y);

    public static int3 int3(int x, int y, int z)
        => new int3(x, y, z);

    public static int4 int4(int x, int y, int z, int w)
        => new int4(x, y, z, w);

    // ------------------------- float -------------------------------

    public static float2 float2(float x, float y)
        => new float2(x, y);

    public static float3 float3(float v)
        => new float3(v, v, v);

    public static float3 float3(float x, float y, float z)
        => new float3(x, y, z);

    public static float4 float4(float x, float y, float z, float w)
        => new float4(x, y, z, w);

    // ------------------------- functions ------------------------------

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

    public static float min(float left, float right)
        => MathF.Min(left, right);

    public static float max(float left, float right)
        => MathF.Max(left, right);

    public static float3 min(float3 left, float3 right)
        => new float3(
                      MathF.Min(left.x, right.x),
                      MathF.Min(left.y, right.y),
                      MathF.Min(left.z, right.z)
                     );

    public static float3 max(float3 left, float3 right)
        => new float3(
                      MathF.Max(left.x, right.x),
                      MathF.Max(left.y, right.y),
                      MathF.Max(left.z, right.z)
                     );
}