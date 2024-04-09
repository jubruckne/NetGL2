using NetGL.ECS;

namespace NetGL;

public static class TreePrinter {
    public static void print(Entity entity)
        => print<INamed>(entity, get_caption, get_children, "", true, false);

    public static void print<TTreeNode>(Quadtree<TTreeNode> tree) where TTreeNode: class {
        foreach (var node in tree.root_nodes)
            print<Quadtree<TTreeNode>.Node>(node, get_caption, get_children, "", true, false);
    }

    private static IEnumerable<Quadtree<TTreeNode>.Node> get_children<TTreeNode>(Quadtree<TTreeNode>.Node node) where TTreeNode: class {
        foreach (var child in node.sub_nodes)
            yield return child;
    }

    private static string get_caption<TTreeNode>(Quadtree<TTreeNode>.Node node) where TTreeNode: class => node.ToString();

    public static void print<TItem>(TItem node, Func<TItem, string> get_caption, Func<TItem, IEnumerable<TItem>> get_children)
        => print(node, get_caption, get_children, "", true, false);

    private static void print<TItem>(TItem node, Func<TItem, string> get_caption, Func<TItem, IEnumerable<TItem>> get_children, string indent, bool root, bool last) {
        Console.Write(indent);

        if (root) {
            //Console.Write("\ud83c\udf00"); // Unique marker for the root node
        } else if (last) {
            Console.Write("\u2570\u2500");
            indent += "  ";
        } else {
            Console.Write("├─");
            indent += "\u2502 ";
        }

        Console.WriteLine(get_caption(node));

        var children = get_children(node).ToArray();

        for (var i = 0; i < children.Length; ++i)
            print(children[i], get_caption, get_children, indent, false, i == children.Length - 1);
    }

    private static string get_caption(INamed obj) => obj.name;

    private static IEnumerable<INamed> get_children(INamed obj) {
        if (obj is Entity entity) {
            foreach (var comp in entity.components)
                yield return comp;
            foreach (var child in entity.children)
                yield return child;
        }
    }
}