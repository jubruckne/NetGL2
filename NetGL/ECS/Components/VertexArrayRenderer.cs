using System.Diagnostics;
using OpenTK.Mathematics;

namespace NetGL.ECS;

public class VertexArrayRenderer : IComponent<VertexArrayRenderer>, IRenderableComponent {
    public Entity entity { get; }
    public string name { get; }
    public bool enable_updates { get; set; }

    public VertexArray vertex_array { get; }

    internal VertexArrayRenderer(in Entity entity, in VertexArray vertex_array) {
        this.entity = entity;
        this.name = GetType().Name;
        this.vertex_array = vertex_array;
    }

    public override string ToString() {
        return $"va: {vertex_array}";
    }

    public void render(in Matrix4 projection_matrix, in Matrix4 camera_matrix, in Matrix4 model_matrix) {
        var shader = entity.get<ShaderComponent>().shader;
        shader.bind();
        shader.set_projection_matrix(projection_matrix);
        shader.set_camera_matrix(camera_matrix);
        shader.set_model_matrix(model_matrix);

        if (entity.try_get<MaterialComponent>(out var mat)) {
            shader.set_material(mat!.color);
        }

        // Console.WriteLine($"projection:\n{projection_matrix}");
        // Console.WriteLine($"camera:\n{camera_matrix}");
        // Console.WriteLine($"model:\n{model_matrix}");

        vertex_array.bind();
        vertex_array.draw();

        Error.check();
    }
}

public static class VertexArrayRendererExt {
    public static VertexArrayRenderer add_vertex_array_renderer(this Entity entity, in VertexArray vertex_array) {
        var renderer = new VertexArrayRenderer(entity, vertex_array);
        entity.add(renderer);
        return renderer;
    }
}