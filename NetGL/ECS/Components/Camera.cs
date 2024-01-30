using OpenTK.Mathematics;

namespace NetGL.ECS;

public abstract class Camera: IComponent<Camera>, IUpdatableComponent, IRenderableComponent {
    public Entity entity { get; }
    public string name { get; }
    public Viewport viewport { get; }
    public bool enable_input { get; set; }
    public bool enable_update { get; set; }

    public abstract void update(in float game_time, in float delta_time);

    protected Camera(in Entity entity) {
        this.entity = entity;
        name = GetType().Name;
        viewport = new();
        enable_input = true;
        enable_update = true;
    }

    public virtual void render(in Matrix4 projection_matrix, in Matrix4 camera_matrix, in Matrix4 model_matrix) {
        viewport.bind();
    }
}