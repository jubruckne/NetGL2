using OpenTK.Mathematics;

namespace NetGL.ECS;

public static class ArrowPrefab {
    public static Entity create_arrow(this World world, string name, Entity? parent = null, Vector3? from = null, Vector3? to = null, Material? material = null) {
        var entity = world.create_entity(name, parent);

        if (to == null) {
            if (from == null)
                to = -Vector3.UnitZ;
            else
                to = from * 1.5f;
        }
        if(from == null)
            from = Vector3.Zero;

        if(material == null) material = Material.Chrome;

        var model = Model.from_shape(new Arrow(from.Value, to.Value).generate(), material);


        entity.add_shader(AutoShader.for_vertex_type($"{name}.auto", model.vertex_arrays[0], is_sky_box:name == "Environment"));

        entity.add_vertex_array_renderer(model.vertex_arrays[0]);

        return entity;
    }
}