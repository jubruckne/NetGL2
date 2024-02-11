using OpenTK.Mathematics;

namespace NetGL.ECS;

public abstract class Light : IComponent<Light> {
    public Entity entity { get; }
    public string name { get; }

    protected Light(in Entity entity) {
        this.entity = entity;
        name = GetType().Name;
    }
};

public abstract class Light<TLight>: Light, IComponent<Light<TLight>> {
    public TLight data;

    protected Light(in Entity entity, in TLight data): base(entity) {
        this.data = data;
    }

    public override string ToString() {
        return $"{data}";
    }
}

public static class LightComponentExt {
    public static AmbientLight add_ambient_light(this Entity entity, float r, float g, float b, float a = 1f) {
        var light = new AmbientLight(entity, color:(r, g, b, a));
        entity.add(light);
        return light;
    }

    public static DirectionalLight add_directional_light(this Entity entity, in Vector3 direction = default, in Color ambient = default, Color diffuse = default, in Color specular = default) {
        var light = new DirectionalLight(entity, direction, ambient, diffuse, specular);
        entity.add(light);
        return light;
    }

}