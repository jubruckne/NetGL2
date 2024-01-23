
using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public class VertexArrayIndexed: VertexArray {
    protected readonly IIndexBuffer index_buffer;

    public VertexArrayIndexed(
        IVertexBuffer vertex_buffer,
        IIndexBuffer index_buffer): base(vertex_buffer, index_buffer.primitive_type) {

        this.index_buffer = index_buffer;
    }

    public override void upload() {
        if (handle == 0)
            handle = GL.GenVertexArray();

        GL.BindVertexArray(handle);

        vertex_buffer.bind();
        index_buffer.bind();

        upload_attribute_pointers();

        GL.BindVertexArray(0);

        vertex_buffer.unbind();
        index_buffer.unbind();

        Console.WriteLine();
    }

    public override string ToString() {
        return $"{primitive_type}, vert:{vertex_buffer.count}, ind:{index_buffer.count}";
    }

    public override void draw() {
        //Console.WriteLine($"IndexedVertexArray.draw ({primitive_type}, {index_buffer.size}, {index_buffer.draw_element_type}, 0)");
        GL.DrawElements(PrimitiveType.Triangles, index_buffer.size, index_buffer.draw_element_type, 0);
    }
}