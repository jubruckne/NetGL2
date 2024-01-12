using System.Numerics;
using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public class IndexBuffer : IndexBuffer<ushort> {
    public IndexBuffer() : base() {}

    public IndexBuffer(int count) : base(count) { }

    public IndexBuffer(in Triangle[] items) : base(items) { }
}

public class IndexBuffer<T>: Buffer<IndexBuffer<T>.Triangle> where T: unmanaged, IUnsignedNumber<T> {
    public struct Triangle {
        public T p1, p2, p3;

        public void set(T p1, T p2, T p3) {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }
    }

    public IndexBuffer() : base(BufferTarget.ElementArrayBuffer, 0) { }

    public IndexBuffer(int count) : base(BufferTarget.ElementArrayBuffer, count) { }

    public IndexBuffer(in Triangle[] items) : base(BufferTarget.ElementArrayBuffer, items.Length) {
        var triangles = new Triangle[items.Length];
        insert(0, triangles);
    }
}