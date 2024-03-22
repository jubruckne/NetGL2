using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace NetGL;

public readonly struct Plane {
    public readonly Vector3 local_x;
    public readonly Vector3 local_y;
    public readonly Vector3 local_z;

    public static Plane XZ => new(Vector3.UnitY, Vector3.UnitX, Vector3.UnitZ);

    private Plane(Vector3 normal, Vector3 right, Vector3 forward) {
        local_z = normal;
        local_x = right;
        local_y = forward;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 to_world(float x, float y, float height) => new Vector3(x, height, -y);

    public Vector3 to_world(in Vector2 position, float height) => new Vector3(position.X, height, -position.Y);

    [SkipLocalsInit]
    public (Vector2 position, float height) world_to_point_on_plane_3d(in Vector3 world_pos) {
        float x = world_pos.X;
        float y = -world_pos.Z;
        float height = world_pos.Y;

        return (new Vector2(x, y), height);
    }

    [SkipLocalsInit]
    public Vector2 world_to_point_on_plane_2d(in Vector3 world_pos) {
        float x = world_pos.X;
        float y = -world_pos.Z;
        return new Vector2(x, y);
    }
}