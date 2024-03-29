using OpenTK.Mathematics;

namespace NetGL.ECS;

public abstract class Camera: IComponent<Camera>, IUpdatableComponent {
    public Entity entity { get; }
    public string name { get; }
    public Viewport viewport { get; }
    public bool enable_input { get; set; }
    public bool enable_update { get; set; }

    public abstract void update(in float game_time, in float delta_time);

    internal Matrix4 projection_matrix { get; set; }
    internal Matrix4 camera_matrix { get; set; }

    public readonly Transform transform;

    protected Camera(in Entity entity, Viewport viewport) {
        this.entity = entity;
        this.viewport = viewport;
        name = GetType().Name;
        enable_input = true;
        enable_update = true;
        transform = entity.transform.copy();
    }
}