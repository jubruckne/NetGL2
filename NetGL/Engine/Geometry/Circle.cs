using OpenTK.Mathematics;

namespace NetGL;

public class Circle: IShape<Circle> {
    public readonly Vector3 center;
    public readonly float radius;

    public Circle(Vector3 center, float radius) {
        this.center = center;
        this.radius = radius;
    }

    public IShapeGenerator generate(int segments) => generate(segments);
    public IShapeGenerator generate() => generate(36);
    public IShapeGenerator generate(int segments, float thickness) => new CircleShapeGenerator(this, segments, thickness);

    public override string ToString() {
        return $"Circle[center: {center}, radius: {radius}]";
    }
}

public class CircleShapeGenerator : IShapeGenerator {
    private readonly Circle circle;
    private readonly int segments;
    private readonly float thickness;

    public CircleShapeGenerator(Circle circle, int segments, float thickness) {
        this.circle = circle;
        this.segments = Math.Max(3, segments);
        this.thickness = thickness;
    }

    public IEnumerable<Vector3> get_vertices() {
        List<Vector3> vertices = new List<Vector3>();
        // Adjust for both flat and thick circles
        float radiusAdjustment = thickness > 0 ? thickness / 2 : 0;

        for (int i = 0; i < segments; i++) {
            float angle = MathHelper.TwoPi * i / segments;
            if (thickness > 0) {
                // Outer vertices for thick circle
                Vector3 outerVertex = new Vector3(
                    circle.center.X + (circle.radius + radiusAdjustment) * MathF.Cos(angle),
                    circle.center.Y,
                    circle.center.Z + (circle.radius + radiusAdjustment) * MathF.Sin(angle)
                );
                vertices.Add(outerVertex);

                // Inner vertices for thick circle
                Vector3 innerVertex = new Vector3(
                    circle.center.X + (circle.radius - radiusAdjustment) * MathF.Cos(angle),
                    circle.center.Y,
                    circle.center.Z + (circle.radius - radiusAdjustment) * MathF.Sin(angle)
                );
                vertices.Add(innerVertex);
            } else {
                // Single vertex for flat circle
                Vector3 vertex = new Vector3(
                    circle.center.X + circle.radius * MathF.Cos(angle),
                    circle.center.Y,
                    circle.center.Z + circle.radius * MathF.Sin(angle)
                );
                vertices.Add(vertex);
            }
        }

        // Add center vertex for flat circle triangulation
        if (thickness == 0) {
            vertices.Add(circle.center);
        }

        return vertices;
    }

    public IEnumerable<Vector3i> get_indices() {
        List<Vector3i> indices = new List<Vector3i>();

        if (thickness > 0) {
            for (int i = 0; i < segments * 2; i += 2) {
                int nextOuter = (i + 2) % (segments * 2);
                int nextInner = (i + 3) % (segments * 2);
                // Constructing quads with two triangles for 3D circle
                indices.Add(new Vector3i(i, i + 1, nextOuter));
                indices.Add(new Vector3i(i + 1, nextInner, nextOuter));
            }
        } else {
            int centerIndex = segments; // Center vertex index for flat circle
            for (int i = 0; i < segments; i++) {
                int next = (i + 1) % segments;
                // Triangles for flat circle
                indices.Add(new Vector3i(i, next, centerIndex));
            }
        }

        return indices;
    }
}