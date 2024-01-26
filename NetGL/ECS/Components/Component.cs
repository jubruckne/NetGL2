namespace NetGL.ECS;

public class Component<T>: IComponent<T> where T: struct, IComponent<T> {
    private T component_data;

    private Component(in Entity entity, in string name, in T data) {
        this.entity = entity;
        this.name = name;
        component_data = data;
    }

    public Entity entity { get; }
    public string name { get; }

    public ref T data => ref component_data;

    public static Component<T> create(in Entity entity, in string name, in T data) => new (entity, name, data);

    public override string ToString() {
        return $"{entity.name}.{typeof(T).Name} = {data}";
    }
}

public static class ComponentExt {
    public static Component<T> add_component<T>(this Entity entity, in string name, T data)
        where T : struct, IComponent<T> {
        return Component<T>.create(entity, name, data);
    }

    public static Component<T> add_component<T>(this Entity entity, T data) where T : struct, IComponent<T> {
        return Component<T>.create(entity, typeof(T).Name, data);
    }
}