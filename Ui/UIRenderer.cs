using System;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class UIRenderer{
	Dictionary<string, Texture2D> book;
	
	Mesh uiMesh;
	
	Shader uiShader;
	Shader rectShader;
	
	public UIRenderer(Mesh m){
		book = new Dictionary<string, Texture2D>();
		uiShader = Shader.generateFromAssembly("ui");
		rectShader = Shader.generateFromAssembly("rect");
		
		uiMesh = m;
	}
	
	public void setProjection(Matrix4 m){
		uiShader.setMatrix4("projection", m);
		rectShader.setMatrix4("projection", m);
	}
	
	public void addTexture(string n, Texture2D t){
		book.Add(n, t);
	}
	
	public void drawRect(Vector2 pos, Vector2 sca, Color3 col, float alpha = 1f){
		Matrix4 model = Matrix4.CreateScale(new Vector3(sca.X, sca.Y, 0f)) * Matrix4.CreateTranslation(new Vector3(pos.X, pos.Y, 0f));
		
		rectShader.use();
		rectShader.setMatrix4("model", model);
		rectShader.setVector4("col", col, alpha);
		
		uiMesh.draw();
	}
	
	public void drawRect(float xp, float xy, float sx, float sy, Color3 c, float a = 1f){
		drawRect(new Vector2(xp, xy), new Vector2(sx, sy), c, a);
	}
	
	public void draw(string n, Vector2 pos, Vector2 sca, Color3 col, float alpha = 1f){
		if(!book.ContainsKey(n)){
			return;
		}
		
		Matrix4 model = Matrix4.CreateScale(new Vector3(sca.X, sca.Y, 0f)) * Matrix4.CreateTranslation(new Vector3(pos.X, pos.Y, 0f));
		
		uiShader.use();
		uiShader.setMatrix4("model", model);
		uiShader.setVector4("col", col, alpha);
		
		book[n].bind();
		
		uiMesh.draw();
	}
	
	public void draw(string n, float xpos, float ypos, float xsca, float ysca, Color3 col, float alpha = 1f){
		draw(n, new Vector2(xpos, ypos), new Vector2(xsca, ysca), col, alpha);
	}
	
	public void draw(string n, float xpos, float ypos, float sca, Color3 col, float alpha = 1f){
		draw(n, new Vector2(xpos, ypos), new Vector2(sca, sca), col, alpha);
	}
}