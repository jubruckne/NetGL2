using OpenTK.Graphics.OpenGL;

namespace NetGL.ECS;
/*
public class VertexArrayRenderSystem: System<TransformComponent, VertexArrayComponent, ShaderComponent> {
    public VertexArrayRenderSystem(): base("VertexArrayRenderSystem", on_render: render) { }

    private static void render(ref TransformComponent transform, ref VertexArrayComponent va, ref ShaderComponent shader) {
        shader.shader.bind();
        va.vertex_array.bind();

        GL.DrawElements(va.vertex_array.primitive_type, va.vertex_array., va.vertex_array.draw_elements_type, 0);

    }
}
*/