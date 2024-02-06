namespace NetGL.ECS;

public class MaterialComponent: IComponent<MaterialComponent> {
    public Entity entity { get; }
    public string name { get; }

    public Material material;

    internal MaterialComponent(in Entity entity, in Material material) {
        this.entity = entity;
        this.material = material;

        name = $"{GetType().Name}_{entity.get_all<MaterialComponent>().Count()}";
    }

    public override string ToString() {
        return $"Material({material})";
    }
}

public static class MaterialComponentExt {
    public static MaterialComponent add_material(this Entity entity, in Material material) {
        var mat = new MaterialComponent(entity, material);
        entity.add(mat);
        return mat;
    }
}