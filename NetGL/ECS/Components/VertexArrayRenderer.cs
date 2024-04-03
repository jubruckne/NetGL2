using OpenTK.Mathematics;
namespace NetGL.ECS;

public class VertexArrayRenderer: IComponent<VertexArrayRenderer>, IRenderableComponent {
    public Entity entity { get; }

    public string name { get; }
    public bool enable_updates { get; set; }

    public readonly Dictionary<VertexArray, bool> vertex_arrays;

    public bool cull_face = true;
    public bool depth_test = true;
    public bool wireframe = false;
    public bool blending = false;
    public bool front_facing = true;

    public Light[] lights { get; set; }

    internal VertexArrayRenderer(in Entity entity, List<VertexArray> vertex_arrays) {
        this.entity        = entity;
        this.name          = GetType().Name;
        this.vertex_arrays = [];

        foreach (var va in vertex_arrays)
            this.vertex_arrays.Add(va, true);

        lights = entity.get_all<Light>(Entity.EntityRelationship.ParentsRecursive).ToArray();
    }

    public override string ToString() {
        return $"{name}: {entity.name}";
    }

    public void render(in Camera camera,
                       in Matrix4 model_matrix
    ) {
        var shader = entity.get<ShaderComponent>().shader;

        //camera.uniforms.bind(0);
        //shader.set_uniform_buffer(camera.uniforms);

        shader.set_projection_matrix(camera.camera_data.projection_matrix);
        shader.set_camera_matrix(camera.camera_data.camera_matrix);
        shader.set_camera_position(camera.camera_data.camera_position);
        shader.model_matrix.data = model_matrix;
        shader.shared_uniforms.data = camera.camera_data;

        Debug.assert_opengl(this);


        shader.set_light(lights);

        // Console.WriteLine($"projection:\n{projection_matrix}");
        // Console.WriteLine($"camera:\n{camera_matrix}");
        // Console.WriteLine($"model:\n{model_matrix}");

        RenderState.shader.bind(shader);
        RenderState.depth_test.value   = depth_test;
        RenderState.cull_face.value    = cull_face;
        RenderState.blending.value     = blending;
        RenderState.wireframe.value    = wireframe;
        RenderState.front_facing.value = front_facing;

        foreach (var (va, enabled) in vertex_arrays) {
            if (enabled) {
                va.bind();

                va.material.ambient_texture?.bind(0);

                shader.set_material(va.material);

                if (shader.has_tesselation_shader)
                    va.draw_patches();
                else
                    va.draw();
            }
        }

        Debug.assert_opengl();
    }
}

public static class VertexArrayRendererExt {
    public static VertexArrayRenderer add_vertex_array_renderer(this Entity entity, IEnumerable<VertexArray> vertex_arrays) {
        var renderer = new VertexArrayRenderer(entity, vertex_arrays.ToList());
        entity.add(renderer);
        return renderer;
    }

    public static VertexArrayRenderer add_vertex_array_renderer(this Entity entity, VertexArray vertex_array) {
        var renderer = new VertexArrayRenderer(entity, [vertex_array]);
        entity.add(renderer);
        return renderer;
    }

    public static VertexArrayRenderer add_vertex_array_renderer(this Entity entity) {
        var renderer = new VertexArrayRenderer(entity, []);
        entity.add(renderer);
        return renderer;
    }
}