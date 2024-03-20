namespace NetGL.Vectors;

using System.Numerics;

public static class Vector {
    public static Vector3<T> create<T>(this Vector3<T> other) where T: unmanaged, INumberBase<T>, IRootFunctions<T> {
        return new Vector3<T>().set(other);
    }

    public static Vector3<T> create<T>(this IVector3 other) where T: unmanaged, INumberBase<T>, IRootFunctions<T> {
        return new Vector3<T>().set((Vector3<T>)other);
    }

    public static Vector3<T> create<T>(this Vector3<float> other) where T: unmanaged, INumberBase<T>, IRootFunctions<T> {
        return new Vector3<T>().set(other);
    }

    public static Vector3<T> create<T>(this Vector3<double> other) where T: unmanaged, INumberBase<T>, IRootFunctions<T> {
        return new Vector3<T>().set(other);
    }

    public static Vector3<T> create<T>(this Vector3<Half> other) where T: unmanaged, INumberBase<T>, IRootFunctions<T> {
        return new Vector3<T>().set(other);
    }

}