#version 330 core

layout(triangles) in;
layout(points, max_vertices = 12) out;

in VS_OUT {
    vec4 v_color;
    vec4 v_normal;
} gs_in[];

out vec4 v_color;
out vec4 v_normal;

const float MAGNITUDE = 0.1;

uniform mat4 projection;
uniform mat4 camera;
uniform mat4 model;

void main() {
    gl_PointSize = 20;

    v_color = vec4(1, 1, 1, 1);

    gl_Position = gl_in[0].gl_Position;
    v_normal = gs_in[0].v_normal;
    EmitVertex();

    gl_Position = gl_in[1].gl_Position;
    v_normal = gs_in[1].v_normal;
    EmitVertex();

    gl_Position = gl_in[2].gl_Position;
    v_normal = gs_in[2].v_normal;
    EmitVertex();

    gl_PointSize = 10;

    vec4 center = (gl_in[0].gl_Position + gl_in[1].gl_Position + gl_in[2].gl_Position) / 3.0;
    v_color = vec4(0, 0, 1, 1);
    gl_Position = center;
    EmitVertex();

    v_color = vec4(1, 0, 0, 1);
    gl_Position = center + (gs_in[0].v_normal + gs_in[1].v_normal + gs_in[2].v_normal) * 0.5;
    EmitVertex();


}