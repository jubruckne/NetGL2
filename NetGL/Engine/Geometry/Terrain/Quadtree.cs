namespace NetGL;

public class Quadtree<T> where T: notnull {
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
    }

    public readonly T data;

    public readonly int level;
    public readonly int max_level;

    public readonly Bounds bounds;

    private Nodes nodes;
    private readonly AllocateDelegate allocate;
    private readonly DistanceToLevelDelegate distance_to_level;

    public Quadtree(int most_detailed_tile_size, int max_level, AllocateDelegate allocate): this(
         new Bounds(0, 0, most_detailed_tile_size << max_level), 0, max_level, allocate) {


        //Debug.assert(int.IsPow2(max_level));
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

        // we don't have this level, so check it's sub-nodes.

        var new_bounds = bounds.tile();

        if (x < 0) {
            if (y < 0) {
                // bottom left
                if (nodes.bottom_left is null && allocate_as_needed)
                    nodes.bottom_left = new(new_bounds.bottom_left, this.level + 1, max_level, allocate);

                if (nodes.bottom_left is not null) {
                    node = nodes.bottom_left.get_node(x, y, level, allocate_as_needed);
                    return true;
                }
            }

            // top left
            if (nodes.top_left is null && allocate_as_needed)
                nodes.top_left = new(new_bounds.top_left, this.level + 1, max_level, allocate);

            if (nodes.top_left is not null) {
                node = nodes.top_left.get_node(x, y, level, allocate_as_needed);
                return true;
            }
        }

        if (y >= 0) {
            // top right
            if (nodes.top_right is null && allocate_as_needed)
                nodes.top_right = new(new_bounds.top_right, this.level + 1, max_level, allocate);

            if (nodes.top_right is not null) {
                node = nodes.top_right.get_node(x, y, level, allocate_as_needed);
                return true;
            }
        }

        // bottom right
        if (nodes.bottom_right is null && allocate_as_needed)
            nodes.bottom_right = new(new_bounds.bottom_right, this.level + 1, max_level, allocate);

        if (nodes.bottom_right is not null) {
            node = nodes.bottom_right.get_node(x, y, level, allocate_as_needed);
            return true;
        }

        node = null;
        return false;
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
}