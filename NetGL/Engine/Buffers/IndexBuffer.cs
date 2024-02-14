using System.Numerics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace NetGL;

public interface IIndexBuffer : IBuffer {
    DrawElementsType draw_element_type { get; }
    PrimitiveType primitive_type { get; }
    int get_max_vertex_count();
}

public class IndexBuffer<T>: Buffer<T>, IIndexBuffer
    where T: unmanaged, IBinaryInteger<T> {

    int IIndexBuffer.get_max_vertex_count() => get_max_vertex_count();

    public static int get_max_vertex_count() => new T() switch {
        byte => byte.MaxValue,
        ushort => ushort.MaxValue,
        short => short.MaxValue,
        int => int.MaxValue,
        uint => int.MaxValue,
        _ => throw new ArgumentOutOfRangeException(nameof(T), $"Unecpected type {typeof(T).Name}!")
    };

    internal IndexBuffer(int triangle_count)
        : base(BufferTarget.ElementArrayBuffer, triangle_count * 3) {
    }

    internal IndexBuffer(in T[] items)
        : this(items.Length)
        => buffer = items;

    public static IndexBuffer<T> make(in byte[] items) {
        T test = T.Zero;

        if (test is byte)
            return new IndexBuffer<T>(items.reinterpret_ref<byte, T>());

        throw new InvalidOperationException($"Unsupported type {typeof(T).Name}!");
    }

    public static IndexBuffer<T> make(in ushort[] items) {
        T test = T.Zero;

        if (test is ushort)
            return new IndexBuffer<T>(items.reinterpret_ref<ushort, T>());

        throw new InvalidOperationException($"Unsupported type {typeof(T).Name}!");
    }


    public static IndexBuffer<T> make(in int[] items) {
        T test = T.Zero;

        if (test is int)
            return new IndexBuffer<T>(items.reinterpret_ref<int, T>());

        throw new InvalidOperationException($"Unsupported type {typeof(T).Name}!");
    }

    public static IndexBuffer<T> make(in Vector3i[] items) {
        T test = T.Zero;

        if (test is byte) {
            byte[] target_items = new byte[items.Length * 3];

            for (int index = 0; index < items.Length; index++) {
                ref var item = ref items[index];
                if (item.X > byte.MaxValue || item.Y > byte.MaxValue || item.Z > byte.MaxValue)
                    throw new OverflowException();

                target_items[index * 3] = (byte)item.X;
                target_items[index * 3 + 1] = (byte)item.Y;
                target_items[index * 3 + 2] = (byte)item.Z;
            }

            return make(target_items);
        }

        if (test is ushort) {
            ushort[] target_items = new ushort[items.Length * 3];

            for (int index = 0; index < items.Length; index++) {
                ref var item = ref items[index];
                if (item.X > ushort.MaxValue || item.Y > ushort.MaxValue || item.Z > ushort.MaxValue)
                    throw new OverflowException($"{item} does not fit in IndexBuffer<{typeof(T).Name}>!");

                target_items[index * 3] = (ushort)item.X;
                target_items[index * 3 + 1] = (ushort)item.Y;
                target_items[index * 3 + 2] = (ushort)item.Z;
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