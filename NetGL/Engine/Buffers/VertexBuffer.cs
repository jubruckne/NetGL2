using OpenTK.Mathematics;

namespace NetGL;

public interface IVertexBuffer: IBuffer {
    IReadOnlyList<VertexAttribute> get_vertex_spec();
}

public class VertexBuffer<T> : ArrayBuffer<T>, IVertexBuffer where T: struct, IVertexSpec {
    public VertexBuffer(int count): base(count) { }
    public VertexBuffer(in T[] vertices): base(vertices) { }

    public VertexBuffer(in Vector3[] vertices) : base(vertices.Length) {
        for (int index = 0; index < vertices.Length; index++) {
            ref var v = ref this[index];
            v.set(vertices[index]);
        }
    }

    IReadOnlyList<VertexAttribute> IVertexBuffer.get_vertex_spec() => T.get_vertex_spec();
}