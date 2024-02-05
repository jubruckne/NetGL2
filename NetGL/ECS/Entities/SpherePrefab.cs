namespace NetGL.ECS;

public static class SpherePrefab {
    public static Entity create_sphere_uv(this World world, string name, Entity? parent = null, Transform? transform = null, float radius = 0.5f, int rows = 32, int colums = 24) {
        var entity = world.create_entity(name, parent, transform);

        var model = Model.from_shape(new Sphere(radius));

        entity.add_shader(new Shader("default shader", "vert.glsl", "frag.glsl"));
        entity.add_vertex_array_renderer(model.vertex_arrays);

        return entity;
    }

    public static Entity create_sphere_cube(this World world, string name, Entity? parent = null, Transform? transform = null, float radius = 0.5f, int segments = 16) {
        var entity = world.create_entity(name, parent, transform);

        var model = Model.from_shape(new Sphere(radius));

        Shader shader = new Shader("default shader", "vert.glsl", "frag.glsl");

        entity.add_shader(new Shader("default shader", "vert.glsl", "frag.glsl"));
        entity.add_vertex_array_renderer(model.vertex_arrays[0]);

        return entity;
    }
}