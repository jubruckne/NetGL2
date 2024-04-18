using System.Threading.Channels;

namespace NetGL;

using OpenTK.Graphics.OpenGL4;

public class VertexArray: IBindable {
    public enum Type {Points = PrimitiveType.Points, LineStrip = PrimitiveType.LineStrip, Triangles = PrimitiveType.Triangles, Patches = PrimitiveType.Patches}

    public int handle { get; protected set; }

    public readonly PrimitiveType primitive_type;
    public IVertexBuffer? instance_buffer { get; }
    public bool has_instance_buffer => instance_buffer != null;

    public readonly Material? material;
    public readonly Materials.Material? material2;

    public readonly List<IVertexBuffer> vertex_buffers;
    public IReadOnlyList<VertexAttribute> vertex_attributes { get; }

    public VertexArray(IVertexBuffer vertex_buffer, Union<Material, Materials.Material>? material = default): this([vertex_buffer], material) {}

    public VertexArray(List<IVertexBuffer> vertex_buffers, Union<Material, Materials.Material>? material = default): this(
         Type.Triangles,
         vertex_buffers,
         material
        ) {}

    public VertexArray(Type type, IVertexBuffer vertex_buffer, Union<Material, Materials.Material>? material = default):
        this(type, [vertex_buffer], material) {}

    public VertexArray(Type type, List<IVertexBuffer> vertex_buffers, Union<Material, Materials.Material>? material = default) {
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
                if(a.is_instanced)
                    instance_buffer = a.buffer;

                ((List<VertexAttribute>)vertex_attributes).Add(a);
            }
        }
    }

    public void bind() {
        if (handle == 0)
            throw new NotSupportedException("no handle has been allocated yet!");

        GL.BindVertexArray(handle);
    }

    public static void unbind() =>
        GL.BindVertexArray(0);

    public void create() => create(BufferUsageHint.StaticDraw);

    public virtual void create(BufferUsageHint usage) {
        if (handle == 0)
            handle = GL.GenVertexArray();

        GL.BindVertexArray(handle);

        create_attribute_pointers();

        GL.BindVertexArray(0);
    }

    protected void create_attribute_pointers() {
        Debug.assert_not_equal(vertex_attributes.Count, 0);
        if(primitive_type == PrimitiveType.Patches)
            GL.PatchParameter(PatchParameterInt.PatchVertices, 4);

        foreach (var attribute in vertex_attributes) {
            attribute.buffer!.bind();
            GL.EnableVertexAttribArray(attribute.location);

            if(primitive_type == PrimitiveType.Patches) {
                Console.WriteLine("\nVertex attributes: ");
                Console.WriteLine($"  {attribute.buffer.item_type.get_type_name()}: stride = {attribute.buffer.item_size}");
                Console.WriteLine(
                                  $"VertexAttribPointer({attribute.location}, {attribute.count}, {attribute.pointer_type}, {attribute.normalized}, {attribute.buffer.item_size}, {attribute.offset})"
                                 );
                Console.WriteLine();
            }
            GL.VertexAttribPointer(attribute.location, attribute.count, attribute.pointer_type, attribute.normalized, attribute.buffer.item_size, attribute.offset);

            if (attribute.is_instanced) {
                GL.VertexAttribDivisor(attribute.location, attribute.divisor);
            }
        }

        Debug.assert_opengl();
    }

    public override string ToString() {
        return $"vert:{vertex_buffers.sum(static buffer => buffer.length):N0}";
    }

    public virtual void draw() {
        if (handle == 0)
            throw new NotSupportedException("no handle has been allocated yet!");

        Console.WriteLine($"VertexArray.draw ({primitive_type}, {vertex_buffers[0].length:N0})");
        switch (primitive_type) {
            case PrimitiveType.Patches:
                this.bind();
                vertex_buffers[0].update();
                vertex_buffers[0].bind();
                Debug.assert_opengl();

                Debug.assert_equal(vertex_buffers.Count, 1);

                Console.WriteLine(this.vertex_attributes.array_to_string());
                Console.WriteLine(vertex_buffers[0]);
                Debug.assert_opengl();
                Console.WriteLine("jaaaaahhhh");
                Console.WriteLine(primitive_type);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

                GL.DrawArrays(primitive_type, 0, vertex_buffers[0].length);

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