using OpenTK.Mathematics;

namespace NetGL;

public class Cube: IShape<Cube> {
    public float width;
    public float height;
    public float depth;

    public Cube(float radius): this(radius, radius, radius) { }

    public Cube(float width, float height, float depth) {
        this.width = width;
        this.height = height;
        this.depth = depth;
    }

    public static (Vector3[] vertices, ushort[] triangles) create_geometry(Cube cube) {
        var halfWidth = cube.width * 0.5f;
        var halfHeight = cube.height * 0.5f;
        var halfDepth = cube.depth * 0.5f;

        return ([
            (-halfWidth, -halfHeight, -halfDepth), // Bottom front left
            (halfWidth, -halfHeight, -halfDepth), // Bottom front right
            (-halfWidth, halfHeight, -halfDepth), // Top front left
            (halfWidth, halfHeight, -halfDepth), // Top front right
            (-halfWidth, -halfHeight, halfDepth), // Bottom back left
            (halfWidth, -halfHeight, halfDepth), // Bottom back right
            (-halfWidth, halfHeight, halfDepth), // Top back left
            (halfWidth, halfHeight, halfDepth) // Top back right
        ], [
            0, 1, 2,
            1, 3, 2,
            4, 5, 6,
            5, 7, 6,
            0, 2, 6,
            0, 6, 4,
            1, 3, 7,
            1, 7, 5,
            0, 1, 5,
            0, 5, 4,
            2, 3, 7,
            2, 7, 6
        ]);
    }
}