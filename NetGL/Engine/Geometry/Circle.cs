using NetGL;
using OpenTK.Mathematics;

namespace NetGL;

public class Circle: IShape<CircleShapeGenerator.Options> {
    public readonly Vector3 center;
    public readonly float radius;

    public Circle(Vector3 center, float radius) {
        this.center = center;
        this.radius = radius;
    }

    public IShapeGenerator generate() => new CircleShapeGenerator(this);
    public IShapeGenerator generate(CircleShapeGenerator.Options options) => new CircleShapeGenerator(this, options);

    public override string ToString() {
        return $"Circle[center: {center}, radius: {radius}]";
    }
}

public class CircleShapeGenerator : IShapeGenerator {
    public record struct Options(int segments = 36, float thickness = 1f);

    private readonly Circle circle;
    private readonly Options options;

    public CircleShapeGenerator(in Circle circle, in Options options = new()) {
        this.circle = circle;
        this.options = options;
        this.options.segments = Math.Max(3, this.options.segments);
    }

    public IEnumerable<Vector3> get_vertices() {
        List<Vector3> vertices = new List<Vector3>();
        // Adjust for both flat and thick circles
        float radiusAdjustment = options.thickness > 0 ? options.thickness / 2 : 0;

        for (int i = 0; i < options.segments; i++) {
            float angle = MathHelper.TwoPi * i / options.segments;
            if (options.thickness > 0) {
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
        if (options.thickness == 0) {
            vertices.Add(circle.center);
        }

        return vertices;
    }

    public IEnumerable<Vector3i> get_indices() {
        List<Vector3i> indices = new List<Vector3i>();

        if (options.thickness > 0) {
            for (int i = 0; i < options.segments * 2; i += 2) {
                int nextOuter = (i + 2) % (options.segments * 2);
                int nextInner = (i + 3) % (options.segments * 2);
                // Constructing quads with two triangles for 3D circle
                indices.Add(new Vector3i(i, i + 1, nextOuter));
                indices.Add(new Vector3i(i + 1, nextInner, nextOuter));
            }
        } else {
            int centerIndex = options.segments; // Center vertex index for flat circle
            for (int i = 0; i < options.segments; i++) {
                int next = (i + 1) % options.segments;
                // Triangles for flat circle
                indices.Add(new Vector3i(i, next, centerIndex));
            }
        }

        return indices;
    }
}