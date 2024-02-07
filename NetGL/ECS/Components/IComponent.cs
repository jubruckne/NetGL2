using OpenTK.Mathematics;

namespace NetGL.ECS;

public interface IUpdatableComponent {
    bool enable_update { get; set; }
    void update(in float game_time, in float delta_time);
}

public interface IRenderableComponent {
    void render(in Matrix4 projection_matrix, in Matrix4 camera_matrix, in Vector3 camera_pos, in Matrix4 model_matrix);
}

public interface IComponent {
    Entity entity { get; }
    string name { get; }

    string get_path() => $"{entity.get_path()}:{name}";
}

public interface IComponent<T>: IComponent where T: IComponent<T> {
//    Type type_of() => typeof(T);
//    int size_of() => Marshal.SizeOf<T>();
}