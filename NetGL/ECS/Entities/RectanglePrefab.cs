namespace NetGL.ECS;

public static class RectanglePrefab {
    public static Entity create_rectangle(this World world, string name, Entity? parent = null, Transform? transform = null, float width = 1f, float height = 1f, int divisions = 1) {
        var entity = world.create_entity(name, parent, transform);

        var model = Model.from_shape(new Rectangle().generate());

        entity.add_shader(AutoShader.for_vertex_type($"{name}_shader", model.vertex_arrays[0], Material.Brass));
        entity.add_vertex_array_renderer(model.vertex_arrays[0]);

        return entity;
    }
}