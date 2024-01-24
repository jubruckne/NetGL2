using OpenTK.Mathematics;

namespace NetGL.ECS;

public class VertexArrayRenderer : IComponent<VertexArrayRenderer>, IRenderableComponent {
    public Entity entity { get; }
    public string name { get; }

    public VertexArray vertex_array { get; }
    public Shader shader { get; }

    internal VertexArrayRenderer(in Entity entity, in VertexArray vertex_array, in Shader shader) {
        this.entity = entity;
        this.name = GetType().Name;
        this.vertex_array = vertex_array;
        this.shader = shader;
    }

    public override string ToString() {
        return $"va: {vertex_array}, shader: {shader}";
    }

    public void render(in Matrix4 projection_matrix, in Matrix4 camera_matrix, in Matrix4 model_matrix) {
        shader.bind();
        shader.set_projection_matrix(projection_matrix);
        shader.set_camera_matrix(camera_matrix);
        shader.set_model_matrix(model_matrix);

        // Console.WriteLine($"projection:\n{projection_matrix}");
        // Console.WriteLine($"camera:\n{camera_matrix}");
        // Console.WriteLine($"model:\n{model_matrix}");

        vertex_array.bind();
        vertex_array.draw();

        Error.check();
    }
}

public static class VertexArrayRendererExt {
    public static VertexArrayRenderer add_vertex_array_renderer(this Entity entity, in VertexArray vertex_array, in Shader shader) {
        var renderer = new VertexArrayRenderer(entity, vertex_array, shader);
        entity.add(renderer);
        return renderer;
    }
}