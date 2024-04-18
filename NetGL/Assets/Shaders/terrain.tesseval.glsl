#version 410 core

layout (quads, equal_spacing, ccw) in;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

in vec4 tcPos[gl_MaxPatchVertices];

vec4 interpolate(vec4 v1, vec4 v2, vec4 v3, vec4 v4)
{
    vec4 aX = mix(v1, v2, gl_TessCoord.x);
    vec4 bX = mix(v4, v3, gl_TessCoord.x);
    return mix(aX, bX, gl_TessCoord.y);
}

void main()
{
    vec4 pos = interpolate(tcPos[0], tcPos[1], tcPos[2], tcPos[3]);
    gl_Position = pos;
}