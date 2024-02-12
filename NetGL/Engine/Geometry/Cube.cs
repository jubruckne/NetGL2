using OpenTK.Mathematics;

namespace NetGL;

public class Cube: IShape<Cube> {
    public float width;
    public float height;
    public float depth;

    public static Cube make(float width, float height, float depth) => new Cube(width, height, depth);
    public static Cube make(float radius) => make(radius * 2f, radius * 2f, radius * 2f);

    private Cube(float width, float height, float depth) {
        this.width = width;
        this.height = height;
        this.depth = depth;
    }

    public IEnumerable<Vector3> get_vertices() {
        var half_width = width * 0.5f;
        var half_height = height * 0.5f;
        var half_depth = depth * 0.5f;

        return [
            new Vector3(-half_width, -half_height, half_depth),    // 0
            new Vector3(half_width, -half_height, half_depth),     // 1
            new Vector3(half_width, half_height, half_depth),      // 2
            new Vector3(-half_width, half_height, half_depth),     // 3
            new Vector3(-half_width, -half_height, -half_depth),   // 4
            new Vector3(half_width, -half_height, -half_depth),    // 5
            new Vector3(half_width, half_height, -half_depth),     // 6
            new Vector3(-half_width, half_height, -half_depth)     // 7
        ];
/*
        return [
            new Vector3(-1.0f, 1.0f, -1.0f),
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, -1.0f),
            new Vector3(-1.0f, 1.0f, -1.0f),
            new Vector3(-1.0f, -1.0f, 1.0f),
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(-1.0f, 1.0f, -1.0f),
            new Vector3(-1.0f, 1.0f, -1.0f),
            new Vector3(-1.0f, 1.0f, 1.0f),
            new Vector3(-1.0f, -1.0f, 1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, 1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(1.0f, 1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(-1.0f, -1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, 1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(1.0f, -1.0f, 1.0f),
            new Vector3(-1.0f, -1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, -1.0f),
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(-1.0f, -1.0f, 1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(-1.0f, -1.0f, 1.0f),
            new Vector3(1.0f, -1.0f, 1.0f)
        ];
        */
    }

    public IEnumerable<Vector3i> get_indices() {
        return [
            // front face
            (0, 1, 2), (2, 3, 0),
            // top face
            (3, 2, 6), (6, 7, 3),
            // back face
            (7, 6, 5), (5, 4, 7),
            // left face
            (4, 0, 3), (3, 7, 4),
            // bottom face
            (1, 4, 5), (1, 0, 4),
            // right face
            (1, 5, 6), (6, 2, 1)
        ];
    }

    public override string ToString() {
        return $"Cube[width:{width}, height:{height}, depth:{depth}]";
    }
}