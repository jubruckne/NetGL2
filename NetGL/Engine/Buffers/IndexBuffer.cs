using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace NetGL;

public interface IIndexBuffer: IBuffer {
    DrawElementsType draw_element_type { get; }
    PrimitiveType primitive_type { get; }
    int max_vertex_count { get; }
}

public static class IndexBuffer {
    public static IIndexBuffer create(ReadOnlySpan<Vector3i> items, int vertex_count) {
        return vertex_count switch {
            < ushort.MaxValue => new IndexBuffer<ushort>(items.cast_to<ushort>()),
            _ => new IndexBuffer<int>(items.reinterpret_as<Vector3i, Index<int>>())
        };
    }
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct Index<T> where T: unmanaged, INumberBase<T> {
    public readonly T p1, p2, p3;

    private Index(T p1, T p2, T p3) {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
    }

    public static implicit operator Index<T>((T p1, T p2, T p3) index)
        => new (index.p1, index.p2, index.p3);

    public static implicit operator Index<T>(Vector3i index)
        => new (T.CreateChecked(index.X), T.CreateChecked(index.Y),T.CreateChecked(index.Z));
}

public class IndexBuffer<T>: Buffer<Index<T>>, IIndexBuffer where T: unmanaged, INumberBase<T> {
    public int max_vertex_count => T.One switch {
        byte => byte.MaxValue,
        ushort => ushort.MaxValue,
        short => short.MaxValue,
        int => int.MaxValue,
        uint => int.MaxValue,
        _ => throw new ArgumentOutOfRangeException(nameof(T), $"Unexpected type {typeof(T).Name}!")
    };

    public IndexBuffer(int triangle_count): base(BufferTarget.ElementArrayBuffer,triangle_count * Unsafe.SizeOf<Index>()) { }
    public IndexBuffer(ReadOnlySpan<Index<T>> data): base(BufferTarget.ElementArrayBuffer, data) { }
    public IndexBuffer(ReadOnlySpan<T> data): base(BufferTarget.ElementArrayBuffer, data.reinterpret_as<T, Index<T>>()) { }

    public NativeArray<Index<T>>.View<Index<T>> view() => buffer.view<Index<T>>();

    /*
    internal IndexBuffer(in Vector3i[] items) : base(BufferTarget.ElementArrayBuffer, items) {
    }
*/
/*
    public static IndexBuffer<T> make(in byte[] items) {
        if (T.Zero is byte)
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
        if (T.Zero is byte) {
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

        if (T.Zero is ushort) {
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

        if (T.Zero is int) {
            return new IndexBuffer<T>(items.reinterpret_ref<Vector3i, T>().ToArray());
        }

        throw new InvalidOperationException($"Unsupported type {typeof(T).Name}!");
    }

    public static IndexBuffer<T> make(IEnumerable<Vector3i> items) => make(items.ToArray());
*/

    public DrawElementsType draw_element_type => T.One switch {
        byte => DrawElementsType.UnsignedByte,
        short => DrawElementsType.UnsignedShort,
        ushort => DrawElementsType.UnsignedShort,
        int => DrawElementsType.UnsignedInt,
        uint => DrawElementsType.UnsignedInt,
        _ => throw new InvalidOperationException("Unsupported type")
    };

    public PrimitiveType primitive_type => PrimitiveType.Triangles;

    public void reverse_winding() {
        for (var index = 0; index < buffer.length; index += 3) {
            (buffer[index], buffer[index + 2]) = (buffer[index + 2], buffer[index]);
        }
    }
}