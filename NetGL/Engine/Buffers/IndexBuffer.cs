using System.Numerics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public interface IIndexBuffer : IBuffer {
    DrawElementsType draw_element_type { get; }
    PrimitiveType primitive_type { get; }
}

public static class IndexBuffer {
    public static IndexBuffer<T> make<T>(in T[] items) where T : unmanaged, INumber<T> {
        return new IndexBuffer<T>(items);
    }

    public static IndexBuffer<T> make<T>(in IEnumerable<T> items) where T : unmanaged, INumber<T> {
        return new IndexBuffer<T>(items.ToArray());
    }
}

public class IndexBuffer<T>: Buffer<T>, IIndexBuffer
    where T: unmanaged, INumber<T> {

    internal IndexBuffer(int triangle_count)
        : base(BufferTarget.ElementArrayBuffer, triangle_count * 3) {
    }

    internal IndexBuffer(in T[] items)
        : this(items.Length)
        => buffer = items;

    public DrawElementsType draw_element_type => typeof(T) switch {
        { } t when t == typeof(byte) => DrawElementsType.UnsignedByte,
        { } t when t == typeof(short) => DrawElementsType.UnsignedShort,
        { } t when t == typeof(ushort) => DrawElementsType.UnsignedShort,
        { } t when t == typeof(int) => DrawElementsType.UnsignedInt,
        { } t when t == typeof(uint) => DrawElementsType.UnsignedInt,
        _ => throw new InvalidOperationException("Unsupported type")
    };

    public PrimitiveType primitive_type => PrimitiveType.Triangles;
}