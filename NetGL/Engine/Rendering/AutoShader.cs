using System.Text;

namespace NetGL;

public class AutoShader: Shader {
    private AutoShader(in string name) : base(name) { }

    public static AutoShader for_vertex_type(in string name, in VertexArray vertex_array) {
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine($"Creating shader {name} for {vertex_array}");
        var vertex_code = new StringBuilder();
        var shader = new AutoShader("AUTO: " + name);

        vertex_code.AppendLine("#version 410\n");

        foreach (var vertex_attribute in vertex_array.vertex_attributes) {
            vertex_code.AppendLine($"layout (location = {vertex_attribute.location}) in {vertex_attribute.glsl_type} {vertex_attribute.name};");
        }

        vertex_code.AppendLine("uniform mat4 projection;");
        vertex_code.AppendLine("uniform mat4 camera;");
        vertex_code.AppendLine("uniform mat4 model;");
        vertex_code.AppendLine("uniform float game_time;\n");

        vertex_code.AppendLine("out VERTEX {");
        vertex_code.AppendLine("  vec3 local_position;");
        vertex_code.AppendLine("  vec3 world_position;");
        if(vertex_array.has_normals)
            vertex_code.AppendLine("  vec3 normal;");
        vertex_code.AppendLine("  vec3 frag_position;");
        vertex_code.AppendLine("} vertex;\n");

        vertex_code.AppendLine("void main() {");
        vertex_code.AppendLine("  vertex.local_position = position;");
        vertex_code.AppendLine("  vertex.world_position = (vec4(position, 1) * model).xyz;");
        if(vertex_array.has_normals)
            vertex_code.AppendLine("  vertex.normal = normal * mat3(transpose(inverse(model)));");
        vertex_code.AppendLine("  vertex.frag_position = vec3(vec4(position, 1.0) * model);");
        vertex_code.AppendLine("  gl_Position = vec4(vertex.world_position, 1) * camera * projection;");
        vertex_code.AppendLine("}");

        if(vertex_array.has_normals)
             Console.WriteLine($"AutoShader:\n{vertex_code}\n");

        var fragment_code = new StringBuilder();

        fragment_code.AppendLine("#version 410\n");

        fragment_code.AppendLine("in VERTEX {");
        fragment_code.AppendLine("  vec3 local_position;");
        fragment_code.AppendLine("  vec3 world_position;");
        if(vertex_array.has_normals)
            fragment_code.AppendLine("  vec3 normal;");
        fragment_code.AppendLine("  vec3 frag_position;");
        fragment_code.AppendLine("} vertex;\n");

        fragment_code.AppendLine("uniform float game_time;\n");
        fragment_code.AppendLine("uniform vec3 camera_position;\n");
        fragment_code.AppendLine("uniform vec3 ambient_light;\n");

        fragment_code.AppendLine("struct DirectionalLight {");
        fragment_code.AppendLine("  vec3 direction;");
        fragment_code.AppendLine("  vec3 ambient;");
        fragment_code.AppendLine("  vec3 diffuse;");
        fragment_code.AppendLine("  vec3 specular;");
        fragment_code.AppendLine("};");
        fragment_code.AppendLine("uniform DirectionalLight[2] directional_light;\n");

        fragment_code.AppendLine("struct Material {");
        fragment_code.AppendLine("  vec3 ambient;");
        fragment_code.AppendLine("  vec3 diffuse;");
        fragment_code.AppendLine("  vec3 specular;");
        fragment_code.AppendLine("  float shininess;");
        fragment_code.AppendLine("};");
        fragment_code.AppendLine("uniform Material material;\n");

        fragment_code.AppendLine("out vec4 frag_color;\n");
        fragment_code.AppendLine("""
                                   vec3 calculate_directional_light(DirectionalLight light, vec3 normal, vec3 view_direction)
                                   {
                                       vec3 light_direction = normalize(-light.direction);
                                       //diffuse shading
                                       float diff = max(dot(normal, light_direction), 0.0);
                                       //specular shading
                                       vec3 reflect_direction = reflect(-light_direction, normal);
                                       float spec = pow(max(dot(view_direction, reflect_direction), 0.0), material.shininess);
                                       //combine results
                                       vec3 ambient  = light.ambient  * material.ambient;
                                       vec3 diffuse  = light.diffuse  * diff * material.diffuse;
                                       vec3 specular = light.specular * spec * material.specular;
                                       return (ambient + diffuse + specular);
                                   }
                                 """);
        fragment_code.AppendLine();
        fragment_code.AppendLine("void main() {");
        if (vertex_array.has_normals) {
            fragment_code.AppendLine("  vec3 normal = normalize(vertex.normal);");
            fragment_code.AppendLine("  vec3 view_direction = normalize(camera_position - vertex.frag_position);");

            fragment_code.AppendLine("  //------- Ambient lighting -------");
            fragment_code.AppendLine("  vec3 light = material.ambient * ambient_light;");

            fragment_code.AppendLine("  //----- Directional lighting -----");
            fragment_code.AppendLine("  light += calculate_directional_light(directional_light[0], normal, view_direction);");
            fragment_code.AppendLine("  frag_color = vec4(light, 1);");
        } else {
            fragment_code.AppendLine("  vec3 d = directional_light[0].direction;");
            fragment_code.AppendLine(
                "  frag_color = vec4(material.ambient * (ambient_light + directional_light[0].ambient) + 0.25f, 1);");
        }

        fragment_code.AppendLine("}");

        if(vertex_array.has_normals)
            Console.WriteLine(fragment_code);

        shader.compile_from_text(vertex_code.ToString(), fragment_code.ToString());

        return shader;
    }
}