using System.Numerics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace NetGL;

public interface IIndexBuffer : IBuffer {
    DrawElementsType draw_element_type { get; }
    PrimitiveType primitive_type { get; }
}

public class IndexBuffer<T>: Buffer<T>, IIndexBuffer
    where T: unmanaged, IBinaryInteger<T> {

    internal IndexBuffer(int triangle_count)
        : base(BufferTarget.ElementArrayBuffer, triangle_count * 3) {
    }

    internal IndexBuffer(in T[] items)
        : this(items.Length)
        => buffer = items;

    public static IndexBuffer<T> make(in byte[] items) {
        T test = T.Zero;

        if (test is byte)
            return new IndexBuffer<T>(items.reinterpret_cast<byte, T>());

        throw new InvalidOperationException($"Unsupported type {typeof(T).Name}!");
    }

    public static IndexBuffer<T> make(in int[] items) {
        T test = T.Zero;

        if (test is int)
            return new IndexBuffer<T>(items.reinterpret_cast<int, T>());

        throw new InvalidOperationException($"Unsupported type {typeof(T).Name}!");
    }

    public static IndexBuffer<T> make(in Vector3i[] items) {
        T test = T.Zero;

        if (test is byte) {
            byte[] target_items = new byte[items.Length * 3];

            for (int index = 0; index < items.Length; index++) {
                target_items[index * 3] = (byte)items[index].X;
                target_items[index * 3 + 1] = (byte)items[index].Y;
                target_items[index * 3 + 2] = (byte)items[index].Z;
            }

            return make(target_items);
        }

        throw new InvalidOperationException($"Unsupported type {typeof(T).Name}!");
    }

    public static IndexBuffer<T> make(IEnumerable<Vector3i> items) => make(items.ToArray());


    public DrawElementsType draw_element_type => typeof(T) switch {
        { } t when t == typeof(byte) => DrawElementsType.UnsignedByte,
        { } t when t == typeof(short) => DrawElementsType.UnsignedShort,
        { } t when t == typeof(ushort) => DrawElementsType.UnsignedShort,
        { } t when t == typeof(int) => DrawElementsType.UnsignedInt,
        { } t when t == typeof(uint) => DrawElementsType.UnsignedInt,
        _ => throw new InvalidOperationException("Unsupported type")
    };

    public PrimitiveType primitive_type => PrimitiveType.Triangles;

    public void reverse_winding() {
        for (var index = 0; index < buffer.Length; index += 3) {
            (buffer[index], buffer[index + 2]) = (buffer[index + 2], buffer[index]);
        }
    }
}