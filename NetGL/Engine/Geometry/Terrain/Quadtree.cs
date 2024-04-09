using System.Diagnostics;

namespace NetGL;

public sealed partial class Quadtree<T> where T: class {
    public delegate T AllocateDelegate(in Bounds bounds, int level);

    public readonly int max_level;
    public readonly float[] tile_size_by_level;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Bag<int2, Node> nodes;

    private readonly AllocateDelegate allocate;

    public Quadtree(float tile_size, int max_level, AllocateDelegate allocate) {
        // tile_size: the size of the max_level tile (i.e. the smallest tile with the highest resolution)

        // level: 0 is the lowest detail level, with the largest tile size it's one of the roots
        // level: max_level is the highest detail level, with the smallest tile size (highest resolution)

        this.allocate  = allocate;
        this.max_level = max_level;

        tile_size_by_level = new float[max_level + 1];
        for (var i = 0; i <= max_level; i++)
            tile_size_by_level[max_level - i] = tile_size * (1 << i);

        nodes = new(static (ref readonly Node item) => (int2)item.bounds.center);
    }

    public Node request_node(float x, float y, int level) {
        Debug.assert(level <= max_level);

        var nearest_x     = x.nearest_multiple(tile_size_by_level[0]);
        var nearest_y     = y.nearest_multiple(tile_size_by_level[0]);

        var node = nodes.lookup(nearest_x, nearest_y, static (ref Node item, float x, float y) => item.bounds.intersects(x, y));
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
                                    tile_size_by_level[0]
                                   ),
                                0,
                                0 == level
                               );
        nodes.add(new_node);

        return new_node.level == level
            ? new_node
            : new_node.get_node(x, y, level, true);
    }

    public IEnumerable<Node> root_nodes {
        get {
            for (var i = 0; i < nodes.length; ++i)
                yield return nodes[i];
        }
    }

    public override string ToString() => $"Quadtree: tile size level_0={tile_size_by_level[0]}, level_{max_level}={tile_size_by_level[max_level]}";
}