using OpenTK.Mathematics;

namespace NetGL;

public readonly struct Rectangle: IShape, IComparable<Rectangle> {
    public readonly float2 bottom_left;
    public readonly float2 top_right;

    public float x => bottom_left.x;
    public float y => bottom_left.y;
    public float width => top_right.x - bottom_left.x;
    public float height => top_right.y - bottom_left.y;
    public float left => bottom_left.x;
    public float right => top_right.x;
    public float top => top_right.y;
    public float bottom => bottom_left.y;

    public float2 center => (bottom_left + top_right) * 0.5f;

    public Rectangle(float x, float y, float width, float height) {
        bottom_left = new float2(x, y);
        top_right = new float2(x + width, y + height);
    }

    public Rectangle(float2 bottom_left, float2 top_right) {
        this.bottom_left = bottom_left;
        this.top_right = top_right;
    }

    public static Rectangle centered_at(float2 center, float size) {
        float half_size = size * 0.5f;
        return new Rectangle(center.x - half_size, center.y - half_size, size, size);
    }
/*
    public Rectangle(in float2 bottom_left, in float size) {
        this.bottom_left = bottom_left;
        this.top_right = bottom_left + (size, size);
    }
*/
    public static implicit operator Rectangle((float x, float y, float width, float height) rect)
        => new(rect.x, rect.y, rect.width, rect.height);

    public static implicit operator Rectangle((float2 bottom_left, float2 top_right) rect)
        => new(rect.bottom_left, rect.top_right);

    public static Rectangle operator+(Rectangle rectangle, float2 offset)
        => new(rectangle.bottom_left + offset, rectangle.top_right + offset);

    public static Rectangle operator-(Rectangle rectangle, float2 offset)
        => new(rectangle.bottom_left - offset, rectangle.top_right - offset);

    public static Rectangle operator*(Rectangle rectangle, float scale)
        => new(rectangle.bottom_left * scale, rectangle.top_right * scale);

    public static Rectangle operator/(Rectangle rectangle, float scale)
        => new(rectangle.bottom_left / scale, rectangle.top_right / scale);

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

    public int CompareTo(Rectangle other)
        => bottom_left == other.bottom_left
            ? top_right.CompareTo(other.top_right)
            : bottom_left.CompareTo(other.bottom_left);

    public override int GetHashCode()
        => HashCode.Combine(bottom_left.GetHashCode(), top_right.GetHashCode());

    public override string ToString() => $"Rectangle[{bottom_left}:{top_right}]";
}