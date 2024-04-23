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

global using static NetGL.Vectors.Global;

namespace NetGL.Vectors;

public static class Global {
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

    public static float3 float3(float x, float y, float z)
        => new float3(x, y, z);

    public static float4 float4(float x, float y, float z, float w)
        => new float4(x, y, z, w);
}