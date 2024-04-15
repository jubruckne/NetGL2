#version 410

layout (location = 0) in vec3 position;

uniform mat4 projection;
uniform mat4 camera;
uniform mat4 model;

const float terrain_width = 4096;
const float terrain_depth = 4096;

void main() {
  vec3 pos = position;
  float u = clamp((pos.x + 2048) / terrain_width, 0.0, 1.0);
  float v = clamp((pos.z + 2048) / terrain_depth, 0.0, 1.0);

  pos.y = 0; //texture(terrain.heightmap, vec2(u, v)).r * 0.01;

  gl_Position = vec4(pos, 1.0) * model * camera * projection;
}