namespace NetGL.ECS;

public class Component<T>: IComponent<T> {
    private T component_data;

    internal Component(in Entity entity, in string name, in T data) {
        this.entity = entity;
        this.name = name;
        component_data = data;
    }

    public Entity entity { get; }
    public string name { get; }

    public ref T data => ref component_data;

    public override string ToString() {
        return $"{entity.name}.{typeof(T).Name} = {data}";
    }
}