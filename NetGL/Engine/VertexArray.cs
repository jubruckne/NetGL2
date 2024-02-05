using System.Runtime.InteropServices;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace NetGL.dev;

public abstract class VertexAttribute {
    public Type underlying_type { get; }
    public int size_of { get; }
    public string name { get; }

    public VertexAttribPointerType pointer_type { get; }
    public bool normalized { get; }
    public int opengl_size { get; }

    public static VertexAttribute<Vector3> Position =>
        new(name: "position", pointer_type: VertexAttribPointerType.Float);

    public static VertexAttribute<Vector3> Normal =>
        new(name: "normal", pointer_type: VertexAttribPointerType.Float);

    public static VertexAttribute<Color4> Color4f =>
        new(name: "color", pointer_type: VertexAttribPointerType.Float);

    protected VertexAttribute(Type underlying_type, int size_of, string name, VertexAttribPointerType pointer_type, bool normalized = false) {
        this.underlying_type = underlying_type;
        this.size_of = size_of;
        this.name = name;
        this.pointer_type = pointer_type;
        this.normalized = normalized;

        opengl_size = pointer_type switch {
            VertexAttribPointerType.Double => size_of / 8,
            VertexAttribPointerType.Float => size_of / 4,
            VertexAttribPointerType.Int => size_of / 4,
            VertexAttribPointerType.UnsignedInt => size_of / 4,
            VertexAttribPointerType.Short => size_of / 2,
            VertexAttribPointerType.UnsignedShort => size_of / 2,
            VertexAttribPointerType.HalfFloat => size_of / 2,
            VertexAttribPointerType.Byte => size_of,
            VertexAttribPointerType.UnsignedByte => size_of,
            _ => throw new ArgumentOutOfRangeException(nameof(pointer_type), pointer_type, null)
        };
    }

    public override string ToString() {
        return $"{name}: {nameof(underlying_type)}: {underlying_type.Name}, {nameof(size_of)}: {size_of}, {nameof(pointer_type)}: {pointer_type}, {nameof(normalized)}: {normalized}, {nameof(opengl_size)}: {opengl_size}";
    }
}

public class VertexAttribute<T>: VertexAttribute {
    public VertexAttribute(string name, VertexAttribPointerType pointer_type, bool normalized = false)
    : base(typeof(T), Marshal.SizeOf<T>(), name, pointer_type, normalized) { }
}

public abstract class VertexDescriptor {
    [StructLayout(LayoutKind.Sequential)]
    public struct Data<A> where A: unmanaged {
        private A a;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Data<A, B> where A: unmanaged where B: unmanaged {
        private A a;
        private B b;
    }

    public static VertexDescriptor<Data<A1>> make<A1> (VertexAttribute<A1> a1)
        where A1: unmanaged {
        return new VertexDescriptor<Data<A1>>(a1);
    }

    public static VertexDescriptor<Data<A1, A2>> make<A1, A2> (VertexAttribute<A1> a1, VertexAttribute<A2> a2)
        where A1: unmanaged
        where A2: unmanaged {
        return new VertexDescriptor<Data<A1, A2>>(a1, a2);
    }
}

public class VertexDescriptor<T> where T: unmanaged {
    public IEnumerable<VertexAttribute> attributes => attribute_list.Values;
    public int size_of { get; }

    public T[] buffer;

    private readonly Dictionary<string, VertexAttribute> attribute_list;

    public VertexDescriptor(params VertexAttribute[] attributes) {
        attribute_list = new();
        size_of = 0;
        foreach (var attrib in attributes) {
            attribute_list.Add(attrib.name, attrib);
            size_of += attrib.size_of;
        }
    }

    public void allocate(int count) {
        buffer = new T[count];
    }

    public override string ToString() {
        StringBuilder sb = new();
        sb.AppendLine($"VertexDescriptor(size_of: {size_of}) [");
        foreach(var attrib in attributes)
            sb.AppendLine($"  {attrib}");
        sb.AppendLine($"[");
        return sb.ToString();
    }
}