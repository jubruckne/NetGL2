#version 410

in VERTEX {
    vec4 position;
} vertex;

uniform float game_time;
uniform vec3 camera_position;
uniform vec3 ambient_light;

struct DirectionalLight {
    vec3 direction;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

uniform DirectionalLight[2] directional_light;

struct Material {
    sampler2D ambient_texture;
    vec3 diffuse;
    vec3 specular;
    float shininess;
};

uniform Material material;

out vec4 frag_color;

void main() {
    frag_color = vec4(material.diffuse, 1);
}