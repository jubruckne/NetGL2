using OpenTK.Mathematics;

namespace NetGL;

public interface IVertexBuffer: IBuffer {
    public IReadOnlyList<VertexAttribute> attributes { get; }
}

public class VertexBuffer<T>: ArrayBuffer<T>, IVertexBuffer where T: struct {
    public IReadOnlyList<VertexAttribute> attributes { get; }

    public VertexBuffer(int count, params VertexAttribute[] attributes) : base(count) {
        this.attributes = attributes;
    }

    public VertexBuffer(in T[] vertices, params VertexAttribute[] attributes) : base(vertices) {
        this.attributes = attributes;
    }

    public VertexBuffer(in T[] vertices): this(vertices, [VertexAttribute.Position]) {}

    public VertexBuffer(in T[] vertices, IReadOnlyList<VertexAttribute> attributes) : base(vertices) {
        this.attributes = attributes;
    }
    public VertexBuffer(IReadOnlyList<T> vertices) : this(vertices, [VertexAttribute.Position]) { }
    public VertexBuffer(IReadOnlyList<T> vertices, VertexAttribute attribute) : this(vertices, [attribute]) { }

    public VertexBuffer(IEnumerable<T> vertices) : this(vertices, [VertexAttribute.Position]) { }

    public VertexBuffer(IEnumerable<T> vertices, IReadOnlyList<VertexAttribute> attributes): base(vertices) {
        this.attributes = attributes;
    }

    public VertexBuffer(IEnumerable<T> vertices, params VertexAttribute[] attributes) : this(vertices.ToArray(),
        attributes) {}


    public override string ToString() {
        return $"{attributes.array_to_string()}";
    }
}