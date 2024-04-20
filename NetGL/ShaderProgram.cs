/*using Assimp;
using NetGL;
using NetGL.ECS;
using NetGL.Vectors;
using OpenTK.Mathematics;
using Material = NetGL.Material;

public class tewst {
    void test() {
        var shader = new DefaultShader(
                                       new VertexArray(
                                                       VertexArray.Type.Patches, (IVertexBuffer)null,
                                                       new Material(null)
                                                      )
                                      );
        shader.bind();
    }
}

public enum Stage {
    Vertex,
    Fragment,
    Geometry,
    TessControl,
    TessEvaluation
}

[AttributeUsage(AttributeTargets.Class)]
public class ShaderAttribute: Attribute {
    public string? source { get; }

    public ShaderAttribute(string? source = null)
        => (this.source) = (source);
}

[AttributeUsage(AttributeTargets.Class)]
public class VertexStageAttribute: Attribute;
[AttributeUsage(AttributeTargets.Class)]
public class FragmentStageAttribute: Attribute;

[AttributeUsage(AttributeTargets.Class)]
public class StagesAttribute: Attribute {
    public Stage[] stages { get; }

    public StagesAttribute(params Stage[] stages)
        => (this.stages) = (stages);
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
public class LayoutAttribute: Attribute {
    public int index { get; }

    public LayoutAttribute(int index)
        => this.index = index;
}

public abstract class ShaderProgram: INamed {
    public string name { get; }
    public virtual string code { get; } = "";

    protected ShaderProgram() {
        name = GetType().get_type_name();
    }

    public void bind() {
        //bind shader
    }
}

[Shader]
public class DefaultShader(
    [Layout(0)] VertexAttribute<float3> position,
    [Layout(1)] VertexAttribute<float3> normal,
    [Layout(2)] VertexAttribute<float2> uv,
    UniformVariable<Matrix4>? model = null,
    UniformVariable<Matrix4>? view = null,
    UniformVariable<Matrix4>? projection = null
): ShaderProgram {

    public DefaultShader(VertexArray va): this(
                                               (VertexAttribute<float3>)va.vertex_attributes[],
                                               (VertexAttribute<float3>)va.vertex_attributes[0],
                                               (VertexAttribute<float2>)va.vertex_attributes[0]
                                              ) {}

    public override string code =>
        """
            shader {
                version = 410;
                author = "John Doe";
                description = "Shader for rendering with basic diffuse lighting";
                created = "2024-01-01";
            }
        
            using lighting;  //external files to include
            using noise;
        
            vertex {
                vec3 position;
                vec3 normal;
            }
        
            uniform {
                mat4 model;
                mat4 view;
                mat4 projection;
                sampler2D map;
            }
        
            geometry {
                input_type = triangles;
                output_type = triangle_strip;
                max_vertices = 3;
            }
        
            tess_control {
                output_vertices = 3;
            }
        
            tess_evaluation {
                primitive_mode = quads;
                spacing = equal_spacing;
                order = ccw;
            }
        
            void vertex_stage(out vec3 frag_position, out vec3 frag_normal, out vec2 frag_uv) {
                frag_position = vec3(uniform.model * vec4(vertex.position, 1.0));
                frag_normal = normalize(vec3(model * vec4(vertex.normal, 0.0)));
                frag_uv = vertex.position.xy;
            }
        
            void geometry_stage(in vec3[] frag_position, in vec3[] frag_normal, in vec2[] frag_uv,
                                out vec3 geo_position, out vec3 geo_normal, out vec2 geo_uv) {
                for (int i = 0; i < gl_in.length(); ++i) {
                    geo_position = frag_position[i];
                    geo_normal = frag_normal[i];
                    geo_uv = frag_uv[i];
                    EmitVertex();
                }
                EndPrimitive();
            }
        
            void tess_control_stage(uniform in vec3[] frag_position, out vec3[] tess_position) {
                gl_TessLevelOuter[0] = 4.0;
                gl_TessLevelOuter[1] = 4.0;
                gl_TessLevelOuter[2] = 4.0;
                gl_TessLevelInner[0] = 4.0;
        
                tess_position = frag_position;
            }
        
            void tess_evaluation_stage(in vec3[] tess_position, out vec3 eval_position) {
                vec3 p0 = tess_position[0];
                vec3 p1 = tess_position[1];
                vec3 p2 = tess_position[2];
                float u = gl_TessCoord.x;
                float v = gl_TessCoord.y;
                float w = 1.0 - u - v;
                eval_position = u * p0 + v * p1 + w * p2;
            }
        
            void fragment_stage(in vec3 frag_position, in vec3 frag_normal, in vec2 frag_uv, out vec4 color) {
                color = texture(uniform.map, frag_uv);
            }
        """;
}
*/