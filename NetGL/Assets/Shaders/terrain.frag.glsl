#version 410 core

in vec3 vs_color;
in vec3 vs_normal;

out vec4 frag_color;

void main() {
    frag_color = vec4(vs_color, 1) * max(dot(normalize(vs_normal), vec3(0, 0, 1)), 0.85);
}