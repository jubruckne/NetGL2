using System.Text;

namespace NetGL;

public class AutoShader: Shader {
    private AutoShader(in string name) : base(name) { }

    static AutoShader() {
        foreach (var file in AssetManager.get_files<Shader>("generated"))
            File.Delete(file);
    }

    public static AutoShader for_vertex_type(in string name, in VertexArray vertex_array, bool is_sky_box = false, bool tesselate = false) {
        // Console.WriteLine($"\nCreating shader {name} for {vertex_array}");
        var vertex_code = new StringBuilder();
        var shader = new AutoShader(name);

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
        if(vertex_array.material.ambient_texture != null)
            vertex_code.AppendLine("  vec3 texcoord;");
        vertex_code.AppendLine("  vec3 frag_position;");
        vertex_code.AppendLine("} vertex;\n");

        vertex_code.AppendLine("void main() {");
        vertex_code.AppendLine("  vertex.local_position = position;");
        vertex_code.AppendLine("  vertex.world_position = (vec4(position, 1) * model).xyz;");

        if(vertex_array.has_normals)
            vertex_code.AppendLine("  vertex.normal = normal * mat3(transpose(inverse(model)));");

        if (vertex_array.material.ambient_texture != null) {
            if (vertex_array.material.ambient_texture is TextureCubemapBuffer) {
                vertex_code.AppendLine("  // flip y because skybox is rendered inside out");
                vertex_code.AppendLine("  vertex.texcoord = position;");
                vertex_code.AppendLine("  vertex.texcoord.y = 1.0 - vertex.texcoord.y;");
                vertex_code.AppendLine("  vertex.texcoord = normalize(vertex.texcoord);");
            } else {
                vertex_code.AppendLine("  vertex.texcoord = normalize(position);");
            }
        }

        vertex_code.AppendLine("  vertex.frag_position = vec3(vec4(position, 1.0) * model);");
        if(is_sky_box && vertex_array.material.ambient_texture != null)
            vertex_code.AppendLine("  gl_Position = vec4(position, 1) * mat4(mat3(camera)) * projection; gl_Position = gl_Position.xyww;\n");
        else
            vertex_code.AppendLine("  gl_Position = vec4(vertex.world_position, 1) * camera * projection;");

        vertex_code.AppendLine("}");

        var fragment_code = new StringBuilder();

        fragment_code.AppendLine("#version 410\n");

        fragment_code.AppendLine("in VERTEX {");
        fragment_code.AppendLine("  vec3 local_position;");
        fragment_code.AppendLine("  vec3 world_position;");
        if(vertex_array.has_normals)
            fragment_code.AppendLine("  vec3 normal;");
        if(vertex_array.material.ambient_texture != null)
            fragment_code.AppendLine("  vec3 texcoord;");
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
        fragment_code.AppendLine("};\n");
        fragment_code.AppendLine("uniform DirectionalLight[2] directional_light;\n");

        fragment_code.AppendLine("struct Material {");
        if(vertex_array.material.ambient_texture != null)
            fragment_code.AppendLine($"  {vertex_array.material.ambient_texture.glsl_type} ambient_texture;");
        else
            fragment_code.AppendLine("  vec3 ambient_color;");

        fragment_code.AppendLine("  vec3 diffuse;");
        fragment_code.AppendLine("  vec3 specular;");
        fragment_code.AppendLine("  float shininess;");
        fragment_code.AppendLine("};\n");
        fragment_code.AppendLine("uniform Material material;\n");

        fragment_code.AppendLine("out vec4 frag_color;\n");
        fragment_code.AppendLine("""
                                 vec3 calculate_directional_light(DirectionalLight light, vec3 normal, vec3 view_direction, vec3 ambient_color)
                                 {
                                     vec3 light_direction = normalize(-light.direction);
                                     //diffuse shading
                                     float diff = max(dot(normal, -light_direction), 0.0);
                                     //specular shading
                                     vec3 reflect_direction = reflect(-light_direction, normal);
                                     float spec = pow(max(dot(view_direction, reflect_direction), 0.0), material.shininess);
                                     //combine results
                                     vec3 ambient  = light.ambient  * ambient_color;
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

            if(vertex_array.material.ambient_texture != null)
                if(vertex_array.material.ambient_texture is Texture2DBuffer)
                    fragment_code.AppendLine("  vec3 ambient_color = texture(material.ambient_texture, vertex.texcoord.xy).rgb;");
                else
                    fragment_code.AppendLine("  vec3 ambient_color = texture(material.ambient_texture, vertex.texcoord).rgb;");

            else
                fragment_code.AppendLine("  vec3 ambient_color = material.ambient_color;");

            fragment_code.AppendLine("  vec3 light = ambient_color * ambient_light;");

            fragment_code.AppendLine("  //----- Directional lighting -----");
            fragment_code.AppendLine("  light += calculate_directional_light(directional_light[0], normal, view_direction, ambient_color);");
            fragment_code.AppendLine("  frag_color = vec4(light, 1);");
        } else {
            if(vertex_array.material.ambient_texture != null)
                fragment_code.AppendLine("  frag_color = texture(material.ambient_texture, vertex.texcoord);\n");
            else
                fragment_code.AppendLine("  frag_color = vec4(material.ambient_color, 1);");
        }

        fragment_code.AppendLine("}");

        if (false && vertex_array.material.ambient_texture != null) {
            Console.WriteLine($"AutoShader:\n{vertex_code}\n");
            Console.WriteLine(fragment_code);
        }

        save_to_file(name, vertex_code.ToString(), fragment_code.ToString());

        if(tesselate)
            shader.compile_from_text(
                                     vertex_code.ToString(),
                                     fragment_code.ToString(),
                                     tess_control_program: File.ReadAllText(AssetManager.asset_path<Shader>("tessctrl.glsl")),
                                     tess_eval_program: File.ReadAllText(AssetManager.asset_path<Shader>("tesseval.glsl"))
                                     );
        else
            shader.compile_from_text(vertex_code.ToString(), fragment_code.ToString());

        return shader;
    }

    private static void save_to_file(string name, string vertex_code, string fragment_code) {
        string vertex_file = AssetManager.asset_path<Shader>($"generated/{name}.vert.glsl");
        string fragment_file  = AssetManager.asset_path<Shader>($"generated/{name}.frag.glsl");

        File.WriteAllText(vertex_file, vertex_code);
        File.WriteAllText(fragment_file, fragment_code);
    }
}