using System.Numerics;

namespace NetGL.Vectors;

public interface IVector2<T> where T: unmanaged, INumberBase<T> {
    T x { get; set; }
    T y { get; set; }
    void set(T x, T y);
}

public interface IVector3<T>: IVector2<T> where T: unmanaged, INumberBase<T> {
    T z { get; set; }
    void set(T x, T y, T z);
}

public interface IVector4<T>: IVector3<T> where T: unmanaged, INumberBase<T> {
    T w { get; set; }
    void set(T x, T y, T z, T w);
}

public struct Vector2<T>: IVector2<T> where T: unmanaged, INumberBase<T> {
    public T x;
    public T y;

    public Vector2(T x, T y) {
        this.x = x;
        this.y = y;
    }

    T IVector2<T>.x { get => x; set => x = value; }
    T IVector2<T>.y { get => y; set => y = value; }

    public void set(T x, T y) {
        this.x = x;
        this.y = y;
    }

    public static implicit operator Vector2<T>((T x, T y) vector) {
        return new Vector2<T>(vector.x, vector.y);
    }
}

public struct Vector3<T>: IVector3<T> where T: unmanaged, INumberBase<T> {
    public T x;
    public T y;
    public T z;

    public Vector3(T x, T y, T z) {
        this.x = x;
        this.y = y;
        this.z = z;
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

    T IVector2<T>.x { get => x; set => x = value; }
    T IVector2<T>.y { get => y; set => y = value; }
    T IVector3<T>.z { get => z; set => z = value; }

    public static implicit operator Vector3<T>((T x, T y, T z) vector) {
        return new Vector3<T>(vector.x, vector.y, vector.z);
    }
}

public struct Vector4<T>: IVector4<T> where T: unmanaged, INumberBase<T> {
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

    T IVector2<T>.x { get => x; set => x = value; }
    T IVector2<T>.y { get => y; set => y = value; }
    T IVector3<T>.z { get => z; set => z = value; }
    T IVector4<T>.w { get => w; set => w = value; }

    public static implicit operator Vector4<T>((T x, T y, T z, T w) vector) {
        return new Vector4<T>(vector.x, vector.y, vector.z, vector.w);
    }
}