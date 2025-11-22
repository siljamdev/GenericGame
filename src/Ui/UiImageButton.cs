using System;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

class UiImageButton : UiClickable{
	public string textureName;
	
	Vector2 size;
	
	public Color3 color;
	public Color3 hoverColor;
	
	public string? description;
	
	public UiImageButton(Placement p, float x, float y, float xs, float ys, string tn, Color3 c, Color3 hc) : base(p, x, y){
		textureName = tn;
		
		size = new Vector2(xs, ys);
		
		color = c;
		hoverColor = hc;
	}
	
	public UiImageButton(Placement p, float x, float y, float xs, float ys, string tn, Color3 c) : this(p, x, y, xs, ys, tn, c, Renderer.selectedTextColor){
		
	}
	
	public UiImageButton setDescription(string d){
		description = d;
		hasHover = true;
		return this;
	}
	
	public override void draw(Renderer ren, Vector2d m){
		if(box != null && box % m){
			ren.drawTexture(textureName, pos, size, hoverColor);
		}else{
			ren.drawTexture(textureName, pos, size, color);
		}
	}
	
	public override void drawHover(Renderer ren, Vector2d mousePos){
		drawUsualDescription(ren, mousePos, description);
	}
	
	protected override Vector2 updatePos(Renderer ren){
		return base.getPos(ren, size);
	}
	
	protected override AABB2D updateBox(Renderer ren){		
		return new AABB2D(pos.Y, pos.Y - size.Y, pos.X, pos.X + size.X);
	}
}