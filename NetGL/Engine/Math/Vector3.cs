using System.Numerics;

namespace NetGL.Vectors;

public partial struct Vector3<T>:
    IVector3,
    IEquatable<Vector3<T>>,
    IAdditionOperators<Vector3<T>, Vector3<T>, Vector3<T>>,
    ISubtractionOperators<Vector3<T>, Vector3<T>, Vector3<T>>,
    IMultiplyOperators<Vector3<T>, Vector3<T>, Vector3<T>>,
    IMultiplyOperators<Vector3<T>, T, Vector3<T>>,
    IDivisionOperators<Vector3<T>, Vector3<T>, Vector3<T>>,
    IDivisionOperators<Vector3<T>, T, Vector3<T>>,
    IUnaryPlusOperators<Vector3<T>, Vector3<T>>,
    IUnaryNegationOperators<Vector3<T>, Vector3<T>>,
    IEqualityOperators<Vector3<T>, Vector3<T>, bool>
    where T: unmanaged, INumberBase<T> {

    public T x;
    public T y;
    public T z;

    public Vector3() {}

    public Vector3(Vector3<T> other) {
        this.x = other.x;
        this.y = other.y;
        this.z = other.z;
    }

    public Vector3(T x, T y, T z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3<T> set(T x, T y, T z) {
        this.x = x;
        this.y = y;
        this.z = z;
        return this;
    }

    public Vector3<T> set<P>(P x, P y, P z)
        where P: unmanaged, INumberBase<P>, IRootFunctions<P> {
        this.x = T.CreateSaturating(x);
        this.y = T.CreateSaturating(y);
        this.z = T.CreateSaturating(z);
        return this;
    }

    public Vector3<T> set(in Vector3<T> other) {
        x = other.x;
        y = other.y;
        z = other.z;
        return this;
    }

    public Vector3<T> set<P>(Vector3<P> other)
        where P: unmanaged, INumberBase<P> {
        x = T.CreateSaturating(other.x);
        y = T.CreateSaturating(other.y);
        z = T.CreateSaturating(other.z);
        return this;
    }

    public static implicit operator Vector3<T>((T x, T y, T z) other) =>
        new(other.x, other.y, other.z);

    public static implicit operator Vector3<T>(OpenTK.Mathematics.Vector3 other) =>
        new Vector3<T>().set(other.X, other.Y, other.Z);

    public static explicit operator OpenTK.Mathematics.Vector3(Vector3<T> vector) {
        return new(
                   float.CreateSaturating(vector.x),
                   float.CreateSaturating(vector.y),
                   float.CreateSaturating(vector.z)
                  );
    }

    public static explicit operator System.Numerics.Vector3(Vector3<T> vector) {
        return new(
                   float.CreateSaturating(vector.x),
                   float.CreateSaturating(vector.y),
                   float.CreateSaturating(vector.z)
                  );
    }
}

public partial struct Vector3<T> {
    public static Vector3<T> operator+(Vector3<T> value) {
        return value;
    }

    public static Vector3<T> operator-(Vector3<T> value) {
        return new Vector3<T>().set(-value.x, -value.y, -value.z);
    }

    public static Vector3<T> operator+(Vector3<T> left, Vector3<T> right) {
        left.x += right.x;
        left.y += right.y;
        left.z += right.z;
        return left;
    }

    public static Vector3<T> operator-(Vector3<T> left, Vector3<T> right) {
        left.x -= right.x;
        left.y -= right.y;
        left.z -= right.z;
        return left;
    }

    public static Vector3<T> operator*(Vector3<T> left, Vector3<T> right) {
        left.x *= right.x;
        left.y *= right.y;
        left.z *= right.y;
        return left;
    }
    public static Vector3<T> operator*(Vector3<T> left, T right) {
        left.x *= right;
        left.y *= right;
        left.z *= right;
        return left;
    }

    public static Vector3<T> operator/(Vector3<T> left, Vector3<T> right) {
        left.x /= right.x;
        left.y /= right.y;
        left.z /= right.z;
        return left;
    }

    public static Vector3<T> operator/(Vector3<T> left, T right) {
        left.x /= right;
        left.y /= right;
        left.z /= right;
        return left;
    }

    public static bool operator==(Vector3<T> left, Vector3<T> right) {
        return left.x == right.x && left.y == right.y && left.z == right.z;
    }

    public static bool operator!=(Vector3<T> left, Vector3<T> right) {
        return left.x != right.x || left.y != right.y || left.z != right.z;
    }
}

public partial struct Vector3<T> {
    public bool Equals(Vector3<T> other) {
        return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
    }

    public override bool Equals(object? obj) {
        return obj is Vector3<T> other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(x, y, z);
    }

    public override string ToString() => $"({x}, {y}, {z})";
}