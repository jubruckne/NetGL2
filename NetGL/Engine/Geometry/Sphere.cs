using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetGL;
using OpenTK.Mathematics;

namespace NetGL;

public class Sphere: IShape<UVSphereGenerator.UVSphere>, IShape<IcoSphereGenerator.IcoSphere>, IShape<CubeSphereGenerator.CubeSphere> {
    public readonly float radius;

    public Sphere() : this(0.5f) {}
    public Sphere(float radius) => this.radius = radius;

    public IShapeGenerator generate() => new UVSphereGenerator(this);
    public IShapeGenerator generate(IcoSphereGenerator.IcoSphere options) => new IcoSphereGenerator(this, options);
    public IShapeGenerator generate(UVSphereGenerator.UVSphere options) => new UVSphereGenerator(this, options);
    public IShapeGenerator generate(CubeSphereGenerator.CubeSphere options) => new CubeSphereGenerator(this, options);

    public override string ToString() {
        return $"Sphere[radius:{radius}]";
    }
}

public static class sp {
    public static IcoSphereGenerator.IcoSphere IcoSphere(this Sphere sphere, int tesselation = 2) =>
        new IcoSphereGenerator.IcoSphere(tesselation);
}

public class IcoSphereGenerator : IShapeGenerator {
    public record struct IcoSphere(int tesselation = 2);

    private Sphere sphere;
    private IcoSphere options;

    public IcoSphereGenerator(in Sphere sphere, in IcoSphere options = new()) {
        this.sphere = sphere;
        this.options = options;
    }

    public ReadOnlySpan<Vector3> get_vertices() {
        var t = (float)((1.0 + Math.Sqrt(5.0)) / 2.0);
        var vert = new[] {
            new Vector3(-1, t, 0),
            new Vector3(1, t, 0),
            new Vector3(-1, -t, 0),
            new Vector3(1, -t, 0),

            new Vector3(0, -1, t),
            new Vector3(0, 1, t),
            new Vector3(0, -1, -t),
            new Vector3(0, 1, -t),

            new Vector3(t, 0, -1),
            new Vector3(t, 0, 1),
            new Vector3(-t, 0, -1),
            new Vector3(-t, 0, 1)
        };

        // normalize vector to unit length
        for (var i = 0; i < vert.Length; i++)
            vert[i].Normalize();

        return vert;
    }

    public ReadOnlySpan<Vector3i> get_indices() {
        return work().ToArray().AsSpan();

        IEnumerable<Vector3i> work() {
            // 5 faces around point 0
            yield return (0, 5, 11);
            yield return (0, 1, 5);
            yield return (0, 7, 1);
            yield return (0, 10, 7);
            yield return (0, 11, 10);

            // 5 adjacent faces
            yield return (1, 9, 5);
            yield return (5, 4, 11);
            yield return (11, 2, 10);
            yield return (10, 6, 7);
            yield return (7, 8, 1);

            // 5 faces around point 3
            yield return (3, 4, 9);
            yield return (3, 2, 4);
            yield return (3, 6, 2);
            yield return (3, 8, 6);
            yield return (3, 9, 8);

            // 5 adjacent faces
            yield return (4, 5, 9);
            yield return (2, 11, 4);
            yield return (6, 10, 2);
            yield return (8, 7, 6);
            yield return (9, 1, 8);
        }
    }
}

public class UVSphereGenerator: IShapeGenerator {
    public record struct UVSphere(int meridians = 32, int parallels = 24);

    private readonly Sphere sphere;
    private readonly UVSphere options;

    public UVSphereGenerator(Sphere sphere, UVSphere options = new()) {
        this.sphere = sphere;
        this.options = options;
    }

    public override string ToString() => "Sphere";

    public ReadOnlySpan<Vector3> get_vertices() {
        var list = new List<Vector3>();

        float x, y, z, xy;
        float sectorStep = (float)(2f * Math.PI / options.meridians);
        float stackStep = (float)(Math.PI / options.parallels);

        float stackAngle;
        float sectorAngle;

        for (var i = 0; i <= options.parallels; ++i) {
            stackAngle = (float)(Math.PI / 2f - i * stackStep);
            xy = sphere.radius * (float)Math.Cos(stackAngle);
            z = sphere.radius * (float)Math.Sin(stackAngle);

            for (var j = 0; j <= options.meridians; ++j) {
                sectorAngle = j * sectorStep;

                x = xy * (float)Math.Cos(sectorAngle);
                y = xy * (float)Math.Sin(sectorAngle);

                /* // Equirectangular to polar UV mapping
                float polarX = (float) Math.Atan2(x, z);
                float polarY = (float) Math.Acos(y / radius);

                u = polarX * 0.5f / (float) Math.PI + 0.5f;
                v = polarY / (float) Math.PI;
                */
                list.Add(new Vector3(x, y, z));
            }
        }

        // Duplicate vertices along one seam
        for (var i = 1; i <= options.parallels - 1; i++) {
            stackAngle = (float)Math.PI / 2 - i * stackStep;
            xy = sphere.radius * (float)Math.Cos(stackAngle);
            z = sphere.radius * (float)Math.Sin(stackAngle);

            list.Add(new Vector3(xy, 0, z)); // Add an extra vertex at the seam
        }

        return list.as_readonly_span();
    }

    public ReadOnlySpan<Vector3i> get_indices() {
        var list = new List<Vector3i>();

        // Calculate the total vertices per row, considering the extra vertex at the seam for each row
        int verticesPerRow = options.meridians + 1;

        for (int i = 1; i <= options.parallels; ++i) {
            for (int j = 1; j <= options.meridians; ++j) {
                int a = i * (options.meridians + 1) + j;
                int b = i * (options.meridians + 1) + j - 1;
                int c = (i - 1) * (options.meridians + 1) + j - 1;
                int d = (i - 1) * (options.meridians + 1) + j;

                // Adjust for extra seam vertices
                // Check if we're at the last meridian and adjust indices to wrap correctly
                if (j == options.meridians) {
                    a = i * verticesPerRow; // Wrap to the first vertex of the current row
                    d = (i - 1) * verticesPerRow; // Wrap to the first vertex of the previous row
                }

                list.Add(new Vector3i(c, b, a));
                list.Add(new(d, c, a));
            }
        }

        return list.as_readonly_span();
    }
}

public class CubeSphereGenerator: IShapeGenerator {
    public record struct CubeSphere(int subdivisions = 10);

    private readonly Sphere sphere;
    private readonly CubeSphere options;

    internal CubeSphereGenerator(in Sphere sphere, in CubeSphere options = new()) {
        this.sphere = sphere;
        this.options = options;
    }

    public override string ToString() => "Cube";

    public ReadOnlySpan<Vector3> get_vertices() {
        var list = new List<Vector3>();

        for (var face = 0; face < 6; face++) {
            foreach (var vertex in get_face_vertices(face)) {
                list.Add(vertex);
            }
        }

        return list.as_readonly_span();
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
        for (int y = 0; y <= options.subdivisions; y++) {
            for (int x = 0; x <= options.subdivisions; x++) {
                Vector3 point = normal + right * ((x / (float)options.subdivisions) - 0.5f) * 2f +
                                up * ((y / (float)options.subdivisions) - 0.5f) * 2f;
                Vector3 pointOnSphere = point.Normalized() * sphere.radius;
                yield return pointOnSphere.Zyx;
            }
        }
    }

    public ReadOnlySpan<Vector3i> get_indices() {
        var list = new List<Vector3i>();

        int verticesPerRow = options.subdivisions + 1;
        int faceVertexCount = verticesPerRow * verticesPerRow;

        for (int face = 0; face < 6; face++) {
            int offset = face * faceVertexCount;
            for (int y = 0; y < options.subdivisions; y++) {
                for (int x = 0; x < options.subdivisions; x++) {
                    int topLeft = offset + (y * verticesPerRow) + x;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + verticesPerRow;
                    int bottomRight = bottomLeft + 1;

                    // First triangle
                    list.Add(new Vector3i(topLeft, bottomLeft, topRight));
                    // Second triangle
                    list.Add(new Vector3i(topRight, bottomLeft, bottomRight));
                }
            }
        }

        return list.as_readonly_span();
    }
}