#version 330 core
out vec4 FragColor;

in vec2 TexCoord;

uniform sampler2D fontTexture;
uniform vec4 col;

void main()
{	
	float c = texture(fontTexture, TexCoord).x;
	
	vec4 rgba = col * c;
	FragColor = rgba;
} 