using System.Collections;

namespace NetGL;

public class Quadtree<T>: IEnumerable<Quadtree<T>> where T: notnull {
    public delegate T AllocateDelegate(float x, float y, float size, int level);
    public delegate int DistanceToLevelDelegate(float distance);

    private struct Nodes {
        public readonly Quadtree<T>? parent;
        public Quadtree<T>? top_right;
        public Quadtree<T>? bottom_right;
        public Quadtree<T>? bottom_left;
        public Quadtree<T>? top_left;

        public Nodes(Quadtree<T>? parent, Quadtree<T>? top_right, Quadtree<T>? bottom_right, Quadtree<T>? bottom_left, Quadtree<T>? top_left) {
            this.parent = parent;
            this.top_right = top_right;
            this.bottom_right = bottom_right;
            this.bottom_left = bottom_left;
            this.top_left = top_left;
        }

        public Quadtree<T>? this[Bounds.Tile tile] {
            get => tile switch {
                Bounds.Tile.top_right    => top_right,
                Bounds.Tile.bottom_right => bottom_right,
                Bounds.Tile.bottom_left  => bottom_left,
                Bounds.Tile.top_left     => top_left,
                _                        => throw new ArgumentOutOfRangeException(nameof(tile), tile, null)
            };
            set {
                switch (tile) {
                    case Bounds.Tile.bottom_left:
                        bottom_left = value;
                        break;
                    case Bounds.Tile.bottom_right:
                        bottom_right = value;
                        break;
                    case Bounds.Tile.top_left:
                        top_left = value;
                        break;
                    case Bounds.Tile.top_right:
                        top_right = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(tile), tile, null);
                }
            }
        }
    }

    public readonly T data;

    public readonly int level;
    public readonly int max_level;

    public readonly Bounds bounds;

    private Nodes nodes;
    private readonly AllocateDelegate allocate;
    private readonly DistanceToLevelDelegate distance_to_level;

    public Quadtree(int most_detailed_tile_size, int max_level, AllocateDelegate allocate): this(
         new Bounds(0, 0, most_detailed_tile_size << max_level), -1, max_level, allocate) {
    }

    private Quadtree(Bounds bounds, int level, int max_level, AllocateDelegate allocate) {
        this.allocate = allocate;
        this.bounds = bounds;
        this.level = level;
        this.max_level = max_level;
        Debug.println($"new Quadtree: level={level}, max_level={max_level}, bound={bounds}");
        nodes = new();
        data = allocate(0, 0, bounds.size, level);
        distance_to_level = new DistanceToLevelDelegate(distance => (int)(distance / bounds.size).round());
    }

    public T this[float x, float y, int level] => get_node(x, y, level).data;

    public bool is_allocated(float x, float y, int level) => try_get_node(x, y, level, out _);

    public bool try_get_node(float x, float y, int level, out Quadtree<T>? node) => try_get_node(x, y, level, out node, false);

    private bool try_get_node(float x, float y, int level, out Quadtree<T>? node, bool allocate_as_needed) {
        if (level > max_level)
            throw Error.index_out_of_range(level, max_level);

        if (!bounds.intersects(x, y))
            throw Error.index_out_of_range((x, y, level));

        if (this.level > level) {
            node = nodes.parent?.get_node(x, y, level) ?? throw Error.index_out_of_range((x, y, level));
            return true;
        }

        if (this.level == level) {
            node = this;
            return true;
        }

        // we don't have this level, so check its sub-nodes.

        var new_bounds = Bounds.intersects(x, y, bounds.tiles);
        Debug.assert(new_bounds != null);

        if (nodes[new_bounds.tile] is null && allocate_as_needed) {
            nodes[new_bounds.tile] = new(new_bounds, this.level + 1, max_level, allocate);
        }
        if (nodes[new_bounds.tile] is not null) {
            node = nodes[new_bounds.tile]!.get_node(x, y, level, allocate_as_needed);
            return true;
        }

        node = null;
        return false;
    }

    public Quadtree<T> get_best_node(float x, float y, int level) {
        for (var l = level; l >= 0; --l) {
            if (try_get_node(x, y, l, out var n))
                return n!;
        }

        throw Error.index_out_of_range((x, y, level));
    }

    public Quadtree<T> get_node(float x, float y, int level) {
        if (try_get_node(x, y, level, out var n))
            return n;
        throw Error.index_out_of_range((x, y, level));
    }

    private Quadtree<T> get_node(float x, float y, int level, bool allocate_as_needed) {
        if (try_get_node(x, y, level, out var n, allocate_as_needed))
            return n!;

        throw Error.index_out_of_range((x, y, level));
    }

    public Quadtree<T> request_node(float x, float y, int level)
        => get_node(x, y, level, true);

    IEnumerator<Quadtree<T>> IEnumerable<Quadtree<T>>.GetEnumerator() {
        if (level != -1) yield return this;

        if (nodes.top_right != null)
            foreach (var q in nodes.top_right)
                yield return q;

        if (nodes.bottom_right != null)
            foreach (var q in nodes.bottom_right)
                yield return q;

        if (nodes.bottom_left != null)
            foreach (var q in nodes.bottom_left)
                yield return q;

        if (nodes.top_left != null)
            foreach (var q in nodes.top_left)
                yield return q;
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Quadtree<T>>)this).GetEnumerator();

    public override string ToString() => $"QT level = {level}, bounds = {bounds}";
}