#version 410
uniform int instanceSqrt = 5;

flat out int v_instanceID;
out vec2 v_texCoord;

void main() {
	const float size = 0.5;
	const vec2 vertices[4] = vec2[4](vec2(-size, -size),
		vec2( size, -size),
		vec2( size,  size),
		vec2(-size,  size));
	
	float x = gl_InstanceID % instanceSqrt;
	float y = gl_InstanceID / instanceSqrt;
	v_instanceID = gl_InstanceID;
	v_texCoord = (vertices[gl_VertexID] + vec2(1) + vec2(x, y)) / instanceSqrt;

	vec2 pos = vertices[gl_VertexID] + vec2(x, y) - vec2(instanceSqrt / 2);

	gl_Position = vec4(pos.x, 0, pos.y, 1.0);
}