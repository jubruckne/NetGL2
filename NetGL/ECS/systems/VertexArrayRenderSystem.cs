namespace NetGL.ECS;

public class VertexArrayRenderSystem: System<TransformComponent, RenderableComponent> {
    public VertexArrayRenderSystem(): base("VertexArrayRenderSystem", on_render: render) { }

    private static void render(ref TransformComponent transform, ref RenderableComponent renderable) {
        renderable.shader.bind();
        renderable.vertex_array.bind();

    }
}