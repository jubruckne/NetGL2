#version 410

layout(triangles, equal_spacing, cw) in;

// Interpolated attributes from TCS
in VERTEX {
    vec4 position;
} vertex_in[];

// Output to fragment shader
out VERTEX {
    vec4 position;
} vertex_out;

void main() {
    // Interpolate world position and normal for the current vertex
    vec3 position = gl_TessCoord.x * vertex_in[0].position +
    gl_TessCoord.y * vertex_in[1].position +
    gl_TessCoord.z * vertex_in[2].position;

    vertex_out.position = position;
}