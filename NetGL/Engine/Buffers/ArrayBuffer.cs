using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public class ArrayBuffer<T>: Buffer<T> where T: struct {
    public ArrayBuffer() : base(BufferTarget.ArrayBuffer) { }

    public ArrayBuffer(int count) : base(BufferTarget.ArrayBuffer, count) { }

    public ArrayBuffer(in T[] items) : base(BufferTarget.ArrayBuffer, items.Length) {
        insert(0, items);
    }
}