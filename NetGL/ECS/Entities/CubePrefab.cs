namespace NetGL.ECS;

public static class CubePrefab {
    public static Entity create_cube(this World world, string name, Entity? parent = null, Transform? transform = null, float radius = 0.5f) {
        var entity = world.create_entity(name, parent, transform);

        var sphere_geometry = Cube.create_geometry(radius);

        VertexBuffer<Vertex> vb = new(sphere_geometry.vertices);
        IndexBuffer ib = new(sphere_geometry.triangles);
        vb.upload();
        ib.upload();

        var va = new VertexArrayIndexed(vb, ib);

        va.upload();

        Shader shader = new Shader("default shader", "vert.glsl", "frag.glsl");

        entity.add_vertex_array_renderer(va, shader);

        return entity;
    }
}