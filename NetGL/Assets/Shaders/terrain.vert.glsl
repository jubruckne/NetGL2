#version 410 core

layout (location = 0) in vec2 position;
layout (location = 1) in vec2 offset;
layout (location = 2) in float size;

uniform mat4 projection;
uniform mat4 camera;
uniform mat4 model;

uniform sampler2D heightmap;

out vec3 vs_color;

void main()
{
  vec4 pos = vec4(offset.x + (position.x * size), 0, offset.y + (position.y * size), 1.0);
  //pos.y = texture(heightmap, (vec2(pos.x, pos.z) + vec2(2048, 2048)) / 4096.0).r * 3.5;
  pos.y = gl_InstanceID * -0.0075;

  gl_Position = pos * model * camera * projection;

  vs_color =  vec3(1, 1, 1); //1/ (pos.xzy / 2048.0);
  //vs_color.r = (gl_InstanceID % 255) / 255.0;
}