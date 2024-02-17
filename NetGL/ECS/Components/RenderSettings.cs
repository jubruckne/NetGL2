using OpenTK.Mathematics;

namespace NetGL.ECS;

public class RenderSettings: IComponent<RenderSettings> {
    public Entity entity { get; }
    public string name { get; }

    public bool render = true;
    public bool cull_face = true;
    public bool depth_test = true;
    public bool wireframe = false;
    public bool blending = false;
    public bool front_facing = true;

    internal RenderSettings(in Entity entity) {
        this.entity = entity;
        this.name = GetType().Name;
    }

    public Shader shader => entity.get<ShaderComponent>().shader;
    public Matrix4 model_matrix => Matrix4.LookAt(entity.transform.position, entity.transform.attitude.direction + entity.transform.position, entity.transform.attitude.up).Inverted();
    public Material material => entity.get<MaterialComponent>().material;
    public Light[] lights => entity.get_all<Light>(Entity.EntityRelationship.ParentsRecursive).ToArray();


    public override string ToString() {
        return $"render: {render}, CullFace: {cull_face}, DepthTest: {depth_test}, Wireframe: {wireframe}, Blending: {blending}, FrontFacing: {front_facing}";
    }
}

public static class RenderSettingsExt {
    public static RenderSettings add_render_settings(this Entity entity) {
        var renderer = new RenderSettings(entity);
        entity.add(renderer);
        return renderer;
    }
}