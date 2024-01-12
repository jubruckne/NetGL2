using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public struct Index {
    public ushort p1, p2, p3;
}

public class IndexBuffer: Buffer<Index> {
    public IndexBuffer() : base(BufferTarget.ElementArrayBuffer, 0) { }
    public IndexBuffer(int count) : base(BufferTarget.ElementArrayBuffer, count) { }
}