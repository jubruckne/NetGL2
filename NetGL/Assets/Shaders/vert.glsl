#version 410

in vec3 a_position;
in vec3 a_normal;

uniform mat4 projection;
uniform mat4 camera;
uniform mat4 model;
uniform float game_time;

out VS_OUT {
    vec4 v_color;
    vec4 v_normal;
} gs_out;

void main() {
    gs_out.v_color = vec4(a_position, 0) + vec4(0.5, 0.5, 0.5, 1);

    gl_Position = vec4(a_position, 1.0) * model * camera * projection;
    gs_out.v_normal = vec4(a_normal, 1.0) * mat4(mat3(model)) * mat4(mat3(camera)) * projection;

}