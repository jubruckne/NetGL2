using System.Numerics;
using Microsoft.VisualBasic.CompilerServices;
using Vector3 = OpenTK.Mathematics.Vector3;

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
        var halfWidth = width * 0.5f;
        var halfHeight = height * 0.5f;
        var halfDepth = depth * 0.5f;

        // Define the eight vertices of the cube
        yield return new Vector3(-halfWidth, -halfHeight, -halfDepth);   // Bottom front left
        yield return new Vector3(halfWidth, -halfHeight, -halfDepth);    // Bottom front right
        yield return new Vector3(-halfWidth, halfHeight, -halfDepth);    // Top front left
        yield return new Vector3(halfWidth, halfHeight, -halfDepth);     // Top front right
        yield return new Vector3(-halfWidth, -halfHeight, halfDepth);    // Bottom back left
        yield return new Vector3(halfWidth, -halfHeight, halfDepth);     // Bottom back right
        yield return new Vector3(-halfWidth, halfHeight, halfDepth);     // Top back left
        yield return new Vector3(halfWidth, halfHeight, halfDepth);      // Top back right
    }

    public IEnumerable<ushort> get_indices() {
        // Define the indices for each face (two triangles per face) in clockwise order
        ushort[][] face_indices = [
            [0, 1, 2, 3], // Front face
            [4, 6, 5, 7], // Back face
            [1, 5, 3, 7], // Right face
            [0, 2, 4, 6], // Left face
            [0, 4, 1, 5], // Bottom face
            [2, 3, 6, 7]  // Top face
        ];

        foreach (var face in face_indices) {
            yield return face[0];
            yield return face[1];
            yield return face[2];

            yield return face[2];
            yield return face[1];
            yield return face[3];
        }
    }

    public override string ToString() {
        return $"Cube[width:{width}, height:{height}, depth:{depth}]";
    }
}