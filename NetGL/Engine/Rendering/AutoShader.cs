using System.Text;

namespace NetGL;

public class AutoShader: Shader {
    private AutoShader(in string name) : base(name) { }

    public static AutoShader for_vertex_type<T>(in string name) where T: IVertexSpec {
        var vertex_code = new StringBuilder();
        var shader = new AutoShader("AUTO: " + name);

        vertex_code.AppendLine("#version 410\n");

        foreach (var vertex_attribute in T.get_vertex_spec()) {
            vertex_code.AppendLine($"in {vertex_attribute.glsl_type} {vertex_attribute.name};");
        }

        vertex_code.AppendLine("uniform mat4 projection;");
        vertex_code.AppendLine("uniform mat4 camera;");
        vertex_code.AppendLine("uniform mat4 model;");
        vertex_code.AppendLine("uniform float game_time;\n");

        vertex_code.AppendLine("out VERTEX {");
        vertex_code.AppendLine("  vec3 local_position;");
        vertex_code.AppendLine("  vec3 world_position;");
        vertex_code.AppendLine("} vertex;\n");

        vertex_code.AppendLine("void main() {");
        vertex_code.AppendLine("  vertex.local_position = position;");
        vertex_code.AppendLine("  vertex.world_position = (vec4(position, 1) * model).xyz;");
        vertex_code.AppendLine("  gl_Position = vec4(vertex.world_position, 1) * camera * projection;");
        vertex_code.AppendLine("}");

        Console.WriteLine($"AutoShader:\n{vertex_code}\n");

        var fragment_code = new StringBuilder();

        fragment_code.AppendLine("#version 410\n");

        fragment_code.AppendLine("in VERTEX {");
        fragment_code.AppendLine("  vec3 local_position;");
        fragment_code.AppendLine("  vec3 world_position;");
        fragment_code.AppendLine("} vertex;\n");

        fragment_code.AppendLine("uniform float game_time;\n");

        fragment_code.AppendLine("struct Material {");
        fragment_code.AppendLine("  vec4 ambient;");
        fragment_code.AppendLine("  vec4 diffuse;");
        fragment_code.AppendLine("  vec4 specular;");
        fragment_code.AppendLine("  float shininess;");
        fragment_code.AppendLine("};");
        fragment_code.AppendLine("uniform Material material;\n");

        fragment_code.AppendLine("out vec4 frag_color;\n");

        fragment_code.AppendLine("void main() {");
        fragment_code.AppendLine("  frag_color = material.ambient;");
        fragment_code.AppendLine("}");

        Console.WriteLine(fragment_code);

        shader.compile_from_text(vertex_code.ToString(), fragment_code.ToString());

        return shader;
    }
}