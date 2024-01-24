namespace NetGL.ECS;

public class Hierarchy: IComponent<Hierarchy> {
    public Entity entity { get; }
    public string name { get; }
    public Entity? parent;
    public IReadOnlyList<Entity> children;

    internal Hierarchy(in Entity entity, in Entity? parent, in IReadOnlyList<Entity> children) {
        this.name = GetType().Name;
        this.entity = entity;
        this.parent = parent;
        this.children = children;
    }

    public override string ToString() => entity.get_path();
}