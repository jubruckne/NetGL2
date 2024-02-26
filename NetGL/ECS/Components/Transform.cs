using OpenTK.Mathematics;

namespace NetGL.ECS;

public class Transform : IComponent<Transform> {
    public Entity entity { get; }
    public string name { get; }

    public Vector3 position;
    public Rotation rotation;
    public float scale;

    internal Transform(in Entity entity, in string? name = null, in Vector3? position = null, in Rotation? rotation = null, float scale = 1f) {
        this.entity = entity;
        this.name = name ?? GetType().Name;
        this.position = position ?? new Vector3(0, 0, 0);
        this.rotation = rotation ?? Rotation.from_yaw_pitch_roll(yaw:-90, pitch:0, roll:0);
        this.scale = scale;
    }

    public override string ToString() {
        return $"pos: {position}, rot: {rotation}";
    }

    public Transform copy() {
        return new Transform(entity, name, position, rotation, scale);
    }

    public void copy_from(in Transform source) {
        rotation = source.rotation;
        position = source.position;
        scale = source.scale;
    }
}