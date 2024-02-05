namespace NetGL.ECS;

public class Entity {
    public string name { get; }
    public Transform transform { get; }
    public Entity? parent { get; }
    public ReadOnlyEntityList children { get; }
    private readonly ComponentList component_list;
    public ReadOnlyComponentList components => component_list;
    private readonly List<IUpdatableComponent> updateable_components;
    private readonly List<IRenderableComponent> renderable_components;

    internal Entity(string name, Entity? parent, Transform? transform) {
        this.name = name;
        component_list = [];
        updateable_components = [];
        renderable_components = [];

        this.transform = transform ?? new Transform(this);
        if (GetType() != typeof(World)) {
            add(this.transform);
        }

        this.parent = parent;
        children = new EntityList();

        if (parent != null)
            ((EntityList)parent.children).add(this);
    }

    public enum EntityRelationship {Self, Parent, ParentsRecursive, Children, ChildrenRecursive, HierarchyWithChildrenRecursive}

    /// <summary>
    /// Get the first component T in the current entity.
    /// </summary>
    public T get<T>() where T: class, IComponent {
        foreach (var component in component_list) {
            if (component is T t) return t;
            if (component.GetType().IsAssignableTo(typeof(T))) return (T)component;
        }

        throw new ArgumentOutOfRangeException(nameof(T), $"key not found: {typeof(T).Name}!");
    }

    /// <summary>
    /// Get the first component T in the specified entity relationship
    /// </summary>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public T get<T>(EntityRelationship relationship) where T: class, IComponent {
        if(relationship == EntityRelationship.Self) return get<T>();

        foreach (var component in get_all<T>(relationship))
            return component;

        throw new IndexOutOfRangeException(nameof(T));
    }

    /// <summary>
    /// Gets a list of all entities with the specified entity relationship
    /// </summary>
    public IEnumerable<Entity> get_all(EntityRelationship relationship) {
        switch (relationship) {
            case EntityRelationship.Self:
                yield return this;
                yield break;

            case EntityRelationship.Parent:
                if (parent != null)
                    yield return parent;
                yield break;

            case EntityRelationship.Children:
                foreach(var child in children)
                    yield return child;
                yield break;

            case EntityRelationship.ChildrenRecursive:
                foreach (var child in children) {
                    yield return child;
                    foreach (var child2 in child.children) {
                        yield return child2;
                        foreach (var child3 in child2.children)
                            yield return child3;
                    }
                }
                yield break;

            case EntityRelationship.HierarchyWithChildrenRecursive:
                var root = children;
                if (parent == null) {
                    yield return this;
                } else {
                    root = parent.children;
                }

                foreach (var child in root) {
                    yield return child;
                    foreach (var child2 in child.children) {
                        yield return child2;
                        foreach (var child3 in child2.children)
                            yield return child3;
                    }
                }
                yield break;

            case EntityRelationship.ParentsRecursive:
                var p = parent;
                while (p != null) {
                    yield return p;
                    p = p.parent;
                }
                yield break;
        }

        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets all list of all components T of entities in the specified entity relationship
    /// </summary>
    public IEnumerable<T> get_all<T>(EntityRelationship relationship) where T: class, IComponent {
        if(relationship == EntityRelationship.Self) yield return get<T>();

        foreach(var entity in get_all(relationship)) {
            foreach (var component in entity.component_list) {
                if (component is T t) yield return t;
            }
        }
    }

    /// <summary>
    /// Gets all list of all components T of the current entity.
    /// </summary>
    public IEnumerable<T> get_all<T>() where T: class, IComponent {
        foreach (var component in component_list) {
            if (component is T t) yield return t;
        }
    }

    public IEnumerable<Entity> parents {
        get {
            Entity? p = parent;

            while (p != null) {
                yield return p;
                p = p.parent;
            }
        }
    }

    public void add<T>(in T component) where T: class, IComponent {
        add(typeof(T).Name, component);
    }

    public void add<T>(string name, in T component) where T: class, IComponent {
        if(component is IUpdatableComponent upd)
            updateable_components.Add(upd);
        if(component is IRenderableComponent rnd)
            renderable_components.Add(rnd);

        component_list.add(component);
    }

    internal IEnumerable<IUpdatableComponent> get_updateable_components() => updateable_components;
    internal IEnumerable<IRenderableComponent> get_renderable_components() => renderable_components;

    public bool try_get<T>(out T? value, string? name = null) where T: class, IComponent {
        value = null;

        foreach (var component in component_list) {
            if (component is T t) value = t;
            if (component.GetType().IsAssignableTo(typeof(T))) value = (T)component;
        }

        return value != null;
    }

    public bool has<C1>() where C1: IComponent {
        foreach (var component in component_list)
            if (component is C1) return true;

        return false;
    }

    public bool has<C1, C2>() where C1: IComponent where C2: IComponent {
        foreach (var component in component_list)
            if (component is C1 and C2) return true;

        return false;
    }

    public bool has<C1, C2, C3>() where C1: IComponent where C2: IComponent where C3: IComponent {
        foreach (var component in component_list)
            if (component is C1 and C2 and C3) return true;

        return false;
    }

    public override string ToString() {
        var str = $"Entity {get_path()}:\n";

        foreach (var c in component_list) {
            str += $"  {c}\n";
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