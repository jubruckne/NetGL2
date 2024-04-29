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

    public bool intersects(Box box) {
        var num1 = 0;
        var num2 = 0;
        if (normal.x * box.min.x + normal.y * box.min.y + normal.z * box.min.z + D > 0.0f)
            num1 = 1;
        if (normal.x * box.max.x + normal.y * box.max.y + normal.z * box.max.z + D > 0.0f)
            num2 = 1;
        return num1 != num2;
    }

    public bool intersects(Plane plane) {
        var num1 = 0;
        var num2 = 0;
        if (plane.normal.x * normal.x + plane.normal.y * normal.y + plane.normal.z * normal.z + D > 0.0f)
            num1 = 1;
        if (plane.normal.x * normal.x + plane.normal.y * normal.y + plane.normal.z * normal.z + D > 0.0f)
            num2 = 1;
        return num1 != num2;
    }

    public bool intersects(Ray ray) {
        var t = -(normal.x * ray.origin.x + normal.y * ray.origin.y + normal.z * ray.origin.z + D) /
                (normal.x * ray.direction.x + normal.y * ray.direction.y + normal.z * ray.direction.z);
        return t >= 0;
    }
}