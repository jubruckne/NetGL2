using OpenTK.Mathematics;

namespace NetGL.ECS;

public interface IUpdatableComponent {
    bool enable_update { get; set; }
    void update(in float delta_time);
}

public class RenderData {
    public Shader shader;
    public RenderSettings render_settings;
    public List<VertexArrayIndexed> vertex_arrays;
    public Matrix4 model_matrix;
}

public interface IRenderable {
    RenderData data { get; }
}

public interface IRenderableComponent {
    Light[] lights { get; internal set; }
    void render(in Camera camera, in Matrix4 model_matrix);
}

public interface IComponent: INamed {
    Entity entity { get; }
    string path => $"{entity.path}:{name}";
}

public interface IComponent<T>: IComponent {
//    Type type_of() => typeof(T);
//    int size_of() => Marshal.SizeOf<T>();
}