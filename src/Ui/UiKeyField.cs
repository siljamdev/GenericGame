using System;
using OpenTK;
using OpenTK.Mathematics;
using AshLib;

class UiKeyField : UiSelectable{
	public KeyBind key;
	string question;
	
	public string? description;
	
	public UiKeyField(Placement p, float x, float y, string q, KeyBind k) : base(p, x, y){
		key = k;
		question = q;
	}
	
	public UiKeyField setDescription(string d){
		description = d;
		hasHover = true;
		return this;
	}
	
	public override void draw(Renderer ren, Vector2d m){
		string text = key.key.ToString();
		Vector2 fsize = new Vector2(text.Length * Renderer.textSize.X + 10f, Renderer.textSize.Y + 10f);
		
		if(selected){
			ren.drawRect(pos + new Vector2(question.Length * Renderer.textSize.X + 10f, 0f), fsize, Renderer.fieldSelectedColor, 0.8f);
			ren.fr.drawText(text, pos + new Vector2(question.Length * Renderer.textSize.X + 15f, -5f), Renderer.textSize, Renderer.textColor, 1f);
		}else{
			ren.drawRect(pos + new Vector2(question.Length * Renderer.textSize.X + 10f, 0f), fsize, Renderer.fieldColor, 0.7f);
			ren.fr.drawText(text, pos + new Vector2(question.Length * Renderer.textSize.X + 15f, -5f), Renderer.textSize, Renderer.fieldTextColor, 1f);
		}
		ren.fr.drawText(question, pos + new Vector2(0f, -5f), Renderer.textSize, Renderer.textColor, 1f);
	}
	
	public override void drawHover(Renderer ren, Vector2d mousePos){
		drawUsualDescription(ren, mousePos, description);
	}
	
	protected override Vector2 updatePos(Renderer ren){
		string text = key.key.ToString();
		
		Vector2 size = new Vector2(text.Length * Renderer.textSize.X + 10f + question.Length * Renderer.textSize.X + 10f, Renderer.textSize.Y + 10f);
		
		return base.getPos(ren, size);
	}
	
	protected override AABB2D updateBox(Renderer ren){
		string text = key.key.ToString();
		
		Vector2 fsize = new Vector2(text.Length * Renderer.textSize.X + 10f, Renderer.textSize.Y + 10f);
		
		Vector2 fpos = new Vector2(pos.X + question.Length * Renderer.textSize.X + 10f, pos.Y);
		
		return new AABB2D(fpos.Y, fpos.Y - fsize.Y, fpos.X, fpos.X + fsize.X);
	}
}