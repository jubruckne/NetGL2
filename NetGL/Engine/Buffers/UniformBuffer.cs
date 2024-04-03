using NetGL.ECS;

namespace NetGL;

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

public interface IUniformBuffer: INamed, IUniform;

public class UniformBuffer<T>: Buffer<T>, IUniformBuffer
    where T: unmanaged {

    private readonly Shader shader;
    private readonly int block_index;

    public string name { get; }
    private readonly int binding_point;

    private T uniform_value;

    public UniformBuffer(in Shader shader, in string name, in T item): base(BufferTarget.UniformBuffer, item) {
        this.shader = shader;
        this.name = name;
        verify_std_140_alignment();

        binding_point = 0;
        block_index = GL.GetUniformBlockIndex(handle, name);
        if (block_index == -1) Error.index_out_of_range(name);

        create(BufferUsageHint.DynamicDraw);
    }

    public UniformBuffer(in Shader shader, in string name): this(shader, name, default) {}

    public T data {
        get => uniform_value;
        set {
            if (uniform_value.is_equal_to(in value)) return;
            uniform_value = value;
            update_and_make_current();
        }
    }

    private static void verify_std_140_alignment() {
        var type   = typeof(T);
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        var offset = 0;
        foreach (var field in fields) {
            var alignment = field.FieldType.std140_alignment();
            var size      = Marshal.SizeOf(field.FieldType);

            offset = align(offset, alignment);

            Console.WriteLine($"Field: {field.Name}, Offset: {offset}, Alignment: {alignment}, Size: {size}");

            offset += size;
        }

        var total_size = Unsafe.SizeOf<T>();
        Console.WriteLine($"Total calculated size: {offset}, Total Marshal.SizeOf: {total_size}");

        if(offset != total_size) Error.type_alignment_mismatch<T>(offset, total_size);

        return;

        static int align(int current_offset, int alignment)
            => current_offset + (alignment - current_offset % alignment) % alignment;
    }

    private void update_and_make_current() {
        base.update();

        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, binding_point, shader.handle);
        GL.UniformBlockBinding(shader.handle, block_index, binding_point);

        Debug.assert_opengl();
    }
}