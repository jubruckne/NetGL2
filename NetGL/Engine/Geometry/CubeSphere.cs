using OpenTK.Mathematics;

namespace NetGL;

public class CubeSphere: IShape<Sphere> {
    public readonly float radius;

    public CubeSphere(float radius) {
        this.radius = radius;
    }

    // Generates vertices for a cube sphere
    public IEnumerable<Vector3> get_vertices() => get_vertices(16); // Default resolution

    public IEnumerable<Vector3> get_vertices(int subdivisions) {
        // Generate vertices for 6 faces of a cube
        // Iterating over each face of the cube to generate vertices
        for (int face = 0; face < 6; face++) {
            foreach (var vertex in get_face_vertices(face, subdivisions)) {
                yield return vertex;
            }
        }
    }

    private IEnumerable<Vector3> get_face_vertices(int faceIndex, int subdivisions) {
        // Normal vectors for each face of the cube
        Vector3[] faceNormals = {
            new(1, 0, 0), new(-1, 0, 0),
            new(0, 1, 0), new(0, -1, 0),
            new(0, 0, 1), new(0, 0, -1)
        };

        // Determine the normal, right, and up vectors for the current face
        Vector3 normal = faceNormals[faceIndex];
        Vector3 right = new Vector3(normal.Y, normal.Z, normal.X);
        Vector3 up = Vector3.Cross(normal, right);

        // Generate vertices
        for (int y = 0; y <= subdivisions; y++) {
            for (int x = 0; x <= subdivisions; x++) {
                Vector3 point = normal + right * ((x / (float)subdivisions) - 0.5f) * 2f + up * ((y / (float)subdivisions) - 0.5f) * 2f;
                Vector3 pointOnSphere = point.Normalized() * radius;
                yield return pointOnSphere;
            }
        }
    }

    public IEnumerable<Vector3i> get_indices() => get_indices(16);

    public IEnumerable<Vector3i> get_indices(int subdivisions) {
        int verticesPerRow = subdivisions + 1;
        int faceVertexCount = verticesPerRow * verticesPerRow;

        for (int face = 0; face < 6; face++) {
            int offset = face * faceVertexCount;
            for (int y = 0; y < subdivisions; y++) {
                for (int x = 0; x < subdivisions; x++) {
                    int topLeft = offset + (y * verticesPerRow) + x;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + verticesPerRow;
                    int bottomRight = bottomLeft + 1;

                    // First triangle
                    yield return new Vector3i(topLeft, bottomLeft, topRight);
                    // Second triangle
                    yield return new Vector3i(topRight, bottomLeft, bottomRight);
                }
            }
        }
    }

    public override string ToString() {
        return $"CubeSphere[radius:{radius}]";
    }
}