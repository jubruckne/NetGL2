namespace NetGL.ECS;

public class MaterialComponent: IComponent<MaterialComponent> {
    public Entity entity { get; }
    public string name { get; }

    public Material.Color color;

    internal MaterialComponent(in Entity entity, in Material.Color color) {
        this.entity = entity;
        this.color = color;
        name = color.ToString();
    }

    public override string ToString() {
        return $"Material({color})";
    }
}

public static class MaterialComponentExt {
    public static MaterialComponent add_material(this Entity entity, in Material.Color color) {
        var mat = new MaterialComponent(entity, color);
        entity.add(mat);
        return mat;
    }
}