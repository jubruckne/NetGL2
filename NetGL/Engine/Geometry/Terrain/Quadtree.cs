using System.Collections;

namespace NetGL;

public sealed class Quadtree<T>: IEnumerable<Quadtree<T>.Node> where T: notnull {
    public delegate T AllocateDelegate(in Bounds bounds, int level);

    public sealed class Node: IEnumerable<Node> {
        public readonly T data;
        public readonly int level;
        public readonly Bounds bounds;
        public readonly Quadtree<T> tree;

        public Node? parent;
        public Node? top_right;
        public Node? bottom_right;
        public Node? bottom_left;
        public Node? top_left;

        internal Node(Quadtree<T> tree, Node? parent, Bounds bounds, int level) {
            Debug.println($"new Quadtree.Node: level={level}, max_level={tree.max_level}, bound={bounds}");
            Debug.assert(level.is_between(-1, tree.max_level + 1));
            this.tree   = tree;
            this.parent = parent;
            this.bounds = bounds;
            this.level  = level;
            data        = tree.allocate(bounds, level);
        }

        internal Node? this[Bounds.Tile tile] {
            get => tile switch {
                //Bounds.Tile.parent       => parent,
                Bounds.Tile.top_right    => top_right,
                Bounds.Tile.bottom_right => bottom_right,
                Bounds.Tile.bottom_left  => bottom_left,
                Bounds.Tile.top_left     => top_left,
                _                        => throw new ArgumentOutOfRangeException(nameof(tile), tile, null)
            };
            set {
                switch (tile) {
                    //case Bounds.Tile.parent:
                    //    parent = value;
                    //    break;
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

        internal bool has_node(float x, float y, int level) => get_node(x, y, level, false);

        internal Node get_node(float x, float y, int level) => get_node(x, y, level, true);

        internal Result<Node> get_node(float x, float y, int level, bool allocate_as_needed) {
            Debug.assert(bounds.intersects(x, y));
            Debug.assert(this.level <= level);

            var new_bounds = Bounds.intersects(x, y, bounds.tiles);
            Debug.assert(new_bounds != null);

            if (this[new_bounds.tile] is not null)
                return this[new_bounds.tile]!.get_node(x, y, level, allocate_as_needed);

            if (!allocate_as_needed)
                return false;

            var node = new Node(tree, this, new_bounds, this.level + 1);
            this[new_bounds.tile] = node;

            return node.level == level
                ? node
                : node.get_node(x, y, level, allocate_as_needed);
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Node>)this).GetEnumerator();

        IEnumerator<Node> IEnumerable<Node>.GetEnumerator() {
            if (top_right != null) {
                yield return top_right;
                foreach (var q in top_right)
                    yield return q;
            }

            if (bottom_right != null) {
                yield return bottom_right;
                foreach (var q in bottom_right)
                    yield return q;
            }

            if (bottom_left != null) {
                yield return bottom_left;
                foreach (var q in bottom_left)
                    yield return q;
            }

            if (top_left != null) {
                yield return top_left;
                foreach (var q in top_left)
                    yield return q;
            }
        }

        public override string ToString() => $"Quadtree (node): level={level} bounds={bounds} data={data}";
    }

    public readonly int max_level;
    public readonly float level_max_tile_size;
    public readonly float level_0_tile_size;

    private readonly Bag<int2, Node> roots;

    private readonly AllocateDelegate allocate;

    public Quadtree(float tile_size, int max_level, AllocateDelegate allocate) {
        // tile_size: the size of the max_level tile (i.e. the smallest tile with the highest resolution)

        // level: 0 is the lowest detail level, with the largest tile size it's one of the roots
        // level: max_level is the highest detail level, with the smallest tile size (highest resolution)

        this.allocate  = allocate;
        this.max_level = max_level;

        level_0_tile_size = tile_size * (1 << max_level);
        level_max_tile_size = tile_size;

        roots = new(static (ref readonly Node item) => (int2)item.bounds.center);
    }

    public Node request_node(float x, float y, int level) {
        var nearest_x     = x.nearest_multiple(level_0_tile_size);
        var nearest_y     = y.nearest_multiple(level_0_tile_size);

        var node = roots.lookup(nearest_x, nearest_y, static (ref Node item, float x, float y) => item.bounds.intersects(x, y));
        if (node) {
            return node.value.level == level
                ? node
                : node.value.get_node(x, y, level.at_most(max_level));
        }

        var new_node = new Node(
                                this,
                                null,
                                new(
                                    Bounds.Tile.full,
                                    nearest_x,
                                    nearest_y,
                                    level_0_tile_size
                                   ),
                                0
                               );
        roots.add(new_node);

        return new_node.level == level
            ? new_node
            : new_node.get_node(x, y, level.at_most(max_level));
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Node>)this).GetEnumerator();

    IEnumerator<Node> IEnumerable<Node>.GetEnumerator() {
        for (var i = 0; i < roots.length; ++i) {
            var node = roots[i];
            yield return node;
            foreach (var q in node)
                yield return q;
        }
    }

    public override string ToString() => $"new Quadtree (root): tile size: level_0={level_0_tile_size}, level_{max_level}={level_max_tile_size}";
}