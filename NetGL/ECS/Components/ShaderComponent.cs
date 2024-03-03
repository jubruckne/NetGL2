namespace NetGL.ECS;

public class ShaderComponent: IComponent<ShaderComponent> {
    public Entity entity { get; }
    public string name { get; }
    public bool enable_update { get; set; } = true;

    public readonly Shader shader;

    internal ShaderComponent(in Entity entity, in Shader shader) {
        this.entity = entity;
        this.shader = shader;
        name = GetType().Name;
    }

    public override string ToString() {
        return $"{name}\n  uniform: {String.Join("\n  uniform: ", shader.uniforms.ToList())}";
    }
}

public static class ShaderComponentExt {
    public static ShaderComponent add_shader(this Entity entity, in Shader shader) {
        var shader_component = new ShaderComponent(entity, shader);
        entity.add(shader_component);
        return shader_component;
    }
}