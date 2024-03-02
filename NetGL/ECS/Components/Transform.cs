using OpenTK.Mathematics;

namespace NetGL.ECS;

public class Transform: IComponent<Transform> {
    public Entity entity { get; }
    public string name { get; }

    public Vector3 position;
    public Rotation rotation;
    public float scale;

    internal Transform(in Entity entity, in string? name = null, in Vector3? position = null, in Rotation? rotation = null, float scale = 1f) {
        this.entity = entity;
        this.name = name ?? GetType().Name;
        this.position = position ?? new Vector3(0, 0, 0);
        this.rotation = rotation ?? new Rotation(yaw:0, pitch:0, roll:0);
        this.scale = scale;
    }

    public void yaw(float x) => rotation.yaw += x;
    public void pitch(float y) => rotation.pitch += y;
    public void roll(float z) => rotation.roll += z;

    public void move(float x = 0f, float y = 0f, float z = 0f) {
        if (x != 0f)
            position += rotation.right * x;
        if (y != 0f)
            position += rotation.up * y;
        if (z != 0f)
            position += rotation.forward * z;
    }

    public Matrix4 calculate_look_at_matrix()
        => Matrix4.LookAt(position, position + rotation.forward, rotation.up);

    public Matrix4 calculate_model_matrix()
        => Matrix4.LookAt(position, position + rotation.forward, rotation.up).Inverted();

    public Transform copy() {
        return new Transform(entity, name, position, rotation, scale);
    }

    public void copy_from(in Transform source) {
        rotation = source.rotation;
        position = source.position;
        scale = source.scale;
    }

    public override string ToString() {
        return $"pos: {position}, rot: {rotation}";
    }
}