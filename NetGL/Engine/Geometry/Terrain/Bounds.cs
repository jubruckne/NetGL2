namespace NetGL;
using OpenTK.Mathematics;

public readonly struct Bounds {
    public readonly string name;
    public readonly Vector2 center;

    public readonly float left;
    public readonly float right;
    public readonly float top;
    public readonly float bottom;

    public float height => top - bottom;
    public float width => right - left;
    public float size => (height + width) / 2;

    public Bounds(string name, float left, float right, float bottom, float top) {
        this.name = name;
        this.left   = left;
        this.right  = right;
        this.bottom = bottom;
        this.top    = top;
        (center.X, center.Y) = ((left + right) * 0.5f, (top + bottom) * 0.5f);
    }

    public Bounds(string name, float x, float y, float size) {
        this.name = name;
        var half_size = size * 0.5f;

        (center.X, center.Y) = (x, y);

        left   = x - half_size;
        right  = x + half_size;
        top    = y + half_size;
        bottom = y - half_size;
    }

    public Bounds(float x, float y, float size): this("center", x, y, size) {}

    public (Bounds top_right, Bounds bottom_right, Bounds bottom_left, Bounds top_left) tile() {
        return (
            new Bounds("top_right", center.X, right, center.Y, top),
            new Bounds("bottom_right", center.X, right, bottom, center.Y),
            new Bounds("bottom_left", left, center.X, bottom, center.Y),
            new Bounds("top_left", left, center.X, center.Y, top)
        );
    }

    public bool intersects(Vector2 point) {
        if (point.X < left || point.X > right)
            return false;
        if (point.Y < bottom || point.Y > top)
            return false;

        return true;
    }

    public bool intersects(float x, float y) {
        if (x < left || x > right)
            return false;
        if (y < bottom || y > top)
            return false;

        return true;
    }

    public override string ToString() => $"<{name} x:{center.X}, y:{center.Y}, size:{size}>";
}