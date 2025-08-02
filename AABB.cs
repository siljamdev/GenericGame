using System;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

class AABB{
	public double up{get; private set;}
	public double down{get; private set;}
	public double left{get; private set;}
	public double right{get; private set;}
	
	public const int drawDataSize = 4;
	
	public AABB(Vector2d a, Vector2d b){
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
	
	public AABB(double u, double d, double l, double r){
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
	
	public static bool contained(Vector2d a, AABB b){ //a is contained in b
		return b.left < a.X && b.right > a.X && b.down < a.Y && b.up > a.Y;
	}
	
	public static bool contained(AABB a, AABB b){ //a is contained in b
		return b.left < a.left && b.right > a.right && b.down < a.down && b.up > a.up;
	}
	
	public static bool collide(AABB a, AABB b){
		return a.left <= b.right && a.right >= b.left && a.down <= b.up && a.up >= b.down;
	}
	
	public static bool collide(AABB a, Vector2d b){
		return a.left <= b.X && a.right >= b.X && a.down <= b.Y && a.up >= b.Y;
	}
	
	public static bool collide(Vector2d a, AABB b){
		return collide(b, a);
	}
	
	public static AABB union(params AABB[] boxes){
		if(boxes.Length == 0){
			return null;
		}
		double newLeft = boxes[0].left;
		double newRight = boxes[0].right;
		double newDown = boxes[0].down;
		double newUp = boxes[0].up;
		
		foreach (AABB other in boxes){
			newLeft = Math.Min(newLeft, other.left);
			newRight = Math.Max(newRight, other.right);
			newDown = Math.Min(newDown, other.down);
			newUp = Math.Max(newUp, other.up);
		}
		
		return new AABB(newUp, newDown, newLeft, newRight);
	}
	
	public bool collide(AABB b){
		return collide(this, b);
	}
	
	public bool collide(Vector2d b){
		return collide(this, b);
	}
	
	//Collision, thought it looked cool
	public static bool operator %(AABB a, AABB b){
		return collide(a, b);
	}
	
	public static bool operator %(AABB a, Vector2d b){
		return collide(a, b);
	}
	
	public static bool operator %(Vector2d a, AABB b){
		return collide(a, b);
	}
	
	public static AABB operator +(AABB a, AABB b){
		return union(a, b);
	}
	
	public override string ToString(){
		return "AABB(Left: " + left + ", Right: " + right + ", Down: " + down + ", Up: " + up + ")";
	}
	
	public float[] getDrawData(){
		double w = (right - left) / 2d;
		double h = (up - down) / 2d;
		double mx = left + w;
		double my = down + h;
		
		float[] f = new float[4];
		f[0] = (float) mx;
		f[1] = (float) my;
		
		f[2] = (float) w;
		f[3] = (float) h;	
		return f;
	}
}