namespace NetGL.ECS;

public static class CubePrefab {
    public static Entity create_cube(this World world, string name, Entity? parent = null, Transform? transform = null, float radius = 0.5f, Material? material = null) {
        var entity = world.create_entity(name, parent, transform);

        var model = Model.from_shape(new Cube(radius).generate());

        if(material == null) material = Material.Chrome;

        entity.add_material(material);
        entity.add_shader(AutoShader.for_vertex_type($"{name}.auto", model.vertex_arrays[0], material, is_sky_box:name == "Environment"));

        entity.add_vertex_array_renderer(model.vertex_arrays[0]);

        return entity;
    }
}