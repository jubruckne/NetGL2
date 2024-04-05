using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using NetGL.ECS.Events;

namespace NetGL.ECS;

public class Entity: INamed, IEventSender<Entity, EntityComponentAdded> {
    public string name { get; }
    public string path { get; private set; }
    public Transform transform { get; }

    private Entity? parent_entity;

    public ReadOnlyEntityList parents { get; private set; }

    public Entity? parent {
        get => parent_entity;
        private set {
            parent_entity = value;
            path = name;
            ((EntityList)parents).clear();

            var p = parent;

            while (p != null) {
                ((EntityList)parents).add(p);
                path = $"{p.name}.{path}";
                p = p.parent;
            }
        }
    }

    public ReadOnlyEntityList children { get; }
    private readonly ComponentList component_list;
    public ReadOnlyComponentList components => component_list;
    private readonly List<IUpdatableComponent> updateable_components;
    private readonly List<IRenderableComponent> renderable_components;

    internal Entity(string name, Entity? parent = null, Transform? transform = null) {
        this.name = name;
        this.path = name;
        component_list = new();
        updateable_components = [];
        renderable_components = [];

        this.transform = transform ?? new Transform(this);
        if (GetType() != typeof(World)) {
            add(this.transform);
        }

        parents = new EntityList();
        this.parent = parent;
        children = new EntityList();

        if (parent != null)
            ((EntityList)parent.children).add(this);
    }
    public enum EntityRelationship {Self, Parent, ParentsRecursive, Children, ChildrenRecursive, HierarchyWithChildrenRecursive}

    /// <summary>
    /// Get the first component T in the current entity.
    /// </summary>
    public T get<T>() {
        return component_list.get<T>();
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
    public IReadOnlyList<Entity> get_all(EntityRelationship relationship) {
        switch (relationship) {
            case EntityRelationship.Self:
                return [this];

            case EntityRelationship.Parent:
                if (parent == null)
                    return [];

                return [parent];

            case EntityRelationship.Children:
                return children;

            case EntityRelationship.ChildrenRecursive:
                return children.recursive;

            case EntityRelationship.HierarchyWithChildrenRecursive:
                if (parent == null)
                    return [this];

                return parent.children.recursive;

            case EntityRelationship.ParentsRecursive:
                return parents;
        }

        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets all list of all components T of entities in the specified entity relationship
    /// </summary>
    public List<T> get_all<T>(EntityRelationship relationship) where T: class, IComponent {
        if (relationship == EntityRelationship.Self) return get_all<T>();

        var components = new List<T>();
        foreach (var entity in get_all(relationship)) {
            components.AddRange(entity.component_list.get_all<T>());
        }

        return components;
    }

    /// <summary>
    /// Gets all list of all components T of the current entity.
    /// </summary>
    public List<T> get_all<T>() where T: class, IComponent {
        return component_list.get_all<T>().ToList();
    }

    public void add<T>(in T component) where T:notnull => add(component.GetType().Name, component);

    public void add<T>(string name, in T component) where T:notnull {
        if (component is IComponent i) {
            component_list.add(i);
            if (component is IUpdatableComponent upd)
                updateable_components.Add(upd);
            if (component is IRenderableComponent rnd)
                renderable_components.Add(rnd);
        } else {
            component_list.add(new Component<T>(this, name, component));
        }
    }        

    public List<IUpdatableComponent> get_updateable_components() => updateable_components;
    public List<IRenderableComponent> get_renderable_components() => renderable_components;

    public bool try_get<T>([MaybeNullWhen(false)] out T value) where T: class {
        foreach (var component in component_list) {
            if (component is T t) {
                value = t;
                return true;
            }

            if (component is Component<T> c) {
                value = c.data;
                return true;
            }
        }

        value = null;
        return false;
    }

    public bool has<C1>() where C1: IComponent {
        foreach (var c in component_list)
            if (c is C1)
                return true;

        return false;
    }

    public bool has<C1, C2>() where C1: IComponent where C2: IComponent {
        foreach (var component in component_list.get_all<C1>())
            if (component is C2) return true;

        return false;
    }

    public bool has<C1, C2, C3>() where C1: IComponent where C2: IComponent where C3: IComponent {
        foreach (var component in component_list.get_all<C1>())
            if (component is C2 and C3) return true;

        return false;
    }

    public void send_event(Entity source, EntityComponentAdded data) {
        throw new NotImplementedException();
    }

    public override string ToString() {
        var str = $"Entity {path}:\n";

        foreach (var c in component_list) {
            str += $"  {c}\n";
        }

        str += "\n";

        return str;
    }

    public World world {
        get {
            var e = this;
            while (e != null) {
                if (e is World wrld) return wrld;
                e = e.parent;
            }

            throw new IndexOutOfRangeException("world not found!");
        }
    }
}