using System;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

class UiImage : UiElement{
	public string textureName;
	
	Vector2 size;
	
	public Color3 color;
	
	public string? description;
	
	public UiImage(Placement p, float x, float y, float xs, float ys, string tn, Color3 c) : base(p, x, y){
		textureName = tn;
		
		size = new Vector2(xs, ys);
		
		color = c;
	}
	
	public UiImage setDescription(string d){
		description = d;
		hasHover = true;
		return this;
	}
	
	public override void draw(Renderer ren, Vector2d m){
		ren.drawTexture(textureName, pos, size, color);
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