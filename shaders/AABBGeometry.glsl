#version 330 core

layout (points) in;
layout (line_strip, max_vertices = 5) out;

in vec2 size[];

uniform mat4 projection;

//turns out traingle strips are special or smth
void main()
{	
	vec4 p = vec4(-size[0].x, size[0].y, 0.0, 0.0);
	gl_Position = projection * (gl_in[0].gl_Position + p);
	EmitVertex();
	
	p = vec4(-size[0].x, -size[0].y, 0.0, 0.0);
	gl_Position = projection * (gl_in[0].gl_Position + p);
	EmitVertex();
	
	p = vec4(size[0].x, -size[0].y, 0.0, 0.0);
	gl_Position = projection * (gl_in[0].gl_Position + p);
	EmitVertex();
	
	
	p = vec4(size[0].x, size[0].y, 0.0, 0.0);
	gl_Position = projection * (gl_in[0].gl_Position + p);
	EmitVertex();
	
	//Repeat last vertex so loop is closed
	p = vec4(-size[0].x, size[0].y, 0.0, 0.0);
	gl_Position = projection * (gl_in[0].gl_Position + p);
	EmitVertex();
	
	EndPrimitive();
}