using OpenTK.Mathematics;

namespace NetGL;

public class Cube: IShape<Cube> {
    public readonly float width;
    public readonly float height;
    public readonly float depth;

    public Cube(float width, float height, float depth) {
        this.width = width;
        this.height = height;
        this.depth = depth;
    }

    public Vector3 size => new Vector3(width, height, depth);

    public Cube(float radius): this(radius * 2f, radius * 2f, radius * 2f) { }
    public Cube(): this(1f, 1f, 1f) { }

    public IShapeGenerator generate() => new CubeShapeGenerator(this);
    public IShapeGenerator generate(Vector3 position) => new CubeShapeGenerator(this, position);

    public override string ToString() {
        return $"Cube[width:{width}, height:{height}, depth:{depth}]";
    }
}

file class CubeShapeGenerator: IShapeGenerator {
    private readonly Vector3 position;
    private readonly Size size;

    public CubeShapeGenerator(in Cube cube) : this(cube, Vector3.Zero) {}

    public CubeShapeGenerator(in Cube cube, Vector3 position) {
        this.size = (cube.width, cube.height, cube.depth);
        this.position = position;
    }

    public override string ToString() => "Cube";

    public IEnumerable<Vector3> get_vertices() {
        var half_width = size.width * 0.5f;
        var half_height = size.height * 0.5f;
        var half_depth = size.depth * 0.5f;

        return [
            new Vector3(position.X - half_width, position.Y - half_height, position.Z + half_depth),    // 0
            new Vector3(position.X + half_width, position.Y - half_height, position.Z + half_depth),     // 1
            new Vector3(position.X + half_width, position.Y + half_height, position.Z + half_depth),      // 2
            new Vector3(position.X - half_width, position.Y + half_height, position.Z + half_depth),     // 3
            new Vector3(position.X - half_width, position.Y - half_height, position.Z - half_depth),   // 4
            new Vector3(position.X + half_width, position.Y - half_height, position.Z - half_depth),    // 5
            new Vector3(position.X + half_width, position.Y + half_height, position.Z - half_depth),     // 6
            new Vector3(position.X - half_width, position.Y + half_height, position.Z - half_depth)     // 7
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
}