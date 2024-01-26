namespace NetGL.ECS;

public static class SpherePrefab {
    public static Entity create_sphere_uv(this World world, string name, Entity? parent = null, Transform? transform = null, float radius = 0.5f, int rows = 32, int colums = 24) {
        var entity = world.create_entity(name, parent, transform);

        var sphere_geometry = Sphere.create_geometry(radius, colums, rows);

        VertexBuffer<Vertex> vb = new(sphere_geometry.vertices);
        IndexBuffer ib = new(sphere_geometry.triangles);
        vb.upload();
        ib.upload();

        var va = new VertexArrayIndexed(vb, ib);

        va.upload();

        entity.add_shader(new Shader("default shader", "vert.glsl", "frag.glsl"));
        entity.add_vertex_array_renderer(va);

        return entity;
    }

    public static Entity create_sphere_cube(this World world, string name, Entity? parent = null, Transform? transform = null, float radius = 0.5f, int segments = 16) {
        var entity = world.create_entity(name, parent, transform);

        var sphere_geometry = Cube.create_geometry(radius, segments);

        VertexBuffer<Vertex> vb = new(sphere_geometry.vertices);
        IndexBuffer ib = new(sphere_geometry.triangles);
        vb.upload();
        ib.upload();

        var va = new VertexArrayIndexed(vb, ib);

        va.upload();

        Shader shader = new Shader("default shader", "vert.glsl", "frag.glsl");

        entity.add_shader(new Shader("default shader", "vert.glsl", "frag.glsl"));
        entity.add_vertex_array_renderer(va);

        return entity;
    }
}