namespace NetGL.ECS;

public static class CubePrefab {
    public static Entity create_cube(this World world, string name, Entity? parent = null, Transform? transform = null, float radius = 0.5f) {
        var entity = world.create_entity(name, parent, transform);

        var model = Model.from_shape(Cube.make(radius));

        entity.add_shader(new Shader("default shader", "vert.glsl", "frag.glsl"));

        entity.add_vertex_array_renderer(model.vertex_arrays[0]);

        return entity;
    }
}