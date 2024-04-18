#version 410 core

layout (location = 0) in ivec2 position;
layout (location = 1) in vec2 offset;
layout (location = 2) in float size;

uniform mat4 projection;
uniform mat4 camera;
uniform mat4 model;

uniform sampler2D heightmap;

void main()
{
  vec4 pos = vec4(offset.x + (position.x * size), 0, offset.y + (position.y * size), 1.0);
  pos.y = texture(heightmap, pos.xz).r;

  gl_Position = pos * model * camera * projection;
}