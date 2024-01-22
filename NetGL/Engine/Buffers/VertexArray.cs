using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public class VertexArray {
    protected int handle;

    public readonly PrimitiveType primitive_type;

    protected readonly IVertexBuffer vertex_buffer;

    public VertexArray(IVertexBuffer vertex_buffer, PrimitiveType primitive_type = PrimitiveType.Triangles) {
        handle = 0;

        this.vertex_buffer = vertex_buffer;
        this.primitive_type = primitive_type;
    }
    
    public void bind() {
        if (handle == 0)
            throw new NotSupportedException("no handle has been allocated yet!");

        GL.BindVertexArray(handle);
    }

    public void unbind() {        
        if (handle == 0)
            throw new NotSupportedException("no handle has been allocated yet!");

        GL.BindVertexArray(0);
    }

    public virtual void upload() {
        if (handle == 0)
            handle = GL.GenVertexArray();

        GL.BindVertexArray(handle);

        vertex_buffer.bind();

        upload_attribute_pointers();

        GL.BindVertexArray(0);

        vertex_buffer.unbind();

        Console.WriteLine();
    }

    protected void upload_attribute_pointers() {
        var stride = vertex_buffer.item_size;

        Console.WriteLine("Vertex attributes: ");
        Console.WriteLine($"  stride = {stride}");

        foreach (var field in vertex_buffer.get_vertex_spec()) {
            nint offset = Marshal.OffsetOf(vertex_buffer.item_type, field.name);
            Console.WriteLine($"VertexAttribPointer({field.location}, {field.size}, {field.pointer_type}, {field.normalized}, {stride}, {offset})");
            GL.VertexAttribPointer(field.location, field.size, field.pointer_type, field.normalized, stride, offset);
            GL.EnableVertexAttribArray(field.location);
        }

        Error.check();
    }

    public override string ToString() {
        return $"{primitive_type}, vert:{vertex_buffer.count}";
    }

    public virtual void draw() {
        // Console.WriteLine($"VertexArray.draw ({primitive_type}, {0}, {vertex_buffer.count})");
        GL.DrawArrays(primitive_type, 0, vertex_buffer.count);
    }
}