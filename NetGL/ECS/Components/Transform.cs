using OpenTK.Mathematics;

namespace NetGL.ECS;

public class Transform : IComponent<Transform> {
    public Entity entity { get; }
    public string name { get; }

    public Vector3 position;
    public Attitude attitute;
    public Vector3 forward;
    public Vector3 right;
    public Vector3 up;

    internal Transform(in Entity entity, in string? name = null, in Vector3? position = null, in Vector3? forward = null, in Vector3? up = null) {
        this.entity = entity;
        this.name = name ?? GetType().Name;
        this.position = position ?? new Vector3(0, 0, 0);
        this.forward = forward ?? new Vector3(0, 0, -1);
        this.up = up ?? new Vector3(0, 1, 0);
        this.right = Vector3.Cross(this.forward, this.up);
    }

    public override string ToString() {
        return $"pos: {position}, dir: {forward}, up: {up}";
    }
}