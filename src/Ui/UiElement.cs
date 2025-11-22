using System;
using OpenTK;
using OpenTK.Mathematics;

abstract class UiElement{
	public AABB2D box{get; private set;}
	
	public Placement placement;
	public Vector2 offset{get; private set;}
	
	//To not have to update it always
	protected Vector2 pos{get; private set;}
	
	public bool needGen{get; protected set;}
	
	//Shows up
	public bool active;
	
	public bool hasHover{get; protected set;}
	
	public UiElement(Placement p, Vector2 o){
		active = true;
		
		placement = p;
		offset = o;
	}
	
	public UiElement(Placement p, float x, float y) : this(p, new Vector2(x, y)){
		
	}
	
	public void update(Renderer ren){
		pos = updatePos(ren);
		box = updateBox(ren);
		
		needGen = false;
	}
	
	protected Vector2 getPos(Renderer ren, Vector2 siz){
		Vector2 dim = new Vector2(ren.width / 2f, ren.height / 2f);
		
		switch(placement){
			case Placement.TopLeft: //-1, 1
				return new Vector2(-dim.X, dim.Y) + new Vector2(offset.X, -offset.Y);
			
			case Placement.TopRight: //1, 1
				return new Vector2(dim.X, dim.Y) + new Vector2(-offset.X, -offset.Y) + new Vector2(-siz.X, 0);
			
			case Placement.BottomLeft: //-1, -1
				return new Vector2(-dim.X, -dim.Y) + new Vector2(offset.X, offset.Y) + new Vector2(0, siz.Y);
			
			case Placement.BottomRight: //1, -1
				return new Vector2(dim.X, -dim.Y) + new Vector2(-offset.X, offset.Y) + new Vector2(-siz.X, siz.Y);
			
			case Placement.TopCenter: //0, 1
				return new Vector2(0, dim.Y) + new Vector2(offset.X, -offset.Y) + new Vector2(-siz.X / 2f, 0);
			
			case Placement.BottomCenter: //0, -1
				return new Vector2(0, -dim.Y) + new Vector2(offset.X, offset.Y) + new Vector2(-siz.X / 2f, siz.Y);
			
			case Placement.CenterLeft: //-1, 0
				return new Vector2(-dim.X, 0) + new Vector2(offset.X, -offset.Y) + new Vector2(0, siz.Y / 2f);
			
			case Placement.CenterRight: //1, 0
				return new Vector2(dim.X, 0) + new Vector2(-offset.X, -offset.Y) + new Vector2(-siz.X, siz.Y / 2f);
			
			default:
			case Placement.Center: //0, 0
				return new Vector2(0, 0) + new Vector2(offset.X, -offset.Y) + new Vector2(-siz.X / 2, siz.Y / 2f);
		}
	}
	
	abstract public void draw(Renderer ren, Vector2d mousePos);
	
	abstract public void drawHover(Renderer ren, Vector2d mousePos);
	
	abstract protected AABB2D updateBox(Renderer ren);
	
	abstract protected Vector2 updatePos(Renderer ren);
	
	protected void drawUsualDescription(Renderer ren, Vector2d mousePos, string description){
		Vector2 mouse = (Vector2) mousePos;
		
		Vector2 size = new Vector2(ren.fr.getXsize(description, Renderer.textSize) + 10f, Renderer.textSize.Y + 10f);
		
		if(mouse.X + size.X <= ren.width / 2f){
			ren.drawRect(mouse.X, mouse.Y + Renderer.textSize.Y + 10f, size.X, size.Y, Renderer.black, 0.5f);
			ren.fr.drawText(description, mouse.X + 5f, mouse.Y + Renderer.textSize.Y + 5f, Renderer.textSize, Renderer.textColor);
		}else{
			if((mouse.X + size.X) - (ren.width / 2f) <= (-ren.width / 2f) - (mouse.X - size.X)){
				ren.drawRect(mouse.X, mouse.Y + Renderer.textSize.Y + 10f, size.X, size.Y, Renderer.black, 0.5f);
				ren.fr.drawText(description, mouse.X + 5f, mouse.Y + Renderer.textSize.Y + 5f, Renderer.textSize, Renderer.textColor);
			}else{
				ren.drawRect(mouse.X - size.X, mouse.Y + Renderer.textSize.Y + 10f, size.X, size.Y, Renderer.black, 0.5f);
				ren.fr.drawText(description, mouse.X - size.X + 5f, mouse.Y + Renderer.textSize.Y + 5f, Renderer.textSize, Renderer.textColor);
			}
		}
	}
}

enum Placement{
	Center, TopLeft, TopRight, BottomLeft, BottomRight, CenterLeft, CenterRight, TopCenter, BottomCenter
}