using OpenTK.Mathematics;

namespace NetGL;

public class Rectangle: IShape<Rectangle> {
    public readonly float x;
    public readonly float y;
    public readonly float width;
    public readonly float height;

    public Rectangle(float x = -0.5f, float y = -0.5f, float width = 1f, float height = 1f) {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }

    public static implicit operator Rectangle((float x, float y, float width, float height) tuple) {
        return new Rectangle(tuple.x, tuple.y, tuple.width, tuple.height);
    }

    public IEnumerable<Vector3> get_vertices() => get_vertices(1);

    public IEnumerable<Vector3> get_vertices(int divisions) {
        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;
        float stepX = width / divisions;
        float stepY = height / divisions;

        // Generate vertices
        for (int i = 0; i <= divisions; i++)
        {
            for (int j = 0; j <= divisions; j++)
            {
                float x = -halfWidth + j * stepX;
                float y = -halfHeight + i * stepY;
                yield return new Vector3(x, y, 0);
            }
        }
    }

    public IEnumerable<Vector3i> get_indices() => get_indices(1);

    public IEnumerable<Vector3i> get_indices(int divisions) {
        // Generate triangles (indices)
        for (int i = 0; i < divisions; i++) {
            for (int j = 0; j < divisions; j++) {
                int v0 = i * (divisions + 1) + j;
                int v1 = v0 + 1;
                int v2 = v0 + divisions + 1;
                int v3 = v2 + 1;

                // Two triangles for each square
                yield return (v0, v1, v2);
                yield return (v1, v3, v2);
            }
        }
    }

    public IShapeGenerator generate() {
        throw new NotImplementedException();
    }

    public override string ToString() {
        return $"Rectangle[x:{x}, y:{y}, width:{width}, height:{height}]";
    }
}