#version 330

out vec4 f_color;

void main() {

    f_color = gl_FrontFacing ? vec4(1, 1, 0, 1) : vec4(0.25, 0.25, 0.25, 1);
}