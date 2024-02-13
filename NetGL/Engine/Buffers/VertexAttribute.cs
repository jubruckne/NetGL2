using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace NetGL;

public abstract class VertexAttribute {
    public IVertexBuffer buffer { get; internal set; }
    public abstract Type type_of { get; }
    public abstract int size_of { get; }
    public string name {get; }
    public int offset { get; internal set;  }
    public int location { get; internal set; }
    public int count { get; }
    public VertexAttribPointerType pointer_type { get; }
    public bool normalized { get; }

    public static VertexAttribute<Vector3> Position =>
        new (name:"position", count: 3, pointer_type: VertexAttribPointerType.Float);
    public static VertexAttribute<Vector3> Normal =>
        new (name:"normal", count: 3, pointer_type: VertexAttribPointerType.Float);

#pragma warning disable CS8618
    protected VertexAttribute(string name, int count, VertexAttribPointerType pointer_type, bool normalized = false) {
#pragma warning restore CS8618
        this.name = name;
        this.offset = -1;
        this.location = -1;
        this.count = count;
        this.pointer_type = pointer_type;
        this.normalized = normalized;
    }

    public string glsl_type {
        get {
            return pointer_type switch {
                VertexAttribPointerType.Float => $"vec{count}",
                _ => throw new NotImplementedException($"glsl_type for {name}, {pointer_type}, {count}!")
            };
        }
    }

    public abstract VertexAttribute copy();

    public override string ToString() => $"{name}: {type_of.Name} => {pointer_type}[{count}]";
}

public class VertexAttribute<T>: VertexAttribute {
    public override Type type_of => typeof(T);
    public override int size_of => Marshal.SizeOf<T>();

    public override VertexAttribute copy() {
        var a = new VertexAttribute<T>(name, count, pointer_type, normalized) {
            offset = offset,
            location = location
        };
        return a;
    }

    internal VertexAttribute(string name, int count, VertexAttribPointerType pointer_type, bool normalized = false) :
        base(name, count, pointer_type, normalized) {
    }
}