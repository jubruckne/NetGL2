using System.Runtime.CompilerServices;

namespace NetGL;


public readonly struct Plane {
    public readonly float3 normal;
    public readonly float D;

    public Plane(float3 normal, float d) {
        this.normal = normal;
        this.D      = d;
    }

    /*
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float3 to_world(float x, float y, float height) => new float3(x, height, -y);

    public float3 to_world(in float2 position, float height) => new float3(position.x, height, -position.y);

    [SkipLocalsInit]
    public (float2 position, float height) world_to_point_on_plane_3d(in float3 world_pos) {
        float x = world_pos.x;
        float y = -world_pos.z;
        float height = world_pos.y;

        return (new float2(x, y), height);
    }

    [SkipLocalsInit]
    public Vector2 world_to_point_on_plane_2d(in Vector3 world_pos) {
        float x = world_pos.X;
        float y = -world_pos.Z;
        return new Vector2(x, y);
    }
    */

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float signed_distance(in float3 point)
        => normal.x * point.x + normal.y * point.y + normal.z * point.z + D;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float distance(in float3 point) => MathF.Abs(signed_distance(point));
}