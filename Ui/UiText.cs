using System;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

class UiText : UiElement{
	public string text{get; private set;}
	
	Color3 color;
	
	public UiText(Placement p, float x, float y, string t, Color3 c) : base(p, x, y){
		text = t;
		
		color = c;
	}
	
	public UiText(Placement p, float x, float y, string t) : this(p, x, y, t, Renderer.textColor){
		
	}
	
	public void setText(string t){
		text = t;
		needGen = true;
	}
	
	public override void draw(Renderer ren, Vector2d m){		
		ren.fr.drawText(text, pos, Renderer.textSize, color);
	}
	
	public override void drawHover(Renderer ren, Vector2d m){
		
	}
	
	protected override Vector2 updatePos(Renderer ren){
		Vector2 size = new Vector2(text.Length * Renderer.textSize.X, Renderer.textSize.Y);
		
		return base.getPos(ren, size);
	}
	
	protected override AABB updateBox(Renderer ren){
		return null;
	}
}