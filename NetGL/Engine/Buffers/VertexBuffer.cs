using System.Buffers;
using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public interface IVertexBuffer: IBuffer {
    IReadOnlyList<VertexAttribute> attributes_definitions { get; }
    NativeView<V> get_attributes<V>(string attribute_name) where V: unmanaged;

    public interface Positions<P>: IVertexBuffer where P: unmanaged {
        NativeView<P> positions => get_attributes<P>("positions");
    }

    public interface Normals<N>: IVertexBuffer where N: unmanaged {
        NativeView<N> normals => get_attributes<N>("normals");
    }

    public interface PositionsNormals<P, N>: Positions<P>, Normals<N>
        where P : unmanaged where N : unmanaged;
}

public static class VertexBuffer {

}

public class VertexBuffer<T>: Buffer<T>, IVertexBuffer where T: unmanaged {
    public IReadOnlyList<VertexAttribute> attributes_definitions { get; }

    public VertexBuffer(in T[] vertices, params VertexAttribute[] attributes): base(BufferTarget.ArrayBuffer, vertices) {
        attributes_definitions = attributes;
    }

    public VertexBuffer(in ReadOnlySpan<T> vertices, params VertexAttribute[] attributes): base(BufferTarget.ArrayBuffer, vertices) {
        attributes_definitions = attributes;
    }


    public VertexBuffer(int count, params VertexAttribute[] attributes): base(BufferTarget.ArrayBuffer, count) {
        attributes_definitions = attributes;
    }

    public VertexBuffer(IEnumerable<T> vertices, params VertexAttribute[] attributes): this(vertices.ToArray(),
        attributes) {}

    public override string ToString() => $"VertexBuffer<{nameof(T)}>";

    public NativeView<V> get_attributes<V>(string attribute_name) where V : unmanaged {
        return buffer.get_element_view<V>(
            attributes_definitions.lookup(attribute => attribute.name == attribute_name).offset
        );
    }
}