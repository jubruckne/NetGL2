shader "Basic Diffuse";

//external files to include
using "lighting.shader";
using "noise.shader";

vertex {
    vec3 position;
    vec3 normal;
}

uniform {
    mat4 model;
    mat4 view;
    mat4 projection;
    sampler2D map;
}

void vertex_stage(out vec3 frag_position, out vec3 frag_normal, out vec2 frag_uv) {

}