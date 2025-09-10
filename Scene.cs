using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Scene{	
	static Shader sceneShader;
	static Mesh sceneMesh;
	
	public static void initialize(){
		sceneShader = Shader.fromAssembly("shaders.scene");
		
		float[] vertices = {
			1f, -1f,
			1f, 1f,
			-1f, -1f,
			1f, 1f,
			-1f, 1f,
			-1f, -1f,
		};
		
		sceneMesh = new Mesh("2", vertices, PrimitiveType.Triangles, "scene");
	}
	
	public Camera cam{get; private set;}
	
	Texture2D texture;
	
	public Scene(Renderer ren){
		cam = ren.cam;
		
		texture = Texture2D.fromAssembly("res.textures.sce.png", TextureParams.Default);
		
		cam.onViewChange += setView;
		
		//Initial update needed to set it
		setProjection(ren.projection);
		setView(null, EventArgs.Empty);
	}
	
	public void setProjection(Matrix4 m){
		sceneShader.setMatrix4("projection", m);
	}
	
	public void setView(object s, EventArgs a){
		sceneShader.setMatrix4("view", cam.view);
	}
	
	public void draw(Renderer ren){
		Matrix4 model = Matrix4.CreateScale(new Vector3(100f, 100f, 0f)) * Matrix4.CreateTranslation(new Vector3(0f, 0f, 0f));
		
		sceneShader.use();
		sceneShader.setMatrix4("model", model);
		
		texture.bind();
		
		sceneMesh.draw();
	}
}