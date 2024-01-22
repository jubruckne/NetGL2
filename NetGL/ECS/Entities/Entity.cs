namespace NetGL.ECS;

public class Entity {
    public string name { get; }
    public Transform transform { get; }
    public Entity? parent { get; }
    public IReadOnlyCollection<Entity> children{ get; }
    private readonly Dictionary<Type, Dictionary<string, IComponent>> components;
    private readonly List<IUpdatableComponent> updateable_components;
    private readonly List<IRenderableComponent> renderable_components;

    internal Entity(string name, Entity? parent, Transform? transform) {
        this.name = name;
        this.transform = transform ?? new Transform(this);
        this.components = [];
        this.updateable_components = [];
        this.renderable_components = [];

        add(this.transform);

        this.parent = parent;
        this.children = new List<Entity>();

        add(new Hierarchy(this, parent, children));

        if (parent != null) {
            ((IList<Entity>)parent.children).Add(this);
        }
    }

    public void add<T>(in T component) where T: class, IComponent {
        add(typeof(T).Name, component);
    }

    public void add<T>(string name, in T component) where T: class, IComponent {
        if (!components.TryGetValue(typeof(T), out var cl)) {
            cl = new();
            components.Add(typeof(T), cl);
        }

        if (!cl.TryAdd(name, component))
            throw new ArgumentOutOfRangeException(nameof(component), $"duplicated key: {name}!");

        if(component is IUpdatableComponent upd)
            updateable_components.Add(upd);
        if(component is IRenderableComponent rnd)
            renderable_components.Add(rnd);
    }

    internal IEnumerable<IUpdatableComponent> get_updateable_components() => updateable_components;
    internal IEnumerable<IRenderableComponent> get_renderable_components() => renderable_components;

    public IEnumerable<IComponent> get_components() {
        foreach (var comp_type in components.Values)
          foreach (var component in comp_type.Values)
             yield return component;
    }
/*
    public ref T get<T>(in string name) {
        if (!components.TryGetValue(typeof(T), out var cl))
            throw new ArgumentOutOfRangeException(nameof(T), $"key not found: {typeof(T)}!");

        if (!cl.TryGetValue(name, out var comp))
            throw new ArgumentOutOfRangeException(nameof(T), $"key not found: {typeof(T).Name} {name}!");

        return ref ((Component<T>)comp).data;
    }
*/
    public T get<T>(string? name = null) {
        if (!components.TryGetValue(typeof(T), out var cl))
            throw new ArgumentOutOfRangeException(nameof(T), $"key not found: {typeof(T).Name}!");

        if (!cl.TryGetValue(typeof(T).Name, out var comp))
            throw new ArgumentOutOfRangeException(nameof(T), $"key not found: {typeof(T).Name} {name}!");

        var col = components[typeof(T)];
        var x = col[name ?? typeof(T).Name];
        return (T)x;
    }

    public bool has<C1>() {
        return has(typeof(C1));
    }
/*
    public bool has<C1, C2>() {
        return has(typeof(C1), typeof(C2));
    }

    public bool has<C1, C2, C3>() {
        return has(typeof(C1), typeof(C2), typeof(C3));
    }

    public bool has(Type type) {
        return components.ContainsKey(type);
    }
*/

    private bool has(params Type[] types) {
        foreach (var t in types) {
            if (!components.ContainsKey(t)) return false;
        }

        return true;
    }

    public override string ToString() {
        var str = $"Entity {get_path()}:\n";

        foreach (var ct in components.Values) {
            foreach (var c in ct.Values) {
                str += $"  {c}\n";
            }
        }

        str += "\n";

        return str;
    }

    public string get_path() {
        string path = name;

        Entity? p = parent;

        while (p != null) {
            path = $"{p.name}.{path}";
            p = p.parent;
        }

        return path;
    }
}