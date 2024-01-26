namespace NetGL.ECS;

public static class RectanglePrefab {
    public static Entity create_rectangle(this World world, string name, Entity? parent = null, Transform? transform = null, float width = 1f, float height = 1f, int divisions = 1) {
        var entity = world.create_entity(name, parent, transform);

        var rect_geometry = Rectangle.create_geometry(width, height, divisions);

        VertexBuffer<Vertex> vb = new(rect_geometry.vertices);
        IndexBuffer ib = new(rect_geometry.triangles);
        vb.upload();
        ib.upload();

        var va = new VertexArrayIndexed(vb, ib);

        va.upload();

        entity.add_shader(AutoShader.for_vertex_type<Vertex>("default"));
        entity.add_vertex_array_renderer(va);

        return entity;
    }
}