using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public interface IVertexBuffer: IBuffer {
    ReadOnlyMap<string, VertexAttribute> attribute_definitions { get; }
}

public class VertexBuffer<T>: Buffer<T>, IVertexBuffer where T: unmanaged {
    public ReadOnlyMap<string, VertexAttribute> attribute_definitions { get; } = new Map<string, VertexAttribute>();

    public VertexBuffer(in ReadOnlySpan<T> vertices, params VertexAttribute[] attributes): base(BufferTarget.ArrayBuffer, vertices) {
        attribute_def_init(attributes);
    }

    public VertexBuffer(int count, params VertexAttribute[] attributes): base(BufferTarget.ArrayBuffer, count) {
        attribute_def_init(attributes);
    }

    private void attribute_def_init(in VertexAttribute[] attributes) {
        var offset = 0;
        foreach (var attrib in attributes) {
            attrib.offset = offset;
            offset += attrib.size_of;
            ((Map<string, VertexAttribute>)attribute_definitions).add(attrib.name, attrib);
        }

        Error.assert((offset, Unsafe.SizeOf<T>()), offset == Unsafe.SizeOf<T>());
    }

    public ArrayView<V> get_attributes<V>(in VertexAttribute att) where V: unmanaged => buffer.get_view<V>(att.name);
    public ArrayWriter<T> get_writer() => new ArrayWriter<T>(get_view());
    public ArrayWriter<V> get_writer<V>(string attribute_name) where V : unmanaged => new ArrayWriter<V>(get_view<V>(attribute_name));
    public ArrayView<T> get_view() => buffer.get_view<T>();
    public ArrayView<V> get_view<V>(string attribute_name) where V : unmanaged {
        return buffer.get_view<V>(attribute_definitions[attribute_name].offset);
    }
}