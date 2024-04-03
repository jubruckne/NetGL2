#version 410

layout(triangles, equal_spacing, cw) in;

// Interpolated attributes from TCS
in VERTEX {
    vec3 local_position;
    vec3 world_position;
    vec3 normal;
    vec3 frag_position;
} vertex_in[];

// Output to fragment shader
out VERTEX {
    vec3 local_position;
    vec3 world_position;
    vec3 normal;
    vec3 frag_position;
} vertex_out;

uniform mat4 projection;
uniform mat4 camera;

void main() {
    // Interpolate world position and normal for the current vertex
    vec3 world_position = gl_TessCoord.x * vertex_in[0].world_position +
    gl_TessCoord.y * vertex_in[1].world_position +
    gl_TessCoord.z * vertex_in[2].world_position;
    vec3 local_position = gl_TessCoord.x * vertex_in[0].local_position +
    gl_TessCoord.y * vertex_in[1].local_position +
    gl_TessCoord.z * vertex_in[2].local_position;

    vec3 normal = normalize(gl_TessCoord.x * vertex_in[0].normal +
    gl_TessCoord.y * vertex_in[1].normal +
    gl_TessCoord.z * vertex_in[2].normal);

    vec3 frag_position = gl_TessCoord.x * vertex_in[0].frag_position +
    gl_TessCoord.y * vertex_in[1].frag_position +
    gl_TessCoord.z * vertex_in[2].frag_position;

    vertex_out.world_position = world_position;
    vertex_out.local_position = local_position;
    vertex_out.normal = normal;
    vertex_out.frag_position = frag_position;
}