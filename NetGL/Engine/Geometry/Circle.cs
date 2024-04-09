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

    public ReadOnlySpan<Vector3> get_vertices() {
        throw new NotImplementedException();
    }

    public ReadOnlySpan<Vector3i> get_indices() {
        throw new NotImplementedException();
    }
}