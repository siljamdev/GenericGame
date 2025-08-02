#version 330 core

layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTex;

out vec2 TexCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
	TexCoord = vec2(aPos.x / 2.0 + 0.5, 1.0 - (aPos.y / 2.0 + 0.5));
	gl_Position = projection * view * model * vec4(aPos, 0.0, 1.0); //The position
}