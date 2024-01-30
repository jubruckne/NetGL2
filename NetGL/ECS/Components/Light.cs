using OpenTK.Mathematics;

namespace NetGL.ECS;

public abstract class Light : IComponent<Light> {
    public Entity entity{ get; }
    public string name{ get; }

    protected Light(in Entity entity) {
        this.entity = entity;
        name = GetType().Name;
    }
};

public abstract class Light<TLight>: Light, IComponent<Light<TLight>> {
    public TLight data;

    protected Light(in Entity entity): base(entity) {}

    public override string ToString() {
        return $"{data}";
    }
}

public static class LightComponentExt {
    public static AmbientLight add_ambient_light(this Entity entity, in Color4 color) {
        var light = new AmbientLight(entity, color);
        entity.add(light);
        return light;
    }

    public static DirectionalLight add_directional_light(this Entity entity, in Vector3 direction, in Color4 ambient, in Color4 diffuse, in Color4 specular) {
        var light = new DirectionalLight(entity, direction, ambient, diffuse, specular);
        entity.add(light);
        return light;
    }

}