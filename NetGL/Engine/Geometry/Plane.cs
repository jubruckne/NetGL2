using Vector3 = OpenTK.Mathematics.Vector3;

namespace NetGL;

public readonly struct Plane {
    public readonly Vector3 local_x;
    public readonly Vector3 local_y;
    public readonly Vector3 local_z;

    public static Plane XZ => new(Vector3.UnitY);

    public Plane(Vector3 normal) {
        local_z = normal.Normalized();

        if (normal == Vector3.UnitY) {
            local_x = Vector3.UnitZ;
            local_y = Vector3.UnitX;
        } else {
            throw new ArgumentOutOfRangeException(nameof(normal));
        }
    }

    public Vector3 this[float x, float y] => x * local_x + y * local_y;
    public Vector3 this[float x, float y, float z] => x * local_x + y * local_y - z * local_z;
}