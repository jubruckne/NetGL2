using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace NetGL;

[StructLayout(LayoutKind.Sequential)]
public struct Vertex {
    public Vector3 position;

    public Vertex(Vector3 position) {
        this.position = position;
    }
}