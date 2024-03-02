using System.Runtime.InteropServices.JavaScript;

namespace NetGL.ECS;

public static class SpherePrefab {
    private static (Sphere sphere, Model model, Shader shader)? last = null;

    public static Entity create_sphere_uv(this World world, string name, Entity? parent = null, Transform? transform = null, float radius = 0.5f, int rows = 32, int colums = 24, Material? material = null) {
        var entity = world.create_entity(name, parent, transform);

        Model model;
        Shader shader;

        if(material == null) material = Material.Chrome;

        if (last.HasValue && last.Value.sphere.radius == radius) {
            model = last.Value.model;
            shader = last.Value.shader;
        } else {
            Console.WriteLine("creating uv sphere");
            Sphere sphere = new(radius);
            model = Model.from_shape(sphere.generate_uv_sphere(32, 24));
            shader = AutoShader.for_vertex_type($"{name}.auto", model.vertex_arrays[0], material);
            //shader = new Shader("auto", "vert.glsl", "frag.glsl");
            last = (sphere, model, shader);
        }

        entity.add_material(material);
        entity.add_shader(shader);
        entity.add_vertex_array_renderer(model.vertex_arrays[0]);

        return entity;
    }

    public static Entity create_sphere_cube(this World world, string name, Entity? parent = null, Transform? transform = null, float radius = 0.5f, int segments = 16, Material? material = null) {
        var entity = world.create_entity(name, parent, transform);

        Model model;
        Shader shader;

        if(material == null) material = Material.Chrome;

        if (last.HasValue && last.Value.sphere.radius == radius) {
            model = last.Value.model;
            shader = last.Value.shader;
        } else {
            Sphere sphere = new Sphere(radius);
            model = Model.from_shape(sphere.generate_cube_sphere(segments));
            shader = AutoShader.for_vertex_type($"{name}.auto", model.vertex_arrays[0], material, is_sky_box:name == "Environment");
            last = (sphere, model, shader);
        }

        entity.add_material(material);
        entity.add_shader(shader);
        entity.add_vertex_array_renderer(model.vertex_arrays[0]);

        Error.check();

        return entity;
    }
}