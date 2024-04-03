#version 410

layout(vertices = 3) out;

in VERTEX {
    vec3 local_position;
    vec3 world_position;
    vec3 normal;
    vec3 frag_position;
} vertex_in[];

out VERTEX {
    vec3 local_position;
    vec3 world_position;
    vec3 normal;
    vec3 frag_position;
} vertex_out[];

void main() {
    if (gl_InvocationID == 0){
        gl_TessLevelInner[0] = 3.0;
        gl_TessLevelOuter[0] = 3.0;
        gl_TessLevelOuter[1] = 3.0;
        gl_TessLevelOuter[2] = 3.0;
    }

    vertex_out[gl_InvocationID].local_position = vertex_in[gl_InvocationID].local_position;
    vertex_out[gl_InvocationID].world_position = vertex_in[gl_InvocationID].world_position;
    vertex_out[gl_InvocationID].normal = vertex_in[gl_InvocationID].normal;
    vertex_out[gl_InvocationID].frag_position = vertex_in[gl_InvocationID].frag_position;
}