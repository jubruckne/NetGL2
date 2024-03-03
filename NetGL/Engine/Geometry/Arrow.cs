using OpenTK.Mathematics;

namespace NetGL;

public class Arrow: IShape<Arrow> {
    public readonly Vector3 from;
    public readonly Vector3 to;

    public readonly float length;

    public Arrow(Vector3 from, Vector3 to) {
        this.from = from;
        this.to = to;
        this.length = (from - to).Length;
    }

    public IShapeGenerator generate(int segments) => new ArrowShapeGenerator(this, segments, length * 0.025f, length * 0.1f, length * 0.1f);
    public IShapeGenerator generate() => generate(12);

    public override string ToString() {
        return $"Arrow[{from} - {to}]";
    }
}

public class ArrowShapeGenerator : IShapeGenerator {
    private readonly Arrow arrow;
    private readonly float width;
    private readonly float tip_length;
    private readonly float tip_width;
    private readonly int segments;

    public ArrowShapeGenerator(Arrow arrow, int segments, float width, float tip_length, float tip_width) {
        this.arrow = arrow;
        this.width = width;
        this.tip_length = tip_length;
        this.tip_width = tip_width;
        this.segments = Math.Max(6, segments);
    }

    public IEnumerable<Vector3> get_vertices() {
        List<Vector3> vertices = new List<Vector3>();
        Vector3 direction = Vector3.Normalize(arrow.to - arrow.from);
        Vector3 orthogonal = Vector3.Normalize(Vector3.Cross(direction, Vector3.UnitY));
        if (orthogonal.Length.Equals(0)) orthogonal = Vector3.Normalize(Vector3.Cross(direction, Vector3.UnitX));
        Vector3 binormal = Vector3.Cross(direction, orthogonal);
        orthogonal.Normalize();
        binormal.Normalize();

        float shaftLength = Vector3.Distance(arrow.from, arrow.to) - tip_length;
        Vector3 shaftDirection = direction * shaftLength;

        // Generate vertices for the cylindrical shaft
        for (int i = 0; i < segments; i++) {
            float angle = MathHelper.TwoPi * i / segments;
            Vector3 circleVec = (orthogonal * MathF.Cos(angle) + binormal * MathF.Sin(angle)) * width;
            Vector3 bottom = arrow.from + circleVec;
            Vector3 top = arrow.from + circleVec + shaftDirection;

            vertices.Add(bottom);
            vertices.Add(top);
        }

        // Generate vertices for the conical tip
        Vector3 tipBaseCenter = arrow.to - direction * tip_length;
        for (int i = 0; i < segments; i++) {
            float angle = MathHelper.TwoPi * i / segments;
            Vector3 circleVec = (orthogonal * MathF.Cos(angle) + binormal * MathF.Sin(angle)) * tip_width;
            Vector3 baseVertex = tipBaseCenter + circleVec;

            vertices.Add(baseVertex);
        }

        vertices.Add(arrow.to); // Tip point

        return vertices;
    }

    public IEnumerable<Vector3i> get_indices() {
        List<Vector3i> indices = new List<Vector3i>();

        // Indices for the cylindrical shaft
        for (int i = 0; i < segments; i++) {
            int next = (i + 1) % segments;
            indices.Add(new Vector3i(i * 2, next * 2, i * 2 + 1));
            indices.Add(new Vector3i(next * 2, next * 2 + 1, i * 2 + 1));
        }

        // Indices for the conical tip
        int baseIndex = segments * 2; // Starting index for the tip's base vertices
        for (int i = 0; i < segments; i++) {
            int next = (i + 1) % segments;
            indices.Add(new Vector3i(baseIndex + i, baseIndex + next, segments * 2 + segments)); // Base to tip point
        }

        return indices;
    }
}