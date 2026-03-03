using System;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

class UiText : UiElement{
	public string text{get; private set;}
	
	public Color3 color;
	
	public override bool doScissor => false; //TT fonts might extend more than they say they do
	
	public UiText(Placement p, float x, float y, string t) : base(p, x, y){
		text = t;
		color = Renderer.textColor;
	}
	
	public UiText setText(string t){
		text = t;
		needsToUpdate = true;
		return this;
	}
	
	public UiText setColor(Color3 c){
		color = c;
		return this;
	}
	
	public override void draw(Renderer ren, Vector2d mousePos){
		ren.fr.drawText(text, pos, Renderer.textSize, color);
	}
	
	protected override Vector2 updateSize(Renderer ren){
		return new Vector2(ren.fr.getXsize(text, Renderer.textSize), Renderer.textSize.Y);
	}
}