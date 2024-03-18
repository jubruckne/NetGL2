using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public class VertexArray: IBindable {
    protected int handle;

    public readonly PrimitiveType primitive_type;

    public readonly IVertexBuffer[] vertex_buffers;
    public IReadOnlyList<VertexAttribute> vertex_attributes { get; }

    public VertexArray(PrimitiveType primitive_type = PrimitiveType.Triangles, params IVertexBuffer[] vertex_buffers) {
        handle = 0;

        this.vertex_buffers = vertex_buffers;
        this.primitive_type = primitive_type;

        int location = 0;
        vertex_attributes = new List<VertexAttribute>();
        foreach (var buffer in this.vertex_buffers) {
            int offset = 0;
            foreach (var attrib in buffer.attributes_definitions) {
                var a = attrib.copy();
                a.location = location;
                a.offset = offset;
                a.buffer = buffer;
                offset += a.size_of;
                location++;

                ((List<VertexAttribute>)vertex_attributes).Add(a);
            }
        }
    }

    public virtual void bind() {
        if (handle == 0)
            throw new NotSupportedException("no handle has been allocated yet!");

        GL.BindVertexArray(handle);
    }

    public virtual void upload() {
        if (handle == 0)
            handle = GL.GenVertexArray();

        GL.BindVertexArray(handle);

        upload_attribute_pointers();

        GL.BindVertexArray(0);
    }

    protected void upload_attribute_pointers() {
        foreach (var attribute in vertex_attributes) {
            attribute.buffer.bind();
/*
            Console.WriteLine("Vertex attributes: ");
            Console.WriteLine($"  stride = {attribute.buffer.item_size}");

            Console.WriteLine(attribute.buffer.item_type);
            Console.WriteLine($"VertexAttribPointer({attribute.location}, {attribute.count}, {attribute.pointer_type}, {attribute.normalized}, {attribute.buffer.item_size}, {attribute.offset})");
           */
            GL.VertexAttribPointer(attribute.location, attribute.count, attribute.pointer_type, attribute.normalized, attribute.buffer.item_size, attribute.offset);
            GL.EnableVertexAttribArray(attribute.location);
        }

        Error.check();
    }

    public override string ToString() {
        return $"vert:{vertex_buffers.sum(buffer => buffer.length):N0}";
    }

    public virtual void draw() {
        foreach (var buffer in vertex_buffers) {
            //buffer.bind();
            GL.DrawArrays(primitive_type, 0, buffer.length);
        }
    }

    public bool has_normals {
        get {
            foreach (var a in vertex_attributes)
                if (a.name == "normal")
                    return true;
            return false;
        }
    }
}