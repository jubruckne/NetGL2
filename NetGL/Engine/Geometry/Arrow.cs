using OpenTK.Mathematics;

namespace NetGL;

public class Arrow: IShape<ArrowShapeGenerator.Options> {
    public readonly Vector3 from;
    public readonly Vector3 to;

    public readonly float length;

    public Arrow(Vector3 from, Vector3 to) {
        this.from = from;
        this.to = to;
        this.length = (from - to).Length;
    }

    public IShapeGenerator generate() => new ArrowShapeGenerator(this);
    public IShapeGenerator generate(ArrowShapeGenerator.Options options) => new ArrowShapeGenerator(this, options);

    public override string ToString() => $"Arrow[{from} - {to}]";
}

public class ArrowShapeGenerator: IShapeGenerator {
    public record struct Options(int segments, float width, float tip_length, float tip_width);

    private readonly Arrow arrow;
    private readonly Options options;

    internal ArrowShapeGenerator(in Arrow arrow) {
        this.arrow = arrow;

        options = new Options(
            16,
            MathF.Sqrt(arrow.length) * 0.03f,
            MathF.Sqrt(arrow.length) * 0.35f,
            MathF.Sqrt(arrow.length) * 0.1f
        );
    }

    internal ArrowShapeGenerator(in Arrow arrow, in Options options) {
        this.arrow = arrow;
        this.options.segments = Math.Max(6, this.options.segments);
    }

    public IEnumerable<Vector3> get_vertices() {
        List<Vector3> vertices = new List<Vector3>();
        Vector3 direction = Vector3.Normalize(arrow.to - arrow.from);
        // Check if direction is parallel to Vector3.UnitY
        Vector3 orthogonal;
        if (Vector3.Dot(direction, Vector3.UnitY) > 0.999f || Vector3.Dot(direction, Vector3.UnitY) < -0.999f) {
            orthogonal = Vector3.Normalize(Vector3.Cross(direction, Vector3.UnitZ));
        } else {
            orthogonal = Vector3.Normalize(Vector3.Cross(direction, Vector3.UnitY));
        }

        Vector3 binormal = Vector3.Cross(direction, orthogonal);
        orthogonal.Normalize();
        binormal.Normalize();

        // Shaft base circle
        for (int i = 0; i < options.segments; i++) {
            float angle = MathHelper.TwoPi * i / options.segments;
            Vector3 circleVec = (orthogonal * MathF.Cos(angle) + binormal * MathF.Sin(angle)) * options.width;
            Vector3 point = arrow.from + circleVec;
            vertices.Add(point); // Bottom circle of the shaft
        }

        // Shaft top circle
        Vector3 shaftTopCenter = arrow.from + direction * (Vector3.Distance(arrow.from, arrow.to) - options.tip_length);
        for (int i = 0; i < options.segments; i++) {
            float angle = MathHelper.TwoPi * i / options.segments;
            Vector3 circleVec = (orthogonal * MathF.Cos(angle) + binormal * MathF.Sin(angle)) * options.width;
            Vector3 point = shaftTopCenter + circleVec;
            vertices.Add(point); // Top circle of the shaft
        }

        // Tip base circle
        Vector3 tipBaseCenter = arrow.to - direction * options.tip_length;
        for (int i = 0; i < options.segments; i++) {
            float angle = MathHelper.TwoPi * i / options.segments;
            Vector3 circleVec = (orthogonal * MathF.Cos(angle) + binormal * MathF.Sin(angle)) * options.tip_width;
            Vector3 point = tipBaseCenter + circleVec;
            vertices.Add(point); // Base circle of the tip
        }

        vertices.Add(arrow.to); // Tip point
        vertices.Add(arrow.from); // This is the new central point of the bottom cap

        return vertices;
    }

    public IEnumerable<Vector3i> get_indices() {
        List<Vector3i> indices = new List<Vector3i>();

        var center_point_index = options.segments * 3 + 2 - 1;

        // Indices for the bottom cap
        for (int i = 0; i < options.segments; i++) {
            int next = (i + 1) % options.segments;
            indices.Add(new Vector3i(1 + next, 1 + i, center_point_index));
        }

        // Indices for the shaft
        for (int i = 0; i < options.segments; i++) {
            int next = (i + 1) % options.segments;
            // Connect bottom and top circle of the shaft
            indices.Add(new Vector3i(next, i + options.segments, i));
            indices.Add(new Vector3i(next + options.segments, i + options.segments, next));
        }

        // Indices for the tip connection
        int shaftTopStartIndex = options.segments; // Start of the top circle of the shaft
        int tipBaseStartIndex = options.segments * 2; // Start of the base circle of the tip
        for (int i = 0; i < options.segments; i++) {
            int next = (i + 1) % options.segments;
            // Connect the top circle of the shaft to the base circle of the tip
            //indices.Add(new Vector3i(shaftTopStartIndex + i, tipBaseStartIndex + i, shaftTopStartIndex + next));
            //indices.Add(new Vector3i(shaftTopStartIndex + next, tipBaseStartIndex + i, tipBaseStartIndex + next));
            indices.Add(new Vector3i(shaftTopStartIndex + next, tipBaseStartIndex + i, shaftTopStartIndex + i));
            indices.Add(new Vector3i(tipBaseStartIndex + next, tipBaseStartIndex + i, shaftTopStartIndex + next));
        }

        // Indices for the tip
        int tipPointIndex = options.segments * 3; // Index of the tip point
        for (int i = 0; i < options.segments; i++) {
            int next = (i + 1) % options.segments;
            // Connect the base circle of the tip to the tip point
            indices.Add(new Vector3i(tipBaseStartIndex + i, tipBaseStartIndex + next, tipPointIndex));
        }

        return indices;
    }
}