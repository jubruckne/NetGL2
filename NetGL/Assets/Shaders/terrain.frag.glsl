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

struct Terrain {
    sampler2D heightmap;
    vec4 tile_color;
};

uniform Terrain terrain;

out vec4 frag_color;

void main() {
    frag_color = terrain.tile_color;
}