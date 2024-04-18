using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace NetGL;

public interface IIndexBuffer: IBuffer, IBindable {
    DrawElementsType draw_element_type { get; }
    PrimitiveType primitive_type { get; }
    int max_vertex_count { get; }

    void create();
    void update();
}

public static class IndexBuffer {
    public static IIndexBuffer create(ReadOnlySpan<Vector3i> items, int vertex_count) {
        return vertex_count switch {
            < ushort.MaxValue => new IndexBuffer<ushort>(items.cast_to<ushort>()),
            _ => new IndexBuffer<int>(items.reinterpret_as<Vector3i, Index<int>>())
        };
    }

    public static IIndexBuffer create(int index_count, int vertex_count) {
        return vertex_count switch {
            < ushort.MaxValue => new IndexBuffer<ushort>(index_count),
            _ => new IndexBuffer<int>(index_count)
        };
    }

    public static void unbind()
        => GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
}

[StructLayout(LayoutKind.Sequential)]
public struct Index<T> where T: unmanaged, IBinaryInteger<T> {
    public T p0, p1, p2;

    [MethodImpl(MethodImplOptions.AggressiveInlining), SkipLocalsInit]
    public Index(T p0, T p1, T p2) {
        this.p0 = p0;
        this.p1 = p1;
        this.p2 = p2;
    }

    public static readonly int max_vertex_count = T.One switch {
        byte   => byte.MaxValue,
        ushort => ushort.MaxValue,
        short  => short.MaxValue,
        int    => int.MaxValue,
        uint   => int.MaxValue,
        _      => throw new ArgumentOutOfRangeException(nameof(T), $"Unexpected type {typeof(T).Name}!")
    };

    public override string ToString() => $"<{p0},{p1},{p2}>";
    public override int GetHashCode() => HashCode.Combine(p0, p1, p2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Index<T>((T p1, T p2, T p3) index)
        => new (index.p1, index.p2, index.p3);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Index<T>(Vector3i index)
        => new (T.CreateChecked(index.X), T.CreateChecked(index.Y),T.CreateChecked(index.Z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Index<T>((int p1, int p2, int p3) index)
        => new (T.CreateChecked(index.p1), T.CreateChecked(index.p2),T.CreateChecked(index.p3));
}

public class IndexBuffer<T>: Buffer<Index<T>>, IIndexBuffer where T: unmanaged, IBinaryInteger<T> {
    public IndexBuffer(int triangle_count): base(BufferTarget.ElementArrayBuffer, triangle_count) { }
    public IndexBuffer(ReadOnlySpan<Index<T>> data): base(BufferTarget.ElementArrayBuffer, data) { }
    public IndexBuffer(ReadOnlySpan<T> data): base(BufferTarget.ElementArrayBuffer, data.reinterpret_as<T, Index<T>>()) { }

    public DrawElementsType draw_element_type => T.One switch {
        byte => DrawElementsType.UnsignedByte,
        short => DrawElementsType.UnsignedShort,
        ushort => DrawElementsType.UnsignedShort,
        int => DrawElementsType.UnsignedInt,
        uint => DrawElementsType.UnsignedInt,
        _ => throw new InvalidOperationException("Unsupported type")
    };

    public PrimitiveType primitive_type => PrimitiveType.Triangles;

    public (int min, int max) calculate_vertex_range() {
        int min = int.MaxValue, max = 0;

        foreach (var i in indices) {
            var p1 = int.CreateChecked(i.p0);
            var p2 = int.CreateChecked(i.p1);
            var p3 = int.CreateChecked(i.p2);

            min = int.Min(min, p1);
            min = int.Min(min, p2);
            min = int.Min(min, p3);

            max = int.Max(max, p1);
            max = int.Max(max, p2);
            max = int.Max(max, p3);
        }

        return (min, max);
    }

    public int max_vertex_count => Index<T>.max_vertex_count;

    public ArrayView<Index<T>> indices => get_view();
    public ArrayView<T> indices_individual => get_view<T>();

    public void reverse_winding() {
        for (var index = 0; index < buffer.length; index += 3) {
            (buffer[index], buffer[index + 2]) = (buffer[index + 2], buffer[index]);
        }
    }

    public void bind() => bind_buffer();
}