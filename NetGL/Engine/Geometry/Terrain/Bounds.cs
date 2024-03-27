namespace NetGL;

using OpenTK.Mathematics;

public class Bounds {
    public enum Tile { center = 0, top_left = 1, bottom_left = 2, top_right = 3, bottom_right = 4 }

    public readonly Tile tile;
    public readonly Vector2 center;

    public readonly float left;
    public readonly float right;
    public readonly float top;
    public readonly float bottom;

    public float height => top - bottom;
    public float width => right - left;
    public float size => (height + width) / 2;

    public Bounds(Tile tile, float left, float right, float bottom, float top) {
        this.tile = tile;
        this.left   = left;
        this.right  = right;
        this.bottom = bottom;
        this.top    = top;
        (center.X, center.Y) = ((left + right) * 0.5f, (top + bottom) * 0.5f);
    }

    public Bounds(Tile tile, float x, float y, float size) {
        this.tile = tile;
        var half_size = size * 0.5f;

        (center.X, center.Y) = (x, y);

        left   = x - half_size;
        right  = x + half_size;
        top    = y + half_size;
        bottom = y - half_size;
    }

    public Bounds(float x, float y, float size): this(Tile.center, x, y, size) {}

    public Bounds[] tiles => [top_right, bottom_right, bottom_left, top_left];

    public Bounds top_right => new Bounds(Tile.top_right, center.X, right, center.Y, top);
    public Bounds bottom_right => new Bounds(Tile.bottom_right, center.X, right, bottom, center.Y);
    public Bounds bottom_left => new Bounds(Tile.bottom_left, left, center.X, bottom, center.Y);
    public Bounds top_left => new Bounds(Tile.top_left, left, center.X, center.Y, top);

    public static Bounds? intersects(Vector2 point, in Bounds[] bounds) {
        foreach (var b in bounds)
            if (b.intersects(point)) return b;
        return null;
    }

    public static Bounds? intersects(float x, float y, in Bounds[] bounds) {
        foreach (var b in bounds)
            if (b.intersects(x, y)) return b;
        return null;
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

    public override string ToString() => $"<{tile} x:{center.X}, y:{center.Y}, size:{size} | left:{left} right:{right} bottom:{bottom} top:{top}>";
}