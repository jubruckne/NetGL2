#version 410

in vec4 v_color;
in vec4 v_normal;

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
        int checkerWidth = int(v_color.a * 8); // Width of each checker square, adjust as needed
        int x = int(gl_FragCoord.x) / checkerWidth;
        int y = int(gl_FragCoord.y) / checkerWidth;

        // Calculate checkerboard pattern
        if (v_color.a == 1 || (x + y) % 2 == 0) {
                f_color = v_color;
        } else {
                discard;
        }

        // if(gl_FrontFacing)
    //else
    //    f_color = v_color;

}