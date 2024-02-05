
using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public class VertexArrayIndexed: VertexArray {
    protected readonly IIndexBuffer index_buffer;

    public VertexArrayIndexed(IIndexBuffer index_buffer, params IVertexBuffer[] vertex_buffers
        ): base(index_buffer.primitive_type, vertex_buffers) {

        this.index_buffer = index_buffer;
    }

    public override void upload() {
        if (handle == 0)
            handle = GL.GenVertexArray();

        GL.BindVertexArray(handle);

        index_buffer.bind();

        upload_attribute_pointers();

        GL.BindVertexArray(0);

        index_buffer.unbind();

        //Console.WriteLine();
    }

    public override string ToString() {
        return $"vert:{vertex_buffers.sum(buffer => buffer.count):N0}, ind:{index_buffer.count:N0}";
    }

    public override void draw() {
        //Console.WriteLine($"IndexedVertexArray.draw ({primitive_type}, {index_buffer.size}, {index_buffer.draw_element_type}, 0)");
        GL.DrawElements(PrimitiveType.Triangles, index_buffer.size, index_buffer.draw_element_type, 0);
    }
}