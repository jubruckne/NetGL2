namespace NetGL.ECS;

public struct RenderableComponent: IComponent<RenderableComponent> {
    public Shader shader;
    public VertexArray vertex_array;

    public RenderableComponent(VertexArray vertex_array, Shader shader) {
        this.shader = shader;
        this.vertex_array = vertex_array;
    }

    public override string ToString() {
        return "Renderable";
    }
}