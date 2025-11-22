#version 330 core
layout (location = 0) in vec2 aPos;

out vec2 TexCoord; 

uniform vec2 pos[256];
uniform vec2 size[256];
uniform vec2 uv0[256];
uniform vec2 uv1[256];

uniform mat4 projection;

void main()
{	
	TexCoord = mix(uv0[gl_InstanceID], uv1[gl_InstanceID], vec2(aPos.x, -aPos.y));
	
	gl_Position = projection * vec4((aPos * size[gl_InstanceID]) + pos[gl_InstanceID], -1.0, 1.0);
}