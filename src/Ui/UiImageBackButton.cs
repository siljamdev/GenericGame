using System;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

class UiImageBackButton : UiClickable{
	public string textureName;
	
	Vector2 size;
	
	public Color3 color;
	public Color3 hoverColor;
	
	public Color3 backColor;
	public Color3 hoverBackColor;
	
	public string? description;
	
	public UiImageBackButton(Placement p, float x, float y, float xs, float ys, string tn, Color3 c, Color3 hc, Color3 b, Color3 hb) : base(p, x, y){
		textureName = tn;
		
		size = new Vector2(xs, ys);
		
		color = c;
		hoverColor = hc;
		backColor = b;
		hoverBackColor = hb;
	}
	
	public UiImageBackButton(Placement p, float x, float y, float xs, float ys, string tn, Color3 c, Color3 b) : this(p, x, y, xs, ys, tn, c, Renderer.selectedTextColor, b, new Color3((byte) (b.R * 1.2f), (byte) (b.G * 1.2f), b.B)){
		
	}
	
	public UiImageBackButton setDescription(string d){
		description = d;
		hasHover = true;
		return this;
	}
	
	public override void draw(Renderer ren, Vector2d m){
		if(box != null && box % m){
			ren.drawRect(pos, size, hoverBackColor, 1f);
			ren.drawTexture(textureName, pos + new Vector2(5f, -5f), size - new Vector2(10f), hoverColor);
		}else{
			ren.drawRect(pos, size, backColor, 1f);
			ren.drawTexture(textureName, pos + new Vector2(5f, -5f), size - new Vector2(10f), color);
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