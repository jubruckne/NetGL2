using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace NetGL.ECS;

public class VertexArrayRenderer : IComponent<VertexArrayRenderer>, IRenderableComponent {
    public Entity entity { get; }

    public string name { get; }
    public bool enable_updates { get; set; }

    public readonly Dictionary<VertexArray, bool> vertex_arrays;

    public bool cull_face = true;
    public bool depth_test = true;
    public bool wireframe = false;
    public bool blending = false;
    public bool front_facing = true;

    public Light[] lights { get; set; }

    internal VertexArrayRenderer(in Entity entity, List<VertexArray> vertex_arrays) {
        this.entity = entity;
        this.name = GetType().Name;
        this.vertex_arrays = [];

        foreach (var va in vertex_arrays)
            this.vertex_arrays.Add(va, true);

        lights = entity.get_all<Light>(Entity.EntityRelationship.ParentsRecursive).ToArray();
    }

    public override string ToString() {
        return $"va: {vertex_arrays}";
    }

    public void render(in Matrix4 projection_matrix, in Matrix4 camera_matrix, in Vector3 camera_pos, in Matrix4 model_matrix) {
        var shader = entity.get<ShaderComponent>().shader;
        shader.bind();
        shader.set_projection_matrix(projection_matrix);
        shader.set_camera_matrix(camera_matrix);
        shader.set_model_matrix(model_matrix);
        shader.set_camera_position(camera_pos);


        shader.set_light(lights);

        // Console.WriteLine($"projection:\n{projection_matrix}");
        // Console.WriteLine($"camera:\n{camera_matrix}");
        // Console.WriteLine($"model:\n{model_matrix}");

        if(depth_test)
            GL.Enable(EnableCap.DepthTest);
        else
            GL.Disable(EnableCap.DepthTest);

        if(cull_face)
            GL.Enable(EnableCap.CullFace);
        else
            GL.Disable(EnableCap.CullFace);

        if (blending) {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        } else {
            GL.Disable(EnableCap.Blend);
        }

        GL.PolygonMode(MaterialFace.FrontAndBack, wireframe ? PolygonMode.Line : PolygonMode.Fill);
        GL.FrontFace(front_facing ? FrontFaceDirection.Ccw : FrontFaceDirection.Cw);

        foreach (var (va, enabled) in vertex_arrays) {
            if (enabled) {
                va.bind();
                va.material.ambient_texture?.bind();
                shader.set_material(va.material);

                va.draw();
            }
        }

        Debug.assert_opengl();
    }
}

public static class VertexArrayRendererExt {
    public static VertexArrayRenderer add_vertex_array_renderer(this Entity entity, IEnumerable<VertexArray> vertex_arrays) {
        var renderer = new VertexArrayRenderer(entity, vertex_arrays.ToList());
        entity.add(renderer);
        return renderer;
    }

    public static VertexArrayRenderer add_vertex_array_renderer(this Entity entity, VertexArray vertex_array) {
        var renderer = new VertexArrayRenderer(entity, [vertex_array]);
        entity.add(renderer);
        return renderer;
    }

    public static VertexArrayRenderer add_vertex_array_renderer(this Entity entity) {
        var renderer = new VertexArrayRenderer(entity, []);
        entity.add(renderer);
        return renderer;
    }
}