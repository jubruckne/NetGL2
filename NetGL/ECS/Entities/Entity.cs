using System.ComponentModel.Design;
using System.Reflection.Metadata;

namespace NetGL.ECS;

public class Entity {
    public string name { get; }
    public Transform transform { get; }
    public Entity? parent { get; }
    public IReadOnlyList<Entity> children { get; }
    private readonly Hierarchy hierarchy;
    private readonly List<IComponent> components;
    private readonly List<IUpdatableComponent> updateable_components;
    private readonly List<IRenderableComponent> renderable_components;

    internal Entity(string name, Entity? parent, Transform? transform) {
        this.name = name;
        this.components = [];
        this.updateable_components = [];
        this.renderable_components = [];

        this.transform = transform ?? new Transform(this);
        if (GetType() != typeof(World)) {
            add(this.transform);
        }

        this.parent = parent;
        children = new List<Entity>();

        add(hierarchy = new Hierarchy(this, parent, children));

        if (parent != null) {
            ((IList<Entity>)parent.children).Add(this);
        }
    }

    public enum EntityRelationship {Self, Parent, ParentsRecursive, Children, ChildrenRecursive, }

    public IEnumerable<Entity> get_entities(EntityRelationship relationship) {
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

            case EntityRelationship.ParentsRecursive:
                var p = parent;
                while (p != null) {
                    yield return p;
                    p = p.parent;
                }

                yield break;
        }

        throw new ArgumentException();
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

    public T get<T>(EntityRelationship relationship) where T: class, IComponent {
        if(relationship == EntityRelationship.Self) return get<T>();

        foreach (Entity entity in get_entities(relationship)) {
            if (entity.try_get<T>(out var component)) {
                return component!;
            }
        }

        throw new IndexOutOfRangeException(nameof(T));
    }

    public IEnumerable<T> get_all<T>(EntityRelationship relationship) where T: class, IComponent {
        if(relationship == EntityRelationship.Self) yield return get<T>();

        foreach (Entity entity in get_entities(relationship)) {
            foreach (IComponent component in entity.components) {
                if (component is T t) yield return t;
            }
        }
    }

    public IEnumerable<T> get_all<T>() where T: class, IComponent {
        foreach (var component in components) {
            if (component is T t) yield return t;
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

        components.Add(component);
    }

    internal IEnumerable<IUpdatableComponent> get_updateable_components() => updateable_components;
    internal IEnumerable<IRenderableComponent> get_renderable_components() => renderable_components;

    public IEnumerable<IComponent> get_components() {
        foreach (var component in components)
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
    public T get<T>() {
        foreach (var component in components) {
            if (component is T t) return t;
            if (component.GetType().IsAssignableTo(typeof(T))) return (T)component;
        }

        throw new ArgumentOutOfRangeException(nameof(T), $"key not found: {typeof(T).Name}!");
    }

    public bool try_get<T>(out T? value, string? name = null) where T: class, IComponent {
        value = null;

        foreach (var component in components) {
            if (component is T t) value = t;
            if (component.GetType().IsAssignableTo(typeof(T))) value = (T)component;
        }

        return value != null;
    }

    public void for_any_component_like<C1, C2, C3>(Action<IComponent> action) where C1: class, IComponent where C2: class, IComponent where C3: class, IComponent {
        foreach (var entity in hierarchy.recursive_parents) {
            if (try_get<C1>(out var c1)) action(c1!);
            if (try_get<C2>(out var c2)) action(c2!);
            if (try_get<C3>(out var c3)) action(c3!);
        }
    }

    public bool has<C1>() {
        foreach (var component in components) {
            if (component is C1 t) return true;
            if (component.GetType().IsAssignableTo(typeof(C1))) return true;
        }

        return false;
    }

    public override string ToString() {
        var str = $"Entity {get_path()}:\n";

        foreach (var c in components) {
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