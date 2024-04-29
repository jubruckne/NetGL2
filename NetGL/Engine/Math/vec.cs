namespace NetGL.Vectors;

#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters.

public static class vec {
    public static float2 xy(this float3 v)
        => float2(v.x, v.y);

    public static float2 xz(this float3 v)
        => float2(v.x, v.z);

    public static float2 yz(this float3 v)
        => float2(v.y, v.z);
}