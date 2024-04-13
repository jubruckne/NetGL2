#version 410

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;

uniform mat4 projection;
uniform mat4 camera;
uniform mat4 model;
uniform float game_time;

struct Material {
  sampler2D ambient_texture;
  vec3 diffuse;
  vec3 specular;
  float shininess;
};

uniform Material material;

out VERTEX {
  vec4 position;
} vertex;

const float terrain_width = 4096;
const float terrain_depth = 4096;

void main() {
  vec3 pos = position;
  float u = clamp((pos.x + 2048) / terrain_width, 0.0, 1.0);
  float v = clamp((pos.z + 2048) / terrain_depth, 0.0, 1.0);

  float height = texture(material.ambient_texture, vec2(u, v)).r * 0.01;

  pos.y = height;

  vertex.position = vec4(pos, 1) * model * camera * projection;
  gl_Position = vertex.position;
}