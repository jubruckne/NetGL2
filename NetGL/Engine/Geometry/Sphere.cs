using OpenTK.Mathematics;

namespace NetGL;

public class Sphere: IShape<Sphere> {
    public readonly float radius;

    public Sphere() : this(0.5f) {}
    public Sphere(float radius) => this.radius = radius;

    public IShapeGenerator generate() => generate_uv_sphere();

    public IShapeGenerator generate_uv_sphere(int meridians = 32, int parallels = 24)
        => new UVSphereGenerator(radius, meridians, parallels);

    public IShapeGenerator generate_cube_sphere(int subdivisions = 10)
        => new CubeSphereGenerator(radius, subdivisions);

    public override string ToString() {
        return $"Sphere[radius:{radius}]";
    }
}

file class UVSphereGenerator: IShapeGenerator {
    private readonly float radius;
    private readonly int meridians;
    private readonly int parallels;

    public UVSphereGenerator(in float radius, int meridians, int parallels) {
        this.radius = radius;
        this.meridians = meridians;
        this.parallels = parallels;
    }

    public override string ToString() => "Sphere";

    public IEnumerable<Vector3> get_vertices() {
        float x, y, z, xy;
        float sectorStep = (float)(2f * Math.PI / meridians);
        float stackStep = (float)(Math.PI / parallels);

        for (int i = 0; i <= parallels; ++i) {
            float stackAngle = (float)(Math.PI / 2f - i * stackStep);
            xy = radius * (float)Math.Cos(stackAngle);
            z = radius * (float)Math.Sin(stackAngle);

            for (int j = 0; j <= meridians; ++j) {
                float sectorAngle = j * sectorStep;

                x = xy * (float)Math.Cos(sectorAngle);
                y = xy * (float)Math.Sin(sectorAngle);

                /* // Equirectangular to polar UV mapping
                float polarX = (float) Math.Atan2(x, z);
                float polarY = (float) Math.Acos(y / radius);

                u = polarX * 0.5f / (float) Math.PI + 0.5f;
                v = polarY / (float) Math.PI;
                */
                yield return new Vector3(x, y, z);
            }
        }

        // Duplicate vertices along one seam
        for (int i = 1; i <= parallels - 1; i++) {
            float stackAngle = (float)Math.PI / 2 - i * stackStep;
            xy = radius * (float)Math.Cos(stackAngle);
            z = radius * (float)Math.Sin(stackAngle);

            yield return new Vector3(xy, 0, z); // Add an extra vertex at the seam
        }

    }

    public IEnumerable<Vector3i> get_indices() {
        // Calculate the total vertices per row, considering the extra vertex at the seam for each row
        int verticesPerRow = meridians + 1;

        for (int i = 1; i <= parallels; ++i) {
            for (int j = 1; j <= meridians; ++j) {
                int a = i * (meridians + 1) + j;
                int b = i * (meridians + 1) + j - 1;
                int c = (i - 1) * (meridians + 1) + j - 1;
                int d = (i - 1) * (meridians + 1) + j;

                // Adjust for extra seam vertices
                // Check if we're at the last meridian and adjust indices to wrap correctly
                if (j == meridians) {
                    a = i * verticesPerRow; // Wrap to the first vertex of the current row
                    d = (i - 1) * verticesPerRow; // Wrap to the first vertex of the previous row
                }

                yield return (c, b, a);
                yield return (d, c, a);
            }
        }
    }
}

file class CubeSphereGenerator : IShapeGenerator {
    private readonly float radius;
    private readonly int subdivisions;

    public CubeSphereGenerator(float radius, int subdivisions) {
        this.radius = radius;
        this.subdivisions = subdivisions;
    }

    public override string ToString() => "Cube";

    public IEnumerable<Vector3> get_vertices() {
        // Generate vertices for 6 faces of a cube
        // Iterating over each face of the cube to generate vertices
        for (int face = 0; face < 6; face++) {
            foreach (var vertex in get_face_vertices(face)) {
                yield return vertex;
            }
        }
    }

    private IEnumerable<Vector3> get_face_vertices(int faceIndex) {
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
                Vector3 point = normal + right * ((x / (float)subdivisions) - 0.5f) * 2f +
                                up * ((y / (float)subdivisions) - 0.5f) * 2f;
                Vector3 pointOnSphere = point.Normalized() * radius;
                yield return pointOnSphere.Zyx;
            }
        }
    }

    public IEnumerable<Vector3i> get_indices() {
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
}