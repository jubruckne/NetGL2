using OpenTK.Mathematics;

namespace NetGL.ECS;

public interface IUpdatableComponent {
    bool enable_updates{ get; set; }
    void update(in float game_time, in float delta_time);
}

public interface IRenderableComponent {
    void render(in Matrix4 projection_matrix, in Matrix4 camera_matrix, in Matrix4 model_matrix);
}

public interface IComponent {
    Entity entity { get; }
    string name { get; }
}

public interface IComponent<T>: IComponent where T: IComponent<T> {
//    Type type_of() => typeof(T);
//    int size_of() => Marshal.SizeOf<T>();
}