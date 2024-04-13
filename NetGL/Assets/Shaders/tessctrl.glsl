#version 410

layout(vertices = 3) out;

in VERTEX {
    vec4 position;
} vertex_in[];

out VERTEX {
    vec4 position;
} vertex_out[];

void main() {
    if (gl_InvocationID == 0){
        gl_TessLevelInner[0] = 3.0;
        gl_TessLevelOuter[0] = 3.0;
        gl_TessLevelOuter[1] = 3.0;
        gl_TessLevelOuter[2] = 3.0;
    }

    vertex_out[gl_InvocationID].position = vertex_in[gl_InvocationID].position;
}