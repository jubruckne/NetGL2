#version 410

in VERTEX {
    vec3 local_position;
    vec3 world_position;
    vec3 normal;
    vec3 frag_position;
} vertex;

uniform float game_time;

// uniform vec3 camera_position;

layout(std140, row_major) uniform Camera {
    mat4  projection;
    mat4  view;
    vec3  position;
    float pad;

} camera;

uniform vec3 ambient_light;

struct DirectionalLight {
    vec3 direction;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

uniform DirectionalLight[2] directional_light;

struct Material {
    vec3 ambient_color;
    vec3 diffuse;
    vec3 specular;
    float shininess;
};

uniform Material material;

out vec4 frag_color;

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

void main() {
    vec3 normal = normalize(vertex.normal);
    vec3 view_direction = normalize(camera.position - vertex.frag_position);
    //------- Ambient lighting -------
    vec3 ambient_color = material.ambient_color;
    vec3 light = ambient_color * ambient_light;
    //----- Directional lighting -----
    light += calculate_directional_light(directional_light[0], normal, view_direction, ambient_color);
    frag_color = vec4(light, 1);
}