using OpenTK.Mathematics;

namespace NetGL.ECS;

public class VertexArrayRenderer : IComponent<VertexArrayRenderer>, IRenderableComponent {
    public Entity entity { get; }
    public string name { get; }
    public bool enable_updates { get; set; }

    public IReadOnlyList<VertexArray> vertex_arrays { get; }

    internal VertexArrayRenderer(in Entity entity, IReadOnlyList<VertexArray> vertex_arrays) {
        this.entity = entity;
        this.name = GetType().Name;
        this.vertex_arrays = vertex_arrays;
    }

    public override string ToString() {
        return $"va: {vertex_arrays}";
    }

    public void render(in Matrix4 projection_matrix, in Matrix4 camera_matrix, in Vector3 camera_pos, in Matrix4 model_matrix) {
        var shader = entity.get<ShaderComponent>().shader;
        shader.bind();
        shader.set_projection_matrix(projection_matrix);
        shader.set_camera_matrix(camera_matrix);
        shader.set_model_matrix(model_matrix);
        shader.set_camera_position(camera_pos);

        if (entity.try_get<MaterialComponent>(out var mat)) {
            shader.set_material(mat!.color);
        }

        var lights = entity.get_all<Light>(Entity.EntityRelationship.ParentsRecursive);
        shader.set_light(lights);

        // Console.WriteLine($"projection:\n{projection_matrix}");
        // Console.WriteLine($"camera:\n{camera_matrix}");
        // Console.WriteLine($"model:\n{model_matrix}");

        foreach (var va in vertex_arrays) {
            va.bind();
            va.draw();
        }

        Error.check();
    }
}

public static class VertexArrayRendererExt {
    public static VertexArrayRenderer add_vertex_array_renderer(this Entity entity, IReadOnlyList<VertexArray> vertex_arrays) {
        var renderer = new VertexArrayRenderer(entity, vertex_arrays);
        entity.add(renderer);
        return renderer;
    }

    public static VertexArrayRenderer add_vertex_array_renderer(this Entity entity, VertexArray vertex_array) {
        var renderer = new VertexArrayRenderer(entity, [vertex_array]);
        entity.add(renderer);
        return renderer;
    }

}