#version 410


flat in int e_instanceID;
in vec3 e_normal;

vec3 getMaterial(int id) {
	if (id == 0) return vec3(1, 0, 0);
	if (id == 1) return vec3(1, 1, 1);
	if (id == 2) return vec3(0.3, 1, 0.3);
	if (id == 3) return vec3(0.3, 0.3, 1);
	if (id == 4) return vec3(0.3, 1, 0);
	if (id == 5) return vec3(1, 1, 0.3);
	if (id == 6) return vec3(1, 0.3, 1);
	return vec3(0.0, 0.0, 0.0);
}

out vec4 color;

void main() 
{
	vec3 c = getMaterial(e_instanceID);
	vec3 n = normalize(e_normal);
	vec3 l = normalize(vec3(1, 1, 0));
	color =  vec4(max(dot(n, l), 0.1) * c, 1.0);
	color =  vec4(n, 1.0);
}