using System.Reflection;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public class VertexArray {
    private int handle;
    
    public VertexArray() {
        handle = 0;
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

    public void upload(Buffer vertex_buffer) {
        if(handle == 0)
            GL.GenVertexArray();

        GL.BindVertexArray(handle);

        vertex_buffer.bind();

        int stride = vertex_buffer.item_size;

        Console.WriteLine("Vertex attributes: ");
        Console.WriteLine("  stride = " + stride);

        var vertex_type = vertex_buffer.GetType().GenericTypeArguments[0];
        Error.check();
        
        foreach (var field in vertex_type.GetFields()) {
            if(!field.IsStatic) {
                var attrib = field.GetCustomAttribute<VertexAttributeAttribute>();
                if (attrib != null) {
                    nint offset = Marshal.OffsetOf(vertex_type, field.Name);
                    Console.WriteLine(field.Name + ": " + attrib + ", offset: " + offset);
                    GL.VertexAttribPointer(attrib.Location, attrib.Size, attrib.Type, attrib.Normalized, stride, offset);
                    GL.EnableVertexAttribArray(attrib.Location);

                    Error.check();
                } else {
                    Console.WriteLine($"skipping field {field}");
                }
            }
        }

        GL.BindVertexArray(0);

        vertex_buffer.unbind();

        Console.WriteLine();
    }

    public void upload(Buffer vertex_buffer, Buffer index_buffer) { 
        if(handle == 0)
            handle = GL.GenVertexArray();

        GL.BindVertexArray(handle);

        vertex_buffer.bind();
        index_buffer.bind();

        int stride = vertex_buffer.item_size;

        Console.WriteLine("Vertex attributes: ");
        Console.WriteLine("  stride = " + stride);

        var vertex_type = vertex_buffer.GetType().GenericTypeArguments[0];
        Error.check();
        
        foreach (var field in vertex_type.GetFields()) {
            if(!field.IsStatic) {
                var attrib = field.GetCustomAttribute<VertexAttributeAttribute>();
                if (attrib != null) {
                    nint offset = Marshal.OffsetOf(vertex_type, field.Name);
                    Console.WriteLine(field.Name + ": " + attrib + ", offset: " + offset);
                    GL.VertexAttribPointer(attrib.Location, attrib.Size, attrib.Type, attrib.Normalized, stride, offset);
                    GL.EnableVertexAttribArray(attrib.Location);

                    Error.check();
                } else {
                    Console.WriteLine($"skipping field {field}");
                }
            }
        }

        GL.BindVertexArray(0);

        vertex_buffer.unbind();
        index_buffer.unbind();

        Console.WriteLine();
    }
}