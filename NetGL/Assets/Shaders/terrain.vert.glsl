#version 410 core

layout (location = 0) in vec2 position;
layout (location = 1) in vec2 offset;
layout (location = 2) in int size;
layout (location = 3) in vec4 color;

uniform mat4 projection;
uniform mat4 camera;
uniform mat4 model;

uniform sampler2D heightmap;

out vec3 vs_normal;
out vec3 vs_color;

void main() {
  vec3 pos = vec3(offset.x + (position.x * size), 0, offset.y + (position.y * size));

  vec2 tex_coord = vec2(pos.x, pos.z) / textureSize(heightmap, 0).xy + 0.5;

  pos.y = texture(heightmap, tex_coord).r * 1.0;

  // calculate normal from surrounding pixels
  float north = textureOffset(heightmap, tex_coord, ivec2(0, 1)).r;
  float south = textureOffset(heightmap, tex_coord, ivec2(0, -1)).r;
  float east = textureOffset(heightmap, tex_coord, ivec2(1, 0)).r;
  float west = textureOffset(heightmap, tex_coord, ivec2(-1, 0)).r;

  // Calculate normal
  vs_normal = normalize(vec3(east - west, 2.0, north - south));

  gl_Position = vec4(pos, 1.0) * model * camera * projection;

  vs_color = color.rgb;
}