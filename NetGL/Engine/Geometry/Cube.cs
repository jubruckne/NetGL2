using OpenTK.Mathematics;

namespace NetGL;

public class Cube: IShape<CubeShapeGenerator.Options> {
    public readonly float width;
    public readonly float height;
    public readonly float depth;

    public Cube(float width, float height, float depth) {
        this.width = width;
        this.height = height;
        this.depth = depth;
    }

    public Size3<float> size => new(width, height, depth);

    public Cube(float radius): this(radius * 2f, radius * 2f, radius * 2f) { }
    public Cube(): this(1f, 1f, 1f) { }

    public IShapeGenerator generate() => new CubeShapeGenerator(this);
    public IShapeGenerator generate(CubeShapeGenerator.Options options) => new CubeShapeGenerator(this, options);

    public override string ToString() => $"Cube[width:{width}, height:{height}, depth:{depth}]";
}

public class CubeShapeGenerator: IShapeGenerator {
    public record struct Options(Vector3 position);

    private readonly Cube cube;
    private readonly Options options;

    internal CubeShapeGenerator(in Cube cube, in Options options = new()) {
        this.cube = cube;
        this.options = options;
    }

    public override string ToString() => "Cube";

    public ReadOnlySpan<Vector3> get_vertices() {
        var half_width = cube.size.width * 0.5f;
        var half_height = cube.size.height * 0.5f;
        var half_depth = cube.size.depth * 0.5f;

        return new[] {
            new Vector3(options.position.X - half_width, options.position.Y - half_height,
                options.position.Z + half_depth), // 0
            new Vector3(options.position.X + half_width, options.position.Y - half_height,
                options.position.Z + half_depth), // 1
            new Vector3(options.position.X + half_width, options.position.Y + half_height,
                options.position.Z + half_depth), // 2
            new Vector3(options.position.X - half_width, options.position.Y + half_height,
                options.position.Z + half_depth), // 3
            new Vector3(options.position.X - half_width, options.position.Y - half_height,
                options.position.Z - half_depth), // 4
            new Vector3(options.position.X + half_width, options.position.Y - half_height,
                options.position.Z - half_depth), // 5
            new Vector3(options.position.X + half_width, options.position.Y + half_height,
                options.position.Z - half_depth), // 6
            new Vector3(options.position.X - half_width, options.position.Y + half_height,
                options.position.Z - half_depth) // 7
        };
    }

    public ReadOnlySpan<Vector3i> get_indices() {
        return new Vector3i[] {
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
        };
    }
}