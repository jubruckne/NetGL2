using OpenTK.Mathematics;

namespace NetGL;

public struct Rectangle: IShape<Rectangle> {
    public readonly float x;
    public readonly float y;
    public readonly float width;
    public readonly float height;

    private Rectangle(float x = -0.5f, float y = -0.5f, float width = 1f, float height = 1f) {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }

    public static Rectangle make(float width = 1f, float height = 1f) {
        return new Rectangle(-width * 0.5f, -height * 0.5f, width, height);
    }

    public static Rectangle make(float x, float y, float width, float height) {
        return new Rectangle(x, y, width, height);
    }

    public static implicit operator Rectangle((float x, float y, float width, float height) tuple) {
        return new Rectangle(tuple.x, tuple.y, tuple.width, tuple.height);
    }


    public static (Vector3[] vertices, IndexBuffer.Triangle[] triangles) create_geometry(Rectangle rectangle, int divisions = 1) {
        return create_geometry(rectangle.width, rectangle.height, divisions);
    }

    public static (Vector3[] vertices, IndexBuffer.Triangle[] triangles) create_geometry(float width = 1f, float height = 1f, int divisions = 1)
    {
        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;
        float stepX = width / divisions;
        float stepY = height / divisions;

        // Generate vertices
        List<Vector3> vertices = new List<Vector3>();
        for (int i = 0; i <= divisions; i++)
        {
            for (int j = 0; j <= divisions; j++)
            {
                float x = -halfWidth + j * stepX;
                float y = -halfHeight + i * stepY;
                vertices.Add(new Vector3(x, y, 0));
            }
        }

        // Generate triangles (indices)
        List<IndexBuffer.Triangle> triangles = new List<IndexBuffer.Triangle>();
        for (int i = 0; i < divisions; i++)
        {
            for (int j = 0; j < divisions; j++)
            {
                int v0 = i * (divisions + 1) + j;
                int v1 = v0 + 1;
                int v2 = v0 + divisions + 1;
                int v3 = v2 + 1;

                // Two triangles for each square
                triangles.Add(new IndexBuffer.Triangle((ushort)v0, (ushort)v1, (ushort)v2));
                triangles.Add(new IndexBuffer.Triangle((ushort)v1, (ushort)v3, (ushort)v2));
            }
        }

        return (vertices.ToArray(), triangles.ToArray());
    }
}