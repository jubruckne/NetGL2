using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public class VertexArrayIndexed: VertexArray {
    public readonly IIndexBuffer index_buffer;

    public VertexArrayIndexed(IIndexBuffer index_buffer, IVertexBuffer vertex_buffer): base(index_buffer.primitive_type, [vertex_buffer]) {
        if (vertex_buffer.length > index_buffer.max_vertex_count)
            throw new ArgumentOutOfRangeException(nameof(index_buffer), $"IndexBuffer<{index_buffer.item_type.Name}> too small for VertexBuffer(count={vertex_buffer.length})!");

        this.index_buffer = index_buffer;
    }

    public VertexArrayIndexed(IIndexBuffer index_buffer, params IVertexBuffer[] vertex_buffers)
        : base(index_buffer.primitive_type, vertex_buffers) {

        foreach (var vb in vertex_buffers) {
            if (vb.length > index_buffer.max_vertex_count)
                throw new ArgumentOutOfRangeException(nameof(index_buffer), $"IndexBuffer<{index_buffer.item_type.Name}> too small for VertexBuffer(count={vb.length})!");
        }

        this.index_buffer = index_buffer;
    }

    public override void upload() {
        if (handle == 0)
            handle = GL.GenVertexArray();

        GL.BindVertexArray(handle);

        index_buffer.bind();

        upload_attribute_pointers();

        GL.BindVertexArray(0);

        Error.assert_opengl();

        //Console.WriteLine();
    }

    public override string ToString() {
        return $"vert:{vertex_buffers.sum(buffer => buffer.length):N0}, ind:{index_buffer.length:N0}";
    }

    public override void draw() {
        //Console.WriteLine($"IndexedVertexArray.draw ({primitive_type}, {index_buffer.length * 3}, {index_buffer.draw_element_type}, 0)");
        if (index_buffer.length > 256 * 256) {
            Console.WriteLine(index_buffer.length);
        }

        GL.DrawElements(PrimitiveType.Triangles, index_buffer.length * 3, index_buffer.draw_element_type, 0);
        Error.assert_opengl();
    }
}