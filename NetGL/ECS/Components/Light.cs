namespace NetGL.ECS;

public interface ILight {}

public abstract class Light<TLight>: ILight, IComponent<Light<TLight>> {
    public Entity entity { get; }
    public string name { get; }
    public TLight data;

    protected Light(in Entity entity) {
        this.entity = entity;
        name = GetType().Name;
    }

    public override string ToString() {
        return $"{data}";
    }
}

public static class LightComponentExt {
    public static AmbientLight add_ambient_light(this Entity entity, in Color4i color) {
        var light = new AmbientLight(entity, color);
        entity.add(light);
        return light;
    }
}