namespace NetGL.Vectors;

using System.Numerics;

public partial struct vec3<T>:
    ivec3<T>,
    IEquatable<vec3<T>>
    where T: unmanaged, INumber<T> {

    public T x;
    public T y;
    public T z;

    public vec3() {}

    public vec3(vec3<T> other) {
        this.x = other.x;
        this.y = other.y;
        this.z = other.z;
    }

    public vec3(T x, T y, T z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public vec3<T> set(T x, T y, T z) {
        this.x = x;
        this.y = y;
        this.z = z;
        return this;
    }

    public vec3<T> set<P>(P x, P y, P z)
        where P: unmanaged, INumberBase<P>, IRootFunctions<P> {
        this.x = T.CreateSaturating(x);
        this.y = T.CreateSaturating(y);
        this.z = T.CreateSaturating(z);
        return this;
    }

    public vec3<T> set(in vec3<T> other) {
        x = other.x;
        y = other.y;
        z = other.z;
        return this;
    }

    public vec3<T> set<P>(vec3<P> other)
        where P: unmanaged, INumber<P> {
        x = T.CreateSaturating(other.x);
        y = T.CreateSaturating(other.y);
        z = T.CreateSaturating(other.z);
        return this;
    }

    public static implicit operator vec3<T>((T x, T y, T z) other) =>
        new(other.x, other.y, other.z);

    public static implicit operator vec3<T>(OpenTK.Mathematics.Vector3 other) =>
        new vec3<T>().set(other.X, other.Y, other.Z);

    public static explicit operator half3(vec3<T> other) =>
        new half3().set(other);

    public static explicit operator float3(vec3<T> other) =>
        new float3().set(other);

    public static explicit operator double3(vec3<T> other) =>
        new double3().set(other);

    public static explicit operator OpenTK.Mathematics.Vector3(vec3<T> vector) {
        return new(
                   float.CreateSaturating(vector.x),
                   float.CreateSaturating(vector.y),
                   float.CreateSaturating(vector.z)
                  );
    }

    public static explicit operator System.Numerics.Vector3(vec3<T> vector) {
        return new(
                   float.CreateSaturating(vector.x),
                   float.CreateSaturating(vector.y),
                   float.CreateSaturating(vector.z)
                  );
    }

    T[] ivec<T>.array => [x, y, z];
    T ivec2<T>.x => x;
    T ivec2<T>.y => y;
    T ivec3<T>.z => z;
    I[] ivec.get_array<I>() => new T[] { x, y, z }.Cast<I>().ToArray();
}

public partial struct vec3<T> {
    public static vec3<T> operator+(vec3<T> value) {
        return value;
    }

    public static vec3<T> operator-(vec3<T> value) {
        return new vec3<T>().set(-value.x, -value.y, -value.z);
    }

    public static vec3<T> operator+(vec3<T> left, vec3<T> right) {
        left.x += right.x;
        left.y += right.y;
        left.z += right.z;
        return left;
    }

    public static vec3<T> operator-(vec3<T> left, vec3<T> right) {
        left.x -= right.x;
        left.y -= right.y;
        left.z -= right.z;
        return left;
    }

    public static vec3<T> operator*(vec3<T> left, vec3<T> right) {
        left.x *= right.x;
        left.y *= right.y;
        left.z *= right.y;
        return left;
    }
    public static vec3<T> operator*(vec3<T> left, T right) {
        left.x *= right;
        left.y *= right;
        left.z *= right;
        return left;
    }

    public static vec3<T> operator/(vec3<T> left, vec3<T> right) {
        left.x /= right.x;
        left.y /= right.y;
        left.z /= right.z;
        return left;
    }

    public static vec3<T> operator/(vec3<T> left, T right) {
        left.x /= right;
        left.y /= right;
        left.z /= right;
        return left;
    }

    public static bool operator==(vec3<T> left, vec3<T> right) {
        return left.x == right.x && left.y == right.y && left.z == right.z;
    }

    public static bool operator!=(vec3<T> left, vec3<T> right) {
        return left.x != right.x || left.y != right.y || left.z != right.z;
    }

    public T length() {
        var v = (float3)this;
        return T.CreateSaturating(MathF.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z));
    }

    public void normalize() => this /= length();
}

public partial struct vec3<T> {
    public bool Equals(vec3<T> other) {
        return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
    }

    public override bool Equals(object? obj) {
        return obj is vec3<T> other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(x, y, z);
    }

    public override string ToString() => $"({x}, {y}, {z})";
}