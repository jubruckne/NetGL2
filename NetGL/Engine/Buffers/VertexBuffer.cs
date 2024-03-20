using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public interface IVertexBuffer: IBuffer {
    ReadOnlyMap<string, VertexAttribute> attribute_definitions { get; }
}

public class VertexBuffer<T>: Buffer<T>, IVertexBuffer where T: unmanaged {
    public ReadOnlyMap<string, VertexAttribute> attribute_definitions { get; } = new Map<string, VertexAttribute>();

    public VertexBuffer(in ReadOnlySpan<T> vertices, params VertexAttribute[] attribute_definitions): base(BufferTarget.ArrayBuffer, vertices) {
        attribute_def_init(attribute_definitions);
    }

    public VertexBuffer(int count, params VertexAttribute[] attribute_definitions): base(BufferTarget.ArrayBuffer, count) {
        attribute_def_init(attribute_definitions);
    }

    private void attribute_def_init(in VertexAttribute[] attribute_definitions) {
        var offset = 0;
        foreach (var attrib in attribute_definitions) {
            attrib.offset = offset;
            offset += attrib.size_of;
            this.attribute_definitions.writeable().add(attrib.name, attrib);
        }

        Error.assert((offset, Unsafe.SizeOf<T>()), offset == Unsafe.SizeOf<T>());
    }

    public ArrayView<T> vertices => get_view();
}

public class VertexBuffer<TPosition, TNormal>: VertexBuffer<VertexBuffer<TPosition,TNormal>.Vertex>
    where TPosition: unmanaged
    where TNormal: unmanaged {

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex {
        public TPosition position;
        public TNormal normal;

        public Vertex(TPosition position, TNormal normal) {
            this.position = position;
            this.normal   = normal;
        }
    }

    public VertexBuffer(in ReadOnlySpan<TPosition> positions, in ReadOnlySpan<TNormal> normals)
        : base(
               positions.Length,
               [
                   new VertexAttribute<TPosition>("position", 3, false),
                   new VertexAttribute<TNormal>("normal", 3, false),
               ]
              ) {
        var writer = vertices.new_writer();
        for (int index = 0; index < positions.Length; index++) {
            writer.write(new Vertex(positions[index], normals[index]));
        }

    }

    public VertexBuffer(in ReadOnlySpan<Vertex> vertices)
        : base(
               vertices,
               [
                   new VertexAttribute<TPosition>("position", 3, false),
                   new VertexAttribute<TNormal>("normal", 3, false),
               ]
              ) {}

    public VertexBuffer(int count)
        : base(
               count,
               [
                   new VertexAttribute<TPosition>("position", 3, false),
                   new VertexAttribute<TNormal>("normal", 3, false),
               ]
              ) {}

    public ArrayView<TPosition> positions
        => get_view<TPosition>("position");

    public ArrayView<TNormal> normals
        => get_view<TNormal>("normal");
}

public class VertexBuffer<TPosition, TNormal, TAttributes>: VertexBuffer<VertexBuffer<TPosition,TNormal,TAttributes>.Vertex>
    where TPosition: unmanaged
    where TNormal: unmanaged
    where TAttributes: unmanaged {

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex {
        public TPosition position;
        public TNormal normal;
        public TAttributes attributes;

        public Vertex(TPosition position, TNormal normal) {
            this.position = position;
            this.normal   = normal;
        }

        public Vertex(TPosition position, TNormal normal, TAttributes attributes) {
            this.position   = position;
            this.normal     = normal;
            this.attributes = attributes;
        }
    }

    public VertexBuffer(in ReadOnlySpan<Vertex> vertices, params VertexAttribute[] attribute_definitions)
        : base(
               vertices,
               [
                   new VertexAttribute<TPosition>("position", 3, false),
                   new VertexAttribute<TNormal>("normal", 3, false),
                   ..attribute_definitions
               ]
              ) {}

    public VertexBuffer(int count, params VertexAttribute[] attribute_definitions)
        : base(
               count,
               [
                   new VertexAttribute<TPosition>("position", 3, false),
                   new VertexAttribute<TNormal>("normal", 3, false),
                   ..attribute_definitions
               ]
              ) {}

    public ArrayView<TPosition> positions
        => get_view<TPosition>("position");

    public ArrayView<TNormal> normals
        => get_view<TNormal>("normal");

    public ArrayView<TAttributes> attributes
        => get_view<TAttributes>("attributes");
}