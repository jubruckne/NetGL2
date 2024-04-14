#version 410

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;

uniform mat4 projection;
uniform mat4 camera;
uniform mat4 model;
uniform float game_time;

struct Terrain {
  sampler2D heightmap;
  vec4 tile_color;
};

uniform Terrain terrain;

out VERTEX {
  vec4 position;
} vertex;

const float terrain_width = 4096;
const float terrain_depth = 4096;

void main() {
  vertex.position = vec4(position, 1.0);
  float u = clamp((vertex.position.x + 2048) / terrain_width, 0.0, 1.0);
  float v = clamp((vertex.position.z + 2048) / terrain_depth, 0.0, 1.0);

  vertex.position.y = texture(terrain.heightmap, vec2(u, v)).r * 0.01;

  gl_Position = vertex.position * model * camera * projection;
}