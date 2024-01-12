using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace NetGL;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
public class VertexAttributeAttribute : Attribute {
    public string Name {get; }
    public int Location { get; }
    public int Size {get; }
    public VertexAttribPointerType Type { get; }
    public bool Normalized { get; }

    public VertexAttributeAttribute(string name, int location, int size, VertexAttribPointerType type, bool normalized = false) {
        Name = name;
        Size = size;
        Location = location;
        Type = type;
        Normalized = normalized;
    }

    public override string ToString() => $"name={Name} location={Location} size={Size} type={Type} normalized={Normalized}";
}

public struct Vertex {
    [VertexAttribute("a_position", location:0, size:3, VertexAttribPointerType.Float)]
    public Vector3 position;

    [VertexAttribute("a_color", location:1, size:4, VertexAttribPointerType.UnsignedByte, normalized:true)] 
    public uint color;

    [VertexAttribute("a_texcoord", location:2, size:3, VertexAttribPointerType.Float)] 
    public Vector3 texcoord;

    public Vertex(Vector3 position, uint color, Vector3 texcoord) {
        this.position = position;
        this.color = color;
        this.texcoord = texcoord;
    }
}

public class ArrayBuffer<T>: Buffer<T> where T: struct {
    public ArrayBuffer() : base(BufferTarget.ArrayBuffer) { }

    public ArrayBuffer(int count) : base(BufferTarget.ArrayBuffer, count) { }

    public ArrayBuffer(in T[] items) : base(BufferTarget.ArrayBuffer, items.Length) {
        insert(0, items);
    }
}