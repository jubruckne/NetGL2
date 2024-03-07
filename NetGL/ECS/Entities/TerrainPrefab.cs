namespace NetGL.ECS;

public static class TerrainPrefab {
    public static Entity create_terrain(this World world, string name, Entity? parent = null, Transform? transform = null, int width = 100, int height = 100, int offset_x = 0, int offset_y = 0) {
        var entity = world.create_entity(name, parent, transform);

        var terrain = new Terrain(Plane.XZ, width, height, offset_x, offset_y);

        var model = Model.from_shape(terrain.generate());
        var material = Material.Copper;
        entity.add_material(material);

        entity.add_shader(AutoShader.for_vertex_type($"{name}.auto", model.vertex_arrays[0], material, is_sky_box:name == "Environment"));

        entity.add_vertex_array_renderer(model.vertex_arrays[0]);

        return entity;
    }
}