using System.Runtime.CompilerServices;

namespace NetGL;

public readonly struct Plane {
    public readonly float3 normal;
    public readonly float D;

    public Plane(float3 normal, float d) {
        this.normal = normal;
        this.D      = d;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float signed_distance(float3 point)
        => normal.x * point.x + normal.y * point.y + normal.z * point.z + D;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float distance(float3 point) => MathF.Abs(signed_distance(point));
}