#version 410

in vec3 a_position;

uniform mat4 projection;
uniform mat4 camera;
uniform mat4 model;

out vec4 v_color;

void main(void) {
    v_color = vec4(a_position, 0) + vec4(0.5, 0.5, 0.5, 1);

    gl_Position = vec4(a_position, 1.0) * model * camera * projection;
}