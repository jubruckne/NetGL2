using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace NetGL;

public class Quadtree<T>: IEnumerable<Quadtree<T>>
    where T: notnull {
    public delegate T AllocateDelegate(float x, float y, float size, int level);

    private struct Nodes {
        public Quadtree<T>? parent;
        public Quadtree<T>? top_right;
        public Quadtree<T>? bottom_right;
        public Quadtree<T>? bottom_left;
        public Quadtree<T>? top_left;

        public Nodes(Quadtree<T>? parent) {
            this.parent = parent;
        }

        public Nodes(Quadtree<T>? parent,
                     Quadtree<T>? top_right,
                     Quadtree<T>? bottom_right,
                     Quadtree<T>? bottom_left,
                     Quadtree<T>? top_left
        ) {
            this.parent       = parent;
            this.top_right    = top_right;
            this.bottom_right = bottom_right;
            this.bottom_left  = bottom_left;
            this.top_left     = top_left;
        }

        public Quadtree<T>? this[Bounds.Tile tile] {
            get => tile switch {
                Bounds.Tile.parent       => parent,
                Bounds.Tile.top_right    => top_right,
                Bounds.Tile.bottom_right => bottom_right,
                Bounds.Tile.bottom_left  => bottom_left,
                Bounds.Tile.top_left     => top_left,
                _                        => throw new ArgumentOutOfRangeException(nameof(tile), tile, null)
            };
            set {
                switch (tile) {
                    case Bounds.Tile.parent:
                        parent = value;
                        break;
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

    public Quadtree<T> root {
        get {
            var q = this;
            while (q.nodes.parent != null)
                q = q.nodes.parent;

            return q;
        }
    }

    private Nodes nodes;
    private readonly AllocateDelegate allocate;

    public Quadtree(float tile_size, int max_level, AllocateDelegate allocate) {
        Debug.println($"new Quadtree (root): level={max_level}, max_level={max_level}, bound={bounds}");

        this.allocate  = allocate;
        this.bounds    = new Bounds(tile_size);
        this.level     = max_level;
        this.max_level = max_level;
        nodes = new();
        data  = allocate(0, 0, bounds.size, level);
    }

    private Quadtree(Quadtree<T>? parent, Bounds bounds, int level, int max_level, AllocateDelegate allocate) {
        Debug.println($"new Quadtree: level={level}, max_level={max_level}, bound={bounds}");
        Debug.assert(level.is_between(0, max_level));

        this.allocate = allocate;
        this.bounds = bounds;
        this.level = level;
        this.max_level = max_level;
        nodes = new(parent);
        data = allocate(0, 0, bounds.size, level);
    }

    public T this[float x, float y, int level] => get_node(x, y, level).data;

    public bool has_node(float x, float y, int level) => try_get_node(x, y, level, out _);

    public bool try_get_node(float x, float y, int level, [NotNullWhen(true)] out Quadtree<T>? node) => try_get_node(x, y, level, out node, false);

    private bool try_get_node(float x, float y, int level, [NotNullWhen(true)] out Quadtree<T>? node, bool allocate_as_needed) {
        if (!bounds.intersects(x, y)) {
            if (level > 0) {
                if (nodes[Bounds.Tile.parent] is null && allocate_as_needed) {
                    Debug.assert(false);
                    //nodes[Bounds.Tile.parent] = new(null, bounds.new_bounds, (this.level - 1).at_least(0), max_level, allocate);
                    //nodes[Bounds.Tile.parent]!.nodes.bottom_right = this;
                }

                if (nodes[Bounds.Tile.parent] is not null) {
                    node = nodes[Bounds.Tile.parent]!.get_node(x, y, level, allocate_as_needed);
                    return true;
                }

            }
        }

        Debug.assert(this.level <= level);

        // we found the requested level of detail
        if (this.level == level) {
            node = this;
            return true;
        }

        // this node is not level enough, so check its sub-nodes.

        // we are already at the highest level
        if (level >= max_level) {
            node = default;
            return false;
        }

        var new_bounds = Bounds.intersects(x, y, bounds.tiles);
        Debug.assert(new_bounds != null);

        if (nodes[new_bounds.tile] is null && allocate_as_needed) {
            nodes[new_bounds.tile] = new(this, new_bounds, (this.level + 1).at_most(max_level), max_level, allocate);
            node = nodes[new_bounds.tile]!.get_node(x, y, level, true);
            return true;
        }

        if (nodes[new_bounds.tile] is not null) {
            node = nodes[new_bounds.tile]!.get_node(x, y, level, false);
            return true;
        }

        node = null;
        return false;
    }

    public Quadtree<T> get_best_node(float x, float y) {
        for (var l = max_level; l >= 0; --l) {
            if (try_get_node(x, y, l, out var n))
                return n;
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
            return n;

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