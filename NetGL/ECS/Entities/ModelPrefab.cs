namespace NetGL.ECS;

public static class ModelPrefab {
    public static Entity create_model(this World world, string name, Model model, Entity? parent = null, Transform? transform = null) {
        var entity = world.create_entity(name, parent, transform);

        foreach (var mat in model.materials)
            entity.add_material(mat);

        entity.add_shader(AutoShader.for_vertex_type(model.name, model.vertex_arrays[0]));
        entity.add_vertex_array_renderer(model.vertex_arrays);

        return entity;
    }
}