namespace NetGL.Vectors;

using System.Numerics;

public static partial class Vector {
    public static Vector3<T> create<T>(Vector3<T> other) where T: unmanaged, INumberBase<T>
        => new Vector3<T>(other);

    public static Vector3<T> create<T>(T x, T y, T z) where T: unmanaged, INumberBase<T>
        => new Vector3<T>(x, y, z);

    public static Vector3<T> create<T>(Vector3<int> other) where T: unmanaged, INumberBase<T>
        => new Vector3<T>().set(other);

    public static Vector3<T> create<T>(Vector3<short> other) where T: unmanaged, INumberBase<T>
        => new Vector3<T>().set(other);

    public static Vector3<T> create<T>(Vector3<float> other) where T: unmanaged, INumberBase<T>
        => new Vector3<T>().set(other);

    public static Vector3<T> create<T>(Vector3<double> other) where T: unmanaged, INumberBase<T>
        => new Vector3<T>().set(other);

    public static Vector3<T> create<T>(Vector3<Half> other) where T: unmanaged, INumberBase<T>
        => new Vector3<T>().set(other);
}

public static partial class Vector {
    public static Vector3<double> normalize(this Vector3<double> vector) => vector /= vector.length();
    public static Vector3<float> normalize(this Vector3<float> vector) => vector /= vector.length();
    public static Vector3<System.Half> normalize(this Vector3<System.Half> vector) => vector /= vector.length();

    public static double length(this Vector3<double> vector)
        => Math.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);

    public static float length(this Vector3<float> vector)
        => MathF.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);

    public static Half length(this Vector3<Half> vector)
        => Half.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
}