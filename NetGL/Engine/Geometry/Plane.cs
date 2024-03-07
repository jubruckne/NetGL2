using Vector3 = OpenTK.Mathematics.Vector3;

namespace NetGL;

public readonly struct Plane {
    public readonly Vector3 local_x;
    public readonly Vector3 local_y;
    public readonly Vector3 local_z;

    public static Plane XZ => new(Vector3.UnitY);

    public Plane(Vector3 normal) {
        local_z = normal.Normalized();

        // Find a vector that is not parallel to the normal
        Vector3 not_parallel = local_z.X != 0 ? new Vector3(local_z.Y, -local_z.X, 0) : new Vector3(0, local_z.Z, -local_z.Y);
        local_x = Vector3.Cross(local_z, not_parallel).Normalized();
        local_y = Vector3.Cross(local_z, local_x).Normalized();
    }

    public Vector3 this[float x, float y] => x * local_x + y * local_y;
    public Vector3 this[float x, float y, float z] => x * local_x + y * local_y + z * local_z;
}