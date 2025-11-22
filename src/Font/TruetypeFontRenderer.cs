using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using StbImageSharp;
using AshLib;

class TruetypeFontRenderer : FontRenderer{	
	const int maxChars = 256;
	
	Dictionary<char, GlyphInfo> map;
	
	float increasedSize;
	
	public TruetypeFontRenderer(Mesh m, TTFont f, float incSiz = 0f)
		:base(m, f.atlas){
		map = f.glyphs;
		increasedSize = incSiz;
		
		fontShader = Shader.fromAssembly("shaders.truetypeFont");
	}
	
	public TextInstance[] textToVert(string text, Vector2 scale){
		text = text.Length <= maxChars ? text : text.Substring(0, maxChars);
		
		List<TextInstance> instances = new List<TextInstance>(text.Length);
	
		float penX = 0;
		float penY = 0;
		
		Vector2 actS = scale + new Vector2(increasedSize);
	
		foreach (char c in text){
			if(!map.TryGetValue(c, out var g)){
				continue; // skip unknown glyphs
			}
	
			// Compute glyph quad position
			float gx = penX + g.bearing.X * actS.X;
			float gy = penY - g.bearing.Y * actS.Y - scale.Y;
	
			TextInstance ti = new TextInstance(
				new Vector2(gx, gy),
				new Vector2(g.size.X * actS.X, g.size.Y * actS.Y),
				g.uv0,
				g.uv1
			);
	
			instances.Add(ti);
	
			// Advance pen
			penX += g.advance * actS.X;
		}
	
		return instances.ToArray();
	}
	
	public override float getXsize(string text, Vector2 sca){
		TextInstance[] l = textToVert(text, sca);
		
		return l.Length > 0 ? l[l.Length - 1].pos.X + l[l.Length - 1].size.X : 0f;
	}
	
	public override void drawText(string text, Vector2 pos, Vector2 sca, Placement p, Color3 col, float alpha = 1f){
		TextInstance[] l = textToVert(text, sca);
		
		float totalSize = l.Length > 0 ? l[l.Length - 1].pos.X + l[l.Length - 1].size.X : 0f;
		
		pos = getPos(pos, sca.Y, totalSize, p);
		
		//Flatten
		Vector2[] posArray	= new Vector2[l.Length];
		Vector2[] sizeArray = new Vector2[l.Length];
		Vector2[] uv0Array	= new Vector2[l.Length];
		Vector2[] uv1Array	= new Vector2[l.Length];
		
		for(int i = 0; i < l.Length; i++){
			posArray[i] = l[i].pos + pos;
			sizeArray[i] = l[i].size;
			uv0Array[i] = l[i].uv0;
			uv1Array[i] = l[i].uv1;
		}
		
		fontShader.use();
		fontShader.setVector2Array("pos[0]", posArray); //Set the position
		fontShader.setVector2Array("size[0]", sizeArray); //Set the size
		fontShader.setVector2Array("uv0[0]", uv0Array);
		fontShader.setVector2Array("uv1[0]", uv1Array);
		
		fontShader.setVector4("col", col, alpha); //Pass the color
		
		font.bind();
		
		fontMesh.drawInstanced(l.Length);
	}
}

record TextInstance (Vector2 pos, Vector2 size, Vector2 uv0, Vector2 uv1);