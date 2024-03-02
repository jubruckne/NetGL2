using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace NetGL;


[StructLayout(LayoutKind.Sequential, Size = 16)]
public struct Size3<T> where T: unmanaged {
    public T width;
    public T height;
    public T depth;

    public Size3(T width, T height, T depth) {
        this.width = width;
        this.height = height;
        this.depth = depth;
    }

    public Size3(float width, float height, float depth) {
        this.width = width.reinterpret<float, T>();
        this.height = height.reinterpret<float, T>();
        this.depth = depth.reinterpret<float, T>();
    }

    public Size3(int width, int height, int depth) {
        this.width = width.reinterpret<int, T>();
        this.height = height.reinterpret<int, T>();
        this.depth = depth.reinterpret<int, T>();
    }

    public static Size3<T> unit_size = new(1f, 1f, 1f);

    public static implicit operator Size3<T>((float width, float height, float depth) size)
        => new(size.width.reinterpret<float, T>(), size.height.reinterpret<float, T>(), size.depth.reinterpret<float, T>());

    public static implicit operator Vector3(in Size3<T> size) {
        return size.reinterpret<Size3<T>, Vector3>();
    }
}

[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct Size2<T> where T: unmanaged {
    public T width;
    public T height;

    public Size2(T width, T height) {
        this.width = width;
        this.height = height;
    }

    public Size2(float width, float height) {
        this.width = width.reinterpret<float, T>();
        this.height = height.reinterpret<float, T>();
    }

    public Size2(int width, int height) {
        this.width = width.reinterpret<int, T>();
        this.height = height.reinterpret<int, T>();
    }

    public static Size2<T> unit_size = new(1f, 1f);

    public static implicit operator Size2<T>((float width, float height) size)
        => new(size.width.reinterpret<float, T>(), size.height.reinterpret<float, T>());

    public static implicit operator Vector2(in Size2<T> size) {
        return size.reinterpret<Size2<T>, Vector2>();
    }

    public static implicit operator Vector2i(in Size2<T> size) {
        return size.reinterpret<Size2<T>, Vector2i>();
    }
}