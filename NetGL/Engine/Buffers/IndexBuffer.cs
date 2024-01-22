using System.Numerics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public interface IIndexBuffer : IBuffer {
    DrawElementsType draw_element_type { get; }
    PrimitiveType primitive_type { get; }
}

public class IndexBuffer : IndexBuffer<ushort>, IIndexBuffer {
    public IndexBuffer() : base() {}

    public IndexBuffer(int count) : base(count) { }

    public IndexBuffer(in Triangle[] items) : base(items) { }
}

public class IndexBuffer<T> : Buffer<IndexBuffer<T>.Triangle>, IIndexBuffer where T : unmanaged, IUnsignedNumber<T> {
    [StructLayout(LayoutKind.Sequential)]
    public struct Triangle: IIndexSpec {
        public T p1, p2, p3;

        public void set(T p1, T p2, T p3) {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        public static int get_point_count() => 3;
    }

    public IndexBuffer() : base(BufferTarget.ElementArrayBuffer, 0) {
    }

    public IndexBuffer(int count) : base(BufferTarget.ElementArrayBuffer, count) {
    }

    public IndexBuffer(in Triangle[] items) : base(BufferTarget.ElementArrayBuffer, items.Length) {
        insert(0, items);
    }

    public DrawElementsType draw_element_type => typeof(T) switch {
        { } t when t == typeof(byte) => DrawElementsType.UnsignedByte,
        { } t when t == typeof(ushort) => DrawElementsType.UnsignedShort,
        { } t when t == typeof(uint) => DrawElementsType.UnsignedInt,
        _ => throw new InvalidOperationException("Unsupported type")
    };

    public PrimitiveType primitive_type => PrimitiveType.Triangles;
}