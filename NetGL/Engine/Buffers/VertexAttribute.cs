using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public class VertexAttribute {
    public string name {get; }
    public int location { get; }
    public int size { get; }
    public VertexAttribPointerType pointer_type { get; }
    public bool normalized { get; }

    public string glsl_type {
        get {
            switch (pointer_type) {
                case VertexAttribPointerType.Float:
                    return $"vec{size}";
            }

            throw new NotImplementedException($"glsl_type for {name}, {pointer_type}, {size}!");
        }
    }

    public VertexAttribute(string name, int location, int size, VertexAttribPointerType pointer_type, bool normalized = false) {
        this.name = name;
        this.size = size;
        this.location = location;
        this.pointer_type = pointer_type;
        this.normalized = normalized;
    }

    public override string ToString() => $"name={name} location={location} size={size} type={pointer_type} normalized={normalized}";
}