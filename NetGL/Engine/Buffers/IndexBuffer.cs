using System.Numerics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace NetGL;

public abstract class IndexBuffer: Buffer<byte> {
    public abstract DrawElementsType draw_element_type { get; }
    public abstract PrimitiveType primitive_type { get; }
    public abstract int get_max_vertex_count();

    protected IndexBuffer(int byte_size) : base(BufferTarget.ElementArrayBuffer, byte_size) {}
    protected IndexBuffer(ReadOnlySpan<byte> data): base(BufferTarget.ElementArrayBuffer, data) {}

    public static IndexBuffer create(ReadOnlySpan<Vector3i> items, int vertex_count = ushort.MaxValue) {
        return vertex_count switch {
            < ushort.MaxValue => new IndexBuffer<ushort>(items.Length),
            _ => new IndexBuffer<int>(items.cast_to<int>())
        };
    }

    public static IndexBuffer create(ReadOnlySpan<int> items, int vertex_count = ushort.MaxValue) {
        IndexBuffer ib = vertex_count switch {
            < ushort.MaxValue => new IndexBuffer<ushort>(items.Length),
            _ => new IndexBuffer<int>(items.Length)
        };

        return ib;
    }


    public static IndexBuffer create<T>(int capacity) where T: unmanaged, IBinaryInteger<T> {
        return new IndexBuffer<T>(capacity);
    }

    public static IndexBuffer create<T>(ReadOnlySpan<IndexBuffer<T>.Index> data) where T: unmanaged, IBinaryInteger<T> {
        return new IndexBuffer<T>(data);
    }


    public abstract void bind();
    public abstract int count { get; }
    public abstract int item_size { get; }
    public abstract Type item_type { get; }
    public abstract int size { get; }
    public abstract void upload();
    public abstract Buffer.Status status { get; }
}

public class IndexBuffer<T>: IndexBuffer where T: unmanaged, INumberBase<T> {
    public readonly struct Index {
        public readonly T p1, p2, p3;

        private Index(T p1, T p2, T p3) {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        public static implicit operator Index((T p1, T p2, T p3) index)
            => new (index.p1, index.p2, index.p3);

        public static implicit operator Index(Vector3i index)
            => new (T.CreateChecked(index.X), T.CreateChecked(index.Y),T.CreateChecked(index.Z));

        public static implicit operator Index((int p1, int p2, int p3) index)
            => new (T.CreateChecked(index.p1), T.CreateChecked(index.p2),T.CreateChecked(index.p3));
    }

    public override int get_max_vertex_count() => T.One switch {
        byte => byte.MaxValue,
        ushort => ushort.MaxValue,
        short => short.MaxValue,
        int => int.MaxValue,
        uint => int.MaxValue,
        _ => throw new ArgumentOutOfRangeException(nameof(T), $"Unexpected type {typeof(T).Name}!")
    };

    public override int size { get; }

    public override void bind() {
        throw new NotImplementedException();
    }

    public override Status status { get; }

    public override void upload() {
        throw new NotImplementedException();
    }

    public override int count { get; }
    public override int item_size { get; }
    public override Type item_type { get; }

    public IndexBuffer(int triangle_count): base(triangle_count * T.One.size_of()) { }
    public IndexBuffer(ReadOnlySpan<Index> data): base(data.cast_to<Index, byte>()) { }
    public IndexBuffer(ReadOnlySpan<T> data): base(data.cast_to<T, byte>()) { }

    public NativeView<T> get_view() => buffer.as_view<T>();

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
        for (var index = 0; index < buffer.length; index += 3) {
            (buffer[index], buffer[index + 2]) = (buffer[index + 2], buffer[index]);
        }
    }
}