namespace NetGL;

using OpenTK.Graphics.OpenGL4;

public class VertexArray: IBindable {
    public int handle { get; protected set; }

    public readonly PrimitiveType primitive_type;
    public Material material;

    public readonly IVertexBuffer[] vertex_buffers;
    public IReadOnlyList<VertexAttribute> vertex_attributes { get; }

    public VertexArray(IVertexBuffer[] vertex_buffers, Material material) {
        handle = 0;

        this.vertex_buffers = vertex_buffers;
        this.primitive_type = PrimitiveType.Triangles;
        this.material = material;

        int location = 0;
        vertex_attributes = new List<VertexAttribute>();
        foreach (var buffer in this.vertex_buffers) {
            foreach (var attrib in buffer.attribute_definitions) {
                var a = attrib.copy();
                Debug.assert(a.offset >= 0);
                a.location = location;
                a.buffer = buffer;
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
            Debug.assert_not_null(attribute.buffer);
            attribute.buffer!.bind();
/*
            Console.WriteLine("Vertex attributes: ");
            Console.WriteLine($"  stride = {attribute.buffer.item_size}");

            Console.WriteLine(attribute.buffer.item_type);
            Console.WriteLine($"VertexAttribPointer({attribute.location}, {attribute.count}, {attribute.pointer_type}, {attribute.normalized}, {attribute.buffer.item_size}, {attribute.offset})");
           */
            GL.VertexAttribPointer(attribute.location, attribute.count, attribute.pointer_type, attribute.normalized, attribute.buffer.item_size, attribute.offset);
            GL.EnableVertexAttribArray(attribute.location);
            if(attribute.is_instanced)
                GL.VertexAttribDivisor(attribute.location, attribute.divisor);

        }

        Debug.assert_opengl();
    }

    public override string ToString() {
        return $"vert:{vertex_buffers.sum(static buffer => buffer.length):N0}";
    }

    public virtual void draw() {
        foreach (var buffer in vertex_buffers) {
            GL.DrawArrays(primitive_type, 0, buffer.length);
        }
    }

    public virtual void draw_patches() {
        throw new NotImplementedException();
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