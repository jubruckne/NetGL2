using OpenTK.Mathematics;

namespace NetGL;

public class Guizmos {
    private VertexBuffer<Vector3> vertices;

    public Guizmos() => clear();

    public void clear() {
        vertices = new VertexBuffer<Vector3>(0, VertexAttribute.Position);
    }

    public void cube(in Vector3 position, float width = 1f, float height = 1f, float depth = 1f) {
        var c = new Cube(width, height, depth);
        vertices.append(c.generate(position).get_vertices().ToArray());
    }
}