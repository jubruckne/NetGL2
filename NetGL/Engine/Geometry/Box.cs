using System.Diagnostics.CodeAnalysis;

namespace NetGL;

public readonly struct Box: IEquatable<Box> {
    public readonly float3 min;
    public readonly float3 max;

    public static Box unit = new Box();

    public Box() {
        min = float3(-0.5f);
        max = float3(0.5f);
    }

    private Box(float3 min, float3 max) {
        this.min = min;
        this.max = max;
    }

    public static Box centered_at(float3 center, float width, float height, float depth)
        => new(center - (width, height, depth), center + (width, height, depth));

    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public static Box from_points(IEnumerable<float3> points) {
        var min = float3(float.MaxValue);
        var max = float3(float.MinValue);

        foreach (var point in points) {
            if (point.x < min.x)
                min.x = point.x;

            if (point.y < min.y)
                min.y = point.y;

            if (point.z < min.z)
                min.z = point.z;

            if (point.x > max.x)
                max.x = point.x;

            if (point.y > max.y)
                max.y = point.y;

            if (point.z > max.z)
                max.z = point.z;
        }

        if(min.x == float.MaxValue)
            throw new ArgumentException(nameof(points));

        return new Box(min, max);
    }

    public static Box from_rectangle_xz(Rectangle<float> rectangle, float bottom, float top) {
        return new Box(
            float3(rectangle.left, bottom, rectangle.bottom),
            float3(rectangle.right, top, rectangle.top)
        );
    }

    public static Box from_rectangle_xz(Rectangle<float> rectangle) {
        return new Box(
                       float3(rectangle.left, float.NegativeInfinity, rectangle.bottom),
                       float3(rectangle.right, float.PositiveInfinity, rectangle.top)
                      );
    }

    public static Box merge(Box left, Box right) {
        return new Box(
            float3(
                Math.Min(left.min.x, right.min.x),
                Math.Min(left.min.y, right.min.y),
                Math.Min(left.min.z, right.min.z)
            ),
            float3(
                Math.Max(left.max.x, right.max.x),
                Math.Max(left.max.y, right.max.y),
                Math.Max(left.max.z, right.max.z)
            )
        );
    }

    public List<float3> corners => [
        min,
        float3(max.x, min.y, min.z),
        float3(min.x, max.y, min.z),
        float3(max.x, max.y, min.z),
        float3(min.x, min.y, max.z),
        float3(max.x, min.y, max.z),
        float3(min.x, max.y, max.z),
        max
    ];

    public bool contains(float3 point) {
        return
            point.x >= min.x && point.x <= max.x &&
            point.y >= min.y && point.y <= max.y &&
            point.z >= min.z && point.z <= max.z;
    }

    public bool contains(Box box) {
        return
            box.min.x >= min.x && box.max.x <= max.x &&
            box.min.y >= min.y && box.max.y <= max.y &&
            box.min.z >= min.z && box.max.z <= max.z;
    }

    public bool intersects(Box box) {
        return
            max.x >= box.min.x && min.x <= box.max.x &&
            max.y >= box.min.y && min.y <= box.max.y &&
            max.z >= box.min.z && min.z <= box.max.z;
    }

    public bool intersects(Plane plane) {
        var num1 = 0;
        var num2 = 0;
        if (plane.normal.x * min.x + plane.normal.y * min.y + plane.normal.z * min.z + plane.D > 0.0)
            num1 = 1;
        if (plane.normal.x * max.x + plane.normal.y * max.y + plane.normal.z * max.z + plane.D > 0.0)
            num2 = 1;
        return num1 != num2;
    }

    public bool intersects(Ray ray) {
        var tmin = (min - ray.origin) / ray.direction;
        var tmax = (max - ray.origin) / ray.direction;

        var t1 = min(tmin, tmax);
        var t2 = max(tmin, tmax);

        var t_near = max(max(t1.x, t1.y), t1.z);
        var t_far = min(min(t2.x, t2.y), t2.z);

        return t_near <= t_far;
    }

    public bool intersects(Frustum frustum) {
        return
            frustum.left_plane.intersects(this) &&
            frustum.right_plane.intersects(this) &&
            frustum.top_plane.intersects(this) &&
            frustum.bottom_plane.intersects(this) &&
            frustum.near_plane.intersects(this) &&
            frustum.far_plane.intersects(this);
    }

    public bool Equals(Box other)
        => min.Equals(other.min) && max.Equals(other.max);

    public override string ToString() => $"BB({min}:{max})";
}