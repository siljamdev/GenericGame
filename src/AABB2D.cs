using System;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using AshLib;

class AABB2D{
	static Shader boxShader;
	static Mesh boxMesh;
	
	static Matrix4 view;
	
	public static void initialize(){
		boxShader = Shader.fromAssembly("shaders.AABB");
		boxShader.setVector3("color", Color3.Red);
		
		float[] vertices = {
			0.5f, -0.5f,
			-0.5f, -0.5f,
			//0.5f, 0.5f,
			-0.5f, 0.5f,
			0.5f, 0.5f,
			0.5f, -0.5f,
			//-0.5f, -0.5f,
		};
		
		boxMesh = new Mesh("2", vertices, PrimitiveType.LineStrip, "box");
	}
	
	public static void setProjection(Matrix4 m){
		boxShader.setMatrix4("projection", m);
	}
	
	public static void setView(object s, EventArgs a){
		view = ((Camera) s).view;
	}
	
	public double up{get; private set;}
	public double down{get; private set;}
	public double left{get; private set;}
	public double right{get; private set;}
	
	public AABB2D(Vector2d a, Vector2d b){
		if(a.X > b.X){
			left = b.X;
			right = a.X;
		}else{
			left = a.X;
			right = b.X;
		}
		
		if(a.Y > b.Y){
			down = b.Y;
			up = a.Y;
		}else{
			down = a.Y;
			up = b.Y;
		}
	}
	
	public AABB2D(double u, double d, double l, double r){
		up = u;
		down = d;
		left = l;
		right = r;
	}
	
	public void expand(double r){
		up += r;
		down -= r;
		left -= r;
		right += r;
	}
	
	public static bool contained(Vector2d a, AABB2D b){ //a is contained in b
		return b.left < a.X && b.right > a.X && b.down < a.Y && b.up > a.Y;
	}
	
	public static bool contained(AABB2D a, AABB2D b){ //a is contained in b
		return b.left < a.left && b.right > a.right && b.down < a.down && b.up > a.up;
	}
	
	public static bool collide(AABB2D a, AABB2D b){
		return a.left <= b.right && a.right >= b.left && a.down <= b.up && a.up >= b.down;
	}
	
	public static bool collide(AABB2D a, Vector2d b){
		return a.left <= b.X && a.right >= b.X && a.down <= b.Y && a.up >= b.Y;
	}
	
	public static bool collide(Vector2d a, AABB2D b){
		return collide(b, a);
	}
	
	public static AABB2D union(params AABB2D[] boxes){
		if(boxes.Length == 0){
			return null;
		}
		double newLeft = boxes[0].left;
		double newRight = boxes[0].right;
		double newDown = boxes[0].down;
		double newUp = boxes[0].up;
		
		foreach (AABB2D other in boxes){
			newLeft = Math.Min(newLeft, other.left);
			newRight = Math.Max(newRight, other.right);
			newDown = Math.Min(newDown, other.down);
			newUp = Math.Max(newUp, other.up);
		}
		
		return new AABB2D(newUp, newDown, newLeft, newRight);
	}
	
	public bool collide(AABB2D b){
		return collide(this, b);
	}
	
	public bool collide(Vector2d b){
		return collide(this, b);
	}
	
	//Collision, thought it looked cool
	public static bool operator %(AABB2D a, AABB2D b){
		return collide(a, b);
	}
	
	public static bool operator %(AABB2D a, Vector2d b){
		return collide(a, b);
	}
	
	public static bool operator %(Vector2d a, AABB2D b){
		return collide(a, b);
	}
	
	public static AABB2D operator +(AABB2D a, AABB2D b){
		return union(a, b);
	}
	
	public override string ToString(){
		return "AABB2D(Left: " + left + ", Right: " + right + ", Down: " + down + ", Up: " + up + ")";
	}
	
	public void drawWorld(){
		double w = right - left;
		double h = up - down;
		
		Matrix4 model = Matrix4.CreateScale((Vector3) new Vector3d(w, h, 0d)) * Matrix4.CreateTranslation((Vector3) new Vector3d(left + w / 2d, down + h / 2d, 0d));
		
		boxShader.use();
		boxShader.setMatrix4("view", view);
		boxShader.setMatrix4("model", model);
		
		boxMesh.draw();
	}
	
	public void drawAbs(){
		double w = right - left;
		double h = up - down;
		
		Matrix4 model = Matrix4.CreateScale((Vector3) new Vector3d(w, h, 0d)) * Matrix4.CreateTranslation((Vector3) new Vector3d(left + w / 2d, down + h / 2d, 0d));
		
		boxShader.use();
		boxShader.setMatrix4("view", Matrix4.Identity);
		boxShader.setMatrix4("model", model);
		
		boxMesh.draw();
	}
}