#version 330 core

layout(triangles) in;
layout(triangle_strip, max_vertices = 54) out;

in VS_OUT {
    vec4 v_color;
    vec4 v_normal;
} gs_in[];

out vec4 v_color;
out vec4 v_normal;

const float THICKNESS = 0.03;
const float ARROW_LENGTH = 0.05; // Length of the arrow
const float ARROW_WIDTH = 0.12; // Width of the arrow base

const float MAGNITUDE = 0.6;

uniform mat4 projection;
uniform mat4 camera;
uniform mat4 model;

// Function to emit a quad for a line segment
void EmitLine(vec4 start, vec4 end, vec4 color_start, vec4 color_end) {
    vec4 direction = normalize(end - start);
    vec4 perpendicular = normalize(vec4(-direction.y, direction.x, direction.z, 0.0)) * THICKNESS;

    v_color = color_start;
    gl_Position = (start + perpendicular);
    EmitVertex();

    gl_Position = (start - perpendicular);
    EmitVertex();

    v_color = color_end;

    gl_Position = (end + perpendicular);
    EmitVertex();

    gl_Position = (end - perpendicular);
    EmitVertex();

    EndPrimitive();
}

// Function to emit an arrowhead
void EmitArrowhead(vec4 base, vec4 tip, vec4 color) {
    vec4 direction = normalize(tip - base);
    vec4 perpendicular = normalize(vec4(-direction.y, direction.x, direction.z, 0.0)) * ARROW_WIDTH;

    v_color = color;

    // Emit vertices for the arrowhead (triangle)
    gl_Position = (base + perpendicular);
    EmitVertex();

    gl_Position = (base - perpendicular);
    EmitVertex();

    gl_Position = tip;
    EmitVertex();

    EndPrimitive();
}


void main() {
    gl_PointSize = 20;

    v_color = vec4(1, 1, 1, 0.25);

    gl_Position = gl_in[0].gl_Position;
    v_normal = gs_in[0].v_normal;
    EmitVertex();

    gl_Position = gl_in[1].gl_Position;
    v_normal = gs_in[1].v_normal;
    EmitVertex();

    gl_Position = gl_in[2].gl_Position;
    v_normal = gs_in[2].v_normal;
    EmitVertex();
    EndPrimitive();

    vec4 center = (gl_in[0].gl_Position + gl_in[1].gl_Position + gl_in[2].gl_Position) / 3.0;
    vec4 start = center - (gs_in[0].v_normal + gs_in[1].v_normal + gs_in[2].v_normal) * MAGNITUDE * 0.15 * 0.5;
    vec4 end = center + (gs_in[0].v_normal + gs_in[1].v_normal + gs_in[2].v_normal) * MAGNITUDE * 0.85 * 0.5;

    EmitLine(start, end, vec4(1, 1, 1, 1), vec4(1, 0, 0, 1));

    EmitArrowhead(end, end + normalize(end - start) * ARROW_LENGTH * 5.5 , vec4(1, 0, 0, 1));

}