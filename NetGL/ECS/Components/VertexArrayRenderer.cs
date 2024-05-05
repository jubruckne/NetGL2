using OpenTK.Mathematics;
namespace NetGL.ECS;

public class VertexArrayRenderer: IComponent<VertexArrayRenderer>, IRenderableComponent {
    public Entity entity { get; }

    public string name { get; }
    public bool enable_updates { get; set; }

    public readonly Dictionary<VertexArray, bool> vertex_arrays;

    public RenderSettings render_settings = new(
                                                cull_face: true,
                                                depth_test: true,
                                                wireframe: false,
                                                blending: false,
                                                front_facing: true,
                                                scissor_test: false
                                               );

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

    public void render(Camera camera,
                       in Matrix4 model_matrix
    ) {
        var shader = entity.get<ShaderComponent>().shader;

        RenderState.bind(shader);
        RenderState.bind(render_settings);

        //shader.set_model_matrix(model_matrix);
        if(shader.model_matrix is not null)
            shader.model_matrix.data = model_matrix;

        if (shader.uniform_buffer_camera_data is not null) {
            var m = camera.camera_data;
            //m.camera_matrix.Transpose();
            //m.projection_matrix.Transpose();
            shader.uniform_buffer_camera_data.data = m;
        } else {
            shader.set_projection_matrix(camera.camera_data.projection_matrix);
            shader.set_camera_matrix(camera.camera_data.camera_matrix);
            shader.set_camera_position(camera.camera_data.camera_position);
        }

        Debug.assert_opengl(this);
        shader.set_light(lights);

        // Console.WriteLine($"projection:\n{projection_matrix}");
        // Console.WriteLine($"camera:\n{camera_matrix}");
        // Console.WriteLine($"model:\n{model_matrix}");

        foreach (var (va, enabled) in vertex_arrays) {
            if (enabled) {
                va.bind();

                if (va.material != null) {
                    va.material.ambient_texture?.bind(0);
                    shader.set_material(va.material);
                } else if(va.material2 != null) {
                    shader.set_material(va.material2);
                }

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