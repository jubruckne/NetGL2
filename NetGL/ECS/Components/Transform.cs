using OpenTK.Mathematics;

namespace NetGL.ECS;

public class Transform : IComponent<Transform> {
    public Entity entity { get; }
    public string name { get; }

    public Vector3 position;
    public Attitude attitude;

    internal Transform(in Entity entity, in string? name = null, in Vector3? position = null, in Attitude? attitude = null) {
        this.entity = entity;
        this.name = name ?? GetType().Name;
        this.position = position ?? new Vector3(0, 0, 0);
        this.attitude = attitude ?? new Attitude(yaw:-90, pitch:0, roll:0);
    }

    public override string ToString() {
        return $"pos: {position}, att: {attitude}";
    }

    public Transform copy() {
        return new Transform(entity, name, position, attitude);
    }

    public void copy_from(in Transform source) {
        attitude = source.attitude;
        position = source.position;
    }
}