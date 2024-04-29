namespace NetGL;

public readonly struct Ray {
    public readonly float3 origin;
    public readonly float3 direction;

    public Ray() {
        origin    = float3.zero;
        direction = float3.unit_z;
    }

    private Ray(float3 origin, float3 direction) {
        this.origin    = origin;
        this.direction = direction;
    }

    public static Ray point_at(float3 origin, float3 target)
        => new Ray(origin, target - origin);

    public static Ray towards(float3 origin, float3 direction)
        => new Ray(origin, direction);

    public IEnumerable<Box> intersects(IEnumerable<Box> boxes) {
        foreach (var box in boxes) {
            if (intersects(box))
                yield return box;
        }
    }

    public bool intersects(Box box) {
        var tmin = (box.min - origin) / direction;
        var tmax = (box.max - origin) / direction;

        var t1   = min(tmin, tmax);
        var t2   = max(tmin, tmax);

        var t_near = max(max(t1.x, t1.y), t1.z);
        var t_far  = min(min(t2.x, t2.y), t2.z);

        return t_near <= t_far;
    }

    public bool intersects(Plane plane) {
        var t = -(plane.normal.x * origin.x + plane.normal.y * origin.y + plane.normal.z * origin.z + plane.D) /
                (plane.normal.x * direction.x + plane.normal.y * direction.y + plane.normal.z * direction.z);
        return t >= 0;
    }

    public IEnumerable<Plane> intersects(IEnumerable<Plane> planes) {
        foreach (var plane in planes) {
            if (intersects(plane))
                yield return plane;
        }
    }

    public bool intersects(Frustum frustum) {
        return frustum.left_plane.intersects(this) ||
               frustum.right_plane.intersects(this) ||
               frustum.top_plane.intersects(this) ||
               frustum.bottom_plane.intersects(this) ||
               frustum.near_plane.intersects(this) ||
               frustum.far_plane.intersects(this);
    }

    public bool intersects(Ray ray) {
        var a = direction;
        var b = ray.direction;
        var c = origin - ray.origin;

        var d = cross(a, b);
        var e = cross(b, c);
        var f = cross(c, a);

        var div = dot(d, d);
        var t = dot(e, d) / div;
        var u = dot(f, d) / div;

        return t >= 0 && u is >= 0 and <= 1;
    }
}