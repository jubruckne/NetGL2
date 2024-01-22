#version 330

in vec3 a_position;

uniform mat4 projection;
uniform mat4 camera;
uniform mat4 model;

void main(void) {
    gl_Position = vec4(a_position, 1.0) * model * camera * projection;
}