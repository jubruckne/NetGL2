#version 410 core

layout (vertices=4) out;

void main(){
    // pass attributes through
    gl_out[gl_InvocationID].gl_Position = gl_in[gl_InvocationID].gl_Position;

    if (gl_InvocationID == 0){
        gl_TessLevelOuter[0] = 4;
        gl_TessLevelOuter[1] = 4;
        gl_TessLevelOuter[2] = 4;
        gl_TessLevelOuter[3] = 4;

        gl_TessLevelInner[0] = 4;
        gl_TessLevelInner[1] = 4;
    }
}