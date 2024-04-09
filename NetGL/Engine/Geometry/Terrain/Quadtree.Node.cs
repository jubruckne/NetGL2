using System.Diagnostics;

namespace NetGL;

public sealed partial class Quadtree<T> {
    public  class Node {
        private T? node_data;
        public readonly int level;
        public readonly Bounds bounds;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Quadtree<T> tree;

        public bool has_data => node_data != null;

        public T data {
            get {
                if (node_data == null)
                    Error.not_allocated(this);

                return node_data!;
            }
        }

        private readonly Node? parent;
        private Node? top_right;
        private Node? bottom_right;
        private Node? bottom_left;
        private Node? top_left;

        private bool is_leaf
            => top_right == null && bottom_right == null && bottom_left == null && top_left == null;

        internal Node(Quadtree<T> tree, Node? parent, Bounds bounds, int level, bool allocate_data) {
            Debug.assert(this, level.is_between_including(0, tree.max_level));
            this.tree   = tree;
            this.parent = parent;
            this.bounds = bounds;
            this.level  = level;
            if(allocate_data)
                this.allocate_data();
        }

        public void allocate_data() =>
            node_data = tree.allocate(bounds, level);

        private Node? this[Bounds.Tile tile] {
            get => tile switch {
                //Bounds.Tile.parent       => parent,
                Bounds.Tile.top_right    => top_right,
                Bounds.Tile.bottom_right => bottom_right,
                Bounds.Tile.bottom_left  => bottom_left,
                Bounds.Tile.top_left     => top_left,
                _                        => throw new ArgumentOutOfRangeException(nameof(tile), tile, null)
            };
        }

        internal bool has_node(float x, float y, int level) => get_node(x, y, level, false);

        internal Node get_node(float x, float y, int level) => get_node(x, y, level, true);

        internal Result<Node> get_node(float x, float y, int level, bool create_nodes_as_needed) {
            Debug.assert(this, bounds.intersects(x, y));
            Debug.assert(this, this.level < level);

            var needed_bounds = Bounds.intersects(x, y, bounds.tiles);
            Debug.assert(needed_bounds != null);

            if (is_leaf && create_nodes_as_needed) {
                // make sure all 4 nodes on this level are allocated
                if (top_left is null)
                    top_left = new(tree, parent, bounds.top_left, this.level + 1, this.level + 1 == level);

                if (top_right is null)
                    top_right = new(tree, parent, bounds.top_right, this.level + 1, this.level + 1 == level);

                if (bottom_left is null)
                    bottom_left = new(tree, parent, bounds.bottom_left, this.level + 1, this.level + 1 == level);

                if (bottom_right is null)
                    bottom_right = new(tree, parent, bounds.bottom_right, this.level + 1, this.level + 1 == level);
            }

            var child_node = this[needed_bounds.tile];

            if (child_node is null) return
                false;

            if (child_node.level == level)
                return child_node;

            var sub_node = child_node.get_node(x, y, level, create_nodes_as_needed);
            if (!sub_node) return false;

            if(!sub_node.value.has_data)
                sub_node.value.allocate_data();

            return sub_node;

        }

        public IEnumerable<Node> sub_nodes {
            get {
                if (top_right != null) {
                    yield return top_right;
                }

                if (bottom_right != null) {
                    yield return bottom_right;
                }

                if (bottom_left != null) {
                    yield return bottom_left;
                }

                if (top_left != null) {
                    yield return top_left;
                }
            }
        }

        public override string ToString() => $"Node: level:{level} bounds:({bounds.center}, size={bounds.size}), has_data:{has_data}";
    }
}