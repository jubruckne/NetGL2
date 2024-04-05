namespace NetGL.ECS;

public abstract class Camera: IComponent<Camera>, IUpdatableComponent {
    public Entity entity { get; }
    public string name { get; }
    public Viewport viewport { get; }
    public bool enable_input { get; set; }
    public bool enable_update { get; set; }

    public abstract void update(in float delta_time);

    public readonly Transform transform;

    public CameraData camera_data;

    protected Camera(in Entity entity, Viewport viewport) {
        this.entity = entity;
        this.viewport = viewport;
        name = GetType().Name;
        enable_input = true;
        enable_update = true;
        transform = entity.transform.copy();
    }
}