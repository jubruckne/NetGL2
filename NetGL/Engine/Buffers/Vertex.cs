using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace NetGL;

[StructLayout(LayoutKind.Sequential)]
public struct Vertex: IVertexSpec {
    public Vector3 position;

    public Vertex(Vector3 position) {
        this.position = position;
    }

    public static IList<VertexAttribute> get_vertex_spec() {
        return [
            new("position", location:0, size:3, pointer_type:VertexAttribPointerType.Float)
        ];
    }

    public void set(in Vector3 position) {
        this.position = position;
    }
}