using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public interface IVertexBuffer: IBuffer {
    IReadOnlyList<VertexAttribute> attributes { get; }
}

public static class VertexBuffer {
    public interface IPositions<T> where T: unmanaged {
        NativeView.Structs<T> positions { get; }
    }

    public interface INormals<T> where T: unmanaged {
        NativeView.Structs<T> normals { get; }
    }

    public sealed class Position_Normal<TPositions, TNormals>:
        VertexBuffer<Struct<TPositions, TNormals>>,
        IPositions<TPositions>,
        INormals<TNormals>

        where TPositions : unmanaged
        where TNormals : unmanaged {

        public NativeView.Structs<TPositions> positions { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public NativeView.Structs<TNormals> normals { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public Position_Normal(in Struct<TPositions, TNormals>[] vertices, params VertexAttribute[] attributes)
            : base(in vertices, [VertexAttribute<TPositions>.Position, VertexAttribute<TNormals>.Normal, ..attributes]) {

            positions = buffer.get_indexer<TPositions>(attributes[0].offset);
            normals = buffer.get_indexer<TNormals>(attributes[1].offset);
        }

        public Position_Normal(int count = 0, params VertexAttribute[] attributes)
            : base(count, [VertexAttribute<TPositions>.Position, VertexAttribute<TNormals>.Normal, ..attributes]) {

            positions = buffer.get_indexer<TPositions>(attributes[0].offset);
            normals = buffer.get_indexer<TNormals>(attributes[1].offset);
        }
    }
}

public class VertexBuffer<T>: Buffer<T>, IVertexBuffer where T: unmanaged {
    public IReadOnlyList<VertexAttribute> attributes { get; }

    public VertexBuffer(in T[] vertices, params VertexAttribute[] attributes): base(BufferTarget.ArrayBuffer, vertices) {
        this.attributes = attributes;
    }

    public VertexBuffer(int count, params VertexAttribute[] attributes): base(BufferTarget.ArrayBuffer, count) {
        this.attributes = attributes;
    }

    public VertexBuffer(IEnumerable<T> vertices, params VertexAttribute[] attributes): this(vertices.ToArray(),
        attributes) {}

    public override string ToString() => $"VertexBuffer<{nameof(T)}>";
}