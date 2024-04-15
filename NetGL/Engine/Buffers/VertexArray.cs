namespace NetGL;

using OpenTK.Graphics.OpenGL4;

public class VertexArray: IBindable {
    public enum Type {Points = PrimitiveType.Points, LineStrip = PrimitiveType.LineStrip, Triangles = PrimitiveType.Triangles, Patches = PrimitiveType.Patches}

    public int handle { get; protected set; }

    public readonly PrimitiveType primitive_type;
    public readonly Material? material;
    public readonly Materials.Material? material2;

    public readonly List<IVertexBuffer> vertex_buffers;
    public IReadOnlyList<VertexAttribute> vertex_attributes { get; }

    public VertexArray(IVertexBuffer vertex_buffer, Union<Material, Materials.Material> material): this([vertex_buffer], material) {}

    public VertexArray(List<IVertexBuffer> vertex_buffers, Union<Material, Materials.Material> material): this(
         Type.Triangles,
         vertex_buffers,
         material
        ) {}

    public VertexArray(Type type, IVertexBuffer vertex_buffer, Union<Material, Materials.Material> material):
        this(type, [vertex_buffer], material) {}

    public VertexArray(Type type, List<IVertexBuffer> vertex_buffers, Union<Material, Materials.Material> material) {
        handle = 0;

        this.vertex_buffers = vertex_buffers;
        this.primitive_type = (PrimitiveType)type;

        this.material = material;
        this.material2 = material;

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

    public void bind() {
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
            attribute.buffer!.bind();

            if(primitive_type == PrimitiveType.Patches) {
                Console.WriteLine("\nVertex attributes: ");
                Console.WriteLine($"  {attribute.buffer.item_type.get_type_name()}: stride = {attribute.buffer.item_size}");
                Console.WriteLine(
                                  $"VertexAttribPointer({attribute.location}, {attribute.count}, {attribute.pointer_type}, {attribute.normalized}, {attribute.buffer.item_size}, {attribute.offset})"
                                 );
                Console.WriteLine();
            }
            GL.VertexAttribPointer(attribute.location, attribute.count, attribute.pointer_type, attribute.normalized, attribute.buffer.item_size, attribute.offset);
            GL.EnableVertexAttribArray(attribute.location);
            if(attribute.is_instanced)
                GL.VertexAttribDivisor(attribute.location, attribute.divisor);

        }

        if(primitive_type == PrimitiveType.Patches)
            GL.PatchParameter(PatchParameterInt.PatchVertices, 4);

        Debug.assert_opengl();
    }

    public override string ToString() {
        return $"vert:{vertex_buffers.sum(static buffer => buffer.length):N0}";
    }

    public virtual void draw() {
        if (handle == 0)
            throw new NotSupportedException("no handle has been allocated yet!");

        GL.GetInteger(GetPName.VertexArrayBinding, out int vertex_array);
        Debug.assert_equal(handle, vertex_array);

        Console.WriteLine($"VertexArray.draw ({primitive_type}, {vertex_buffers[0].length:N0})");
        switch (primitive_type) {
            case PrimitiveType.Patches:
                vertex_buffers[0].bind();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                Debug.assert_opengl();

                Debug.assert_equal(vertex_buffers.Count, 1);

                Console.WriteLine(this.vertex_attributes.array_to_string());
                Console.WriteLine(vertex_buffers[0]);

                GL.DrawArrays(primitive_type, 0, 4);

                Debug.assert_opengl();
                return;
            case PrimitiveType.Triangles or PrimitiveType.Points or PrimitiveType.LineStrip: {
                GL.DrawArrays(primitive_type, 0, vertex_buffers[0].length);
                return;
            }
        }

        throw new NotSupportedException();
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