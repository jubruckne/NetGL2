using System.Numerics;

namespace NetGL.Vectors;

public partial struct vec4<T>:
    ivec4<T> ,
    IEquatable<vec4<T>>
    where T: unmanaged, INumber<T> {

    public T x;
    public T y;
    public T z;
    public T w;

    public vec4(in vec3<T> xyz, T w) {
        this.x = xyz.x;
        this.y = xyz.y;
        this.z = xyz.z;
        this.w = w;
    }

    public vec4(T x, T y, T z, T w) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public void set(T x, T y) {
        this.x = x;
        this.y = y;
    }

    public void set(T x, T y, T z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public void set(T x, T y, T z, T w) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public static implicit operator vec4<T>((T x, T y, T z, T w) vector) =>
        new vec4<T>(vector.x, vector.y, vector.z, vector.w);

    I[] ivec.get_array<I>() => new T[]{x, y, z, w}.Cast<I>().ToArray();
    T[] ivec<T>.array => [x, y, z, w];
    T ivec2<T>.x => x;
    T ivec2<T>.y => y;
    T ivec3<T>.z => z;
    T ivec4<T>.w => w;
}

public partial struct vec4<T> {
    public bool Equals(vec4<T> other)
        => x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);

    public override bool Equals(object? obj)
        => obj is vec3<T> other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(x, y, z, w);

    public override string ToString() => $"({x}, {y}, {z}, {w})";
}