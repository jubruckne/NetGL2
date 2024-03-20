using System.Numerics;
using NetGL.Vectors;

public struct Vector2<T>: IVector2 where T: unmanaged, INumberBase<T> {
    public T x;
    public T y;

    public Vector2(T x, T y) {
        this.x = x;
        this.y = y;
    }

    public void set(T x, T y) {
        this.x = x;
        this.y = y;
    }

    public static implicit operator Vector2<T>((T x, T y) vector)
        => new Vector2<T>(vector.x, vector.y);

    public override string ToString() => $"({x}, {y})";
}

public struct Vector3<T>:
    IVector3,
    IAdditionOperators<Vector3<T>, Vector3<T>, Vector3<T>>,
    ISubtractionOperators<Vector3<T>, Vector3<T>, Vector3<T>>,
    IMultiplyOperators<Vector3<T>, Vector3<T>, Vector3<T>>,
    IMultiplyOperators<Vector3<T>, T, Vector3<T>>,
    IDivisionOperators<Vector3<T>, T, Vector3<T>>
    where T: unmanaged, INumberBase<T>, IRootFunctions<T> {

    public T x;
    public T y;
    public T z;

    public Vector3() {}

    private Vector3(Vector3<T> other) {
        this.x = other.x;
        this.y = other.y;
        this.z = other.z;
    }

    private Vector3(T x, T y, T z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3<T> set(T x, T y) {
        this.x = x;
        this.y = y;
        return this;
    }

    public Vector3<T> set(T x, T y, T z) {
        this.x = x;
        this.y = y;
        this.z = z;
        return this;
    }

    public Vector3<T> set<P>(P x, P y, P z) where P: unmanaged, INumberBase<P>, IRootFunctions<P> {
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

    public Vector3<T> set<P>(Vector3<P> other) where P: unmanaged, INumberBase<P>, IRootFunctions<P> {
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
        return new OpenTK.Mathematics.Vector3(
                                              float.CreateSaturating(vector.x),
                                              float.CreateSaturating(vector.y),
                                              float.CreateSaturating(vector.z)
                                             );
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

    public static Vector3<T> operator/(Vector3<T> left, T right) {
        left.x /= right;
        left.y /= right;
        left.z /= right;
        return left;
    }

    public Vector3<T> normalize() => this /= length();

    public T length() => T.Sqrt(x * x + y * y + z * z);

    public P length<P>() where P: unmanaged, INumberBase<P>, IRootFunctions<P> {
        var v      = new Vector3<float>().set(this);
        var length_squared = v.length();
        var length = float.Sqrt(length_squared);
        return P.CreateSaturating(length);
    }

    public override string ToString() => $"({x}, {y}, {z})";
}

public struct Vector4<T>: IVector4 where T: unmanaged, INumberBase<T> {
    public T x;
    public T y;
    public T z;
    public T w;

    public Vector4(T x, T y, T z, T w) {
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

    public static implicit operator Vector4<T>((T x, T y, T z, T w) vector) =>
        new Vector4<T>(vector.x, vector.y, vector.z, vector.w);

    public override string ToString() => $"({x}, {y}, {z}, {w})";
}