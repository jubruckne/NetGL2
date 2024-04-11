#version 410

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;

uniform mat4 projection;
uniform mat4 camera;
uniform mat4 model;
uniform float game_time;

out VERTEX {
  vec3 local_position;
  vec3 world_position;
  vec3 normal;
  vec3 frag_position;
} vertex;

void main() {
  vertex.local_position = position;
  vertex.world_position = (vec4(position, 1) * model).xyz;
  vertex.normal = normal * mat3(transpose(inverse(model)));
  vertex.frag_position = vec3(vec4(position, 1.0) * model);
  gl_Position = vec4(vertex.world_position, 1) * camera * projection;
}