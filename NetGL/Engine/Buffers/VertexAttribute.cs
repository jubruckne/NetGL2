using System.Numerics;
using System.Runtime.CompilerServices;
using NetGL.ECS;
using OpenTK.Graphics.OpenGL4;
using Spectre.Console;

namespace NetGL;

public class VertexAttribute: INamed {
    public IVertexBuffer? source_buffer { get; internal set; }
    public string name { get; }
    public nint offset { get; internal set; }
    public Type type_of { get; internal set; }
    public int size_of { get; internal set; }
    public int location { get; internal set; }
    public int count { get; }
    public VertexAttribPointerType pointer_type { get; }
    public bool normalized { get; }
    public int divisor { get; }
    public bool is_instanced => divisor > 0;

    private VertexAttribute(string name,
                            Type type_of,
                            int size_of,
                            int count,
                            VertexAttribPointerType pointer_type,
                            bool normalized = false,
                            int divisor = 0
    ) {
        this.name         = name;
        this.type_of      = type_of;
        this.size_of      = size_of;
        this.offset       = -1;
        this.location     = -1;
        this.count        = count;
        this.pointer_type = pointer_type;
        this.normalized   = normalized;
        this.divisor      = divisor;
    }

    private VertexAttribute(string name,
                            (Type type, int size_of, VertexAttribPointerType pointer_type, int count) type,
                            bool normalized = false,
                            int divisor = 0): this(name, type.type, type.size_of, type.count, type.pointer_type, normalized, divisor) {}

    public string glsl_type {
        get {
            return pointer_type switch {
                VertexAttribPointerType.Float => $"vec{count}",
                VertexAttribPointerType.HalfFloat => $"vec{count}",
                _ => throw new NotImplementedException($"glsl_type for {name}, {pointer_type}, {count}!")
            };
        }
    }

    public VertexAttribute copy()
        => new VertexAttribute(name, type_of, size_of, count, pointer_type, normalized, divisor) {
                   offset   = offset,
                   location = location
               };

    public override string ToString() => $"{glsl_type}: {name}]";

    public static VertexAttribute vec3<T>(string name, bool normalized = false, int divisor = 0)
        where T: unmanaged
        => new VertexAttribute(name, new T().to_vertex_attribute_pointer_type(), normalized, divisor);

    public static VertexAttribute vec4<T>(string name, bool normalized = false, int divisor = 0)
        where T: unmanaged
        => new VertexAttribute(name, new T().to_vertex_attribute_pointer_type(), normalized, divisor);

    public static VertexAttribute vec2<T>(string name, bool normalized = false, int divisor = 0)
        where T: unmanaged
        => new VertexAttribute(name, new T().to_vertex_attribute_pointer_type(), normalized, divisor);

    public static VertexAttribute scalar<T>(string name, bool normalized = false, int divisor = 0)
        where T: unmanaged
        => new VertexAttribute(name, new T().to_vertex_attribute_pointer_type(), normalized, divisor);

    public static VertexAttribute float2(string name, bool normalized = false, int divisor = 0)
        => vec2<float>(name, normalized, divisor);

    public static VertexAttribute float3(string name, bool normalized = false, int divisor = 0)
        => vec3<float>(name, normalized, divisor);

    public static VertexAttribute float4(string name, bool normalized = false, int divisor = 0)
        => vec4<float>(name, normalized, divisor);

    public static VertexAttribute short2(string name, bool normalized = false, int divisor = 0)
        => vec2<short>(name, normalized, divisor);

    public static VertexAttribute short3(string name, bool normalized = false, int divisor = 0)
        => vec3<short>(name, normalized, divisor);

    public static VertexAttribute buffer<T>(string name, VertexAttribPointerType type, int count, bool normalized = false, int divisor = 0)
        => new VertexAttribute(name, typeof(T), Unsafe.SizeOf<T>(), count, type, normalized, divisor);
}