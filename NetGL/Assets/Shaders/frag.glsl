#version 410

in vec4 v_color;

/*
struct Material {
    vec4 ambient;
    vec4 diffuse;
    vec4 specular;
    float shininess;
};
uniform Material material;
*/

out vec4 f_color;

void main() {
    if(gl_FrontFacing)
        f_color = v_color;
    else
        f_color = v_color;

}