#version 330 core

layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aSize;

out vec2 size;

uniform mat4 view;
uniform float zoom;

void main()
{
	gl_Position = view * vec4(aPos, 0.0, 1.0); //The position
	
	size = zoom * aSize;
}