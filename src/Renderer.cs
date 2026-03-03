using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class Renderer{
	#region static
	static readonly Color4 backgroundColor = new Color4(0f, 0f, 0f, 1f);
	static readonly Color4 mainMenuBackgroundColor = new Color4(0.18f, 0.38f, 0.33f, 1f);
	
	//public static readonly Vector2 textSize = new Vector2(15f, 18f);
	public static readonly Vector2 textSize = new Vector2(18f);
	
	public const float separation = 40f;
	public const float fieldSeparation = 30f;
	
	public static readonly Color3 textColor = new Color3("EFEFEF");
	public static readonly Color3 selectedTextColor = new Color3("FFFF8F");
	public static readonly Color3 titleTextColor = new Color3("DFDFFF");
	public static readonly Color3 fieldTextColor = new Color3("D6D6D6");
	public static readonly Color3 redTextColor = new Color3("FF8888");
	
	public static readonly Color3 buttonColor = new Color3("555577");
	public static readonly Color3 greenButtonColor = new Color3("557755");
	public static readonly Color3 redButtonColor = new Color3("775555");
	public static readonly Color3 fieldColor = new Color3("222222");
	public static readonly Color3 fieldSelectedColor = new Color3("505050");
	
	public static readonly Color3 black = Color3.Black;
	#endregion
	
	public int width{get; private set;}
	public int height{get; private set;}
	
	public Camera cam{get; private set;}
	public FontRenderer fr{get; private set;}
	public ParticleRenderer uipr{get; private set;} //UI particle renderer

	public Mesh uiMesh{get; private set;}
	
	public Shader uiShader{get; private set;}
	public Shader rectShader{get; private set;}
	
	public Matrix4 projection{get; private set;}
	
	public UiScreen overlayScreen;
	public UiScreen currentScreen;
	
	//Example
	public Texture2D iconTex;
	
	GenericGame genGame;
	
	Dictionary<string, Texture2D> uiTexturesBook = new();
	
	Stack<UiScreen> screens;
	
	Stopwatch sw;
	string corner;
	Color3 cornerColor;
	
	float ramMB = 0f;
	float ramUpdateCounter = 0f;
	
	bool advancedMode;
	
	public Renderer(GenericGame st){
		genGame = st;
		
		width = st.ClientSize.X;
		height = st.ClientSize.Y;
		
		//Enable transparency (blending)
		GL.Enable(EnableCap.Blend);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		
		screens = new Stack<UiScreen>();
		
		//other utilities
		cam = new Camera(this);
		cam.onViewChange += AABB2D.setView;
		cam.onViewChange += LineStrip.setView;
		
		sw = new Stopwatch();
		
		float[] vertices = { //y is in -1 so starting pos of the text is in the left upper corner
			1f, -1f,
			1f, 0f,
			0f, -1f,
			1f, 0f,
			0f, 0f,
			0f, -1f,
		};
		
		uiMesh = new Mesh("2", vertices, PrimitiveType.Triangles, "ui");
		
		uiShader = Shader.fromAssembly("shaders.ui");
		rectShader = Shader.fromAssembly("shaders.rect");
		
		//Bitmap font
		//fr = new BitmapFontRenderer(uiMesh, Texture2D.fromAssembly("res.textures.font.png", TextureParams.Default), 16, 16);
		
		//TrueType font
		TTFont f = TTFont.fromAssembly("res.AtkinsonHyperlegible.ttf", TextureParams.Smooth, 128, 2048);
		fr = new TruetypeFontRenderer(uiMesh, f, 9);

		uipr = new ParticleRenderer();
		
		//Example texture
		iconTex = Texture2D.fromAssembly("res.icon.png", TextureParams.Default);
		
		//load ui textures
		addTexture("tick", Texture2D.fromAssembly("res.textures.ui.tick.png", TextureParams.Default)); //Used for UiCheck
		addTexture("screenshot", Texture2D.fromAssembly("res.textures.ui.screenshotIcon.png", TextureParams.Default));
		addTexture("next", Texture2D.fromAssembly("res.textures.ui.nextIcon.png", TextureParams.Default));
		addTexture("previous", Texture2D.fromAssembly("res.textures.ui.previousIcon.png", TextureParams.Default));
		addTexture("icon", iconTex); //not repeated object!!
		addTexture("info", Texture2D.fromAssembly("res.textures.ui.infoIcon.png", TextureParams.Default));
		addTexture("help", Texture2D.fromAssembly("res.textures.ui.helpIcon.png", TextureParams.Default));
		addTexture("file", Texture2D.fromAssembly("res.textures.ui.fileIcon.png", TextureParams.Default));
		
		//Initialize overlay screen
		overlayScreen = new UiScreen(
			new UiImage(Placement.TopLeft, 0f, 0f, 30, 30, "icon").setColor(Color3.White)
		);
		
		overlayScreen.updateProj(this);
	}
	
	public void setScreen(UiScreen s){
		if(currentScreen != null){
			currentScreen.close();
			screens.Push(currentScreen);
		}
		
		if(s == null){
			screens.Clear();
		}
		
		currentScreen = s;
		currentScreen?.updateProj(this);
	}
	
	public void closeScreen(){
		if(genGame.sce == null && screens.Count == 0){ //Not close main menu
			return;
		}
		
		if(screens.Count == 0){
			currentScreen?.close();
			currentScreen = null;
			return;
		}
		
		currentScreen?.close();
		currentScreen = screens.Pop();
		currentScreen?.updateProj(this);
	}
	
	public void setCornerInfo(string s, Color3 c){
		corner = s;
		cornerColor = c;
		sw.Restart();
	}
	
	public void setCornerInfo(string s){
		corner = s;
		cornerColor = textColor;
		sw.Restart();
	}
	
	public void updateSize(int w, int h){
		width = w;
		height = h;
		projection = Matrix4.CreateOrthographic((float) width, (float) height, -1.0f, 1.0f);
		
		uiShader.setMatrix4("projection", projection);
		rectShader.setMatrix4("projection", projection);
		
		fr.setProjection(projection);
		genGame.sce?.setProjection(projection);
		AABB2D.setProjection(projection);
		LineStrip.setProjection(projection);
		
		overlayScreen.updateProj(this);
		currentScreen?.updateProj(this);
	}
	
	//Helper method
	public void addTexture(string n, Texture2D t){
		uiTexturesBook.Add(n, t);
	}
	
	public void toggleAdvancedMode(){
		advancedMode = !advancedMode;
		
		if(advancedMode){
			setCornerInfo("Advanced mode enabled");
		}else{
			setCornerInfo("Advanced mode diabled");
		}
	}
	
	//Solid rect
	public void drawRect(Vector2 pos, Vector2 sca, Color3 col, float alpha = 1f){
		Matrix4 model = Matrix4.CreateScale(new Vector3(sca.X, sca.Y, 0f)) * Matrix4.CreateTranslation(new Vector3(pos.X, pos.Y, 0f));
		
		rectShader.use();
		rectShader.setMatrix4("model", model);
		rectShader.setMatrix4("view", Matrix4.Identity);
		rectShader.setVector4("col", col, alpha);
		
		uiMesh.draw();
	}
	
	public void drawRect(float xp, float xy, float sx, float sy, Color3 c, float alpha = 1f){
		drawRect(new Vector2(xp, xy), new Vector2(sx, sy), c, alpha);
	}
	
	public void drawTexture(string n, Vector2 pos, Vector2 sca, Color3 col, float alpha = 1f){
		if(!uiTexturesBook.ContainsKey(n)){
			return;
		}
		
		Matrix4 model = Matrix4.CreateScale(new Vector3(sca.X, sca.Y, 0f)) * Matrix4.CreateTranslation(new Vector3(pos.X, pos.Y, 0f));
		
		uiShader.use();
		uiShader.setMatrix4("model", model);
		uiShader.setVector4("col", col, alpha);
		
		uiTexturesBook[n].bind();
		
		uiMesh.draw();
	}
	
	public void drawTexture(string n, float xpos, float ypos, float xsca, float ysca, Color3 col, float alpha = 1f){
		drawTexture(n, new Vector2(xpos, ypos), new Vector2(xsca, ysca), col, alpha);
	}
	
	public void drawTexture(string n, float xpos, float ypos, float sca, Color3 col, float alpha = 1f){
		drawTexture(n, new Vector2(xpos, ypos), new Vector2(sca, sca), col, alpha);
	}
	
	//General draw loop
	public void draw(){		
		cam.startFrame();
		
		GL.ClearColor(genGame.sce == null ? mainMenuBackgroundColor : backgroundColor);
		GL.Clear(ClearBufferMask.ColorBufferBit);
		
		//Render scene
		genGame.sce?.draw(this);
		
		//Render ui
		if(genGame.sce != null){
			if(currentScreen != null){
				overlayScreen.draw(this, false);
				
				drawRect(-width/2f, height/2f, width, height, black, 0.6f);
				currentScreen.draw(this, true);
			}else{
				overlayScreen.draw(this, true);
			}
		}else{			
			currentScreen?.draw(this, true);
		}
		
		uipr.draw(this); //Render particles
		
		//Render corner info
		if(corner != null){
			if(sw.Elapsed.TotalSeconds < 4d){
				fr.drawText(corner, -width/2f, height/2f, textSize, cornerColor);
			}else if(sw.Elapsed.TotalSeconds < 6d){
				fr.drawText(corner, -width/2f, height/2f, textSize, cornerColor, (float) (6d - sw.Elapsed.TotalSeconds));
			}else{
				corner = null;
				sw.Stop();
			}
		}
		
		//Render advanced info
		if(advancedMode){
			string s = "FPS: " + GenericGame.dh.stableFps.ToString("F0");
			fr.drawText(s, width/2f, height/2f, textSize, Placement.TopRight, textColor);
			
			ramUpdateCounter += (float) GenericGame.dh.deltaTime;
			if(ramUpdateCounter > 1f){
				ramUpdateCounter = 0f;
				ramMB = Process.GetCurrentProcess().PrivateMemorySize64 / (1024f * 1024f);
			}
			
			s = "Memory: " + ramMB.ToString("F2") + " MB";
			fr.drawText(s, width/2f, (height/2f) - textSize.Y, textSize, Placement.TopRight, textColor);
			
			int decimals = cam.zoomFactor <= 0 ? 0 : Math.Clamp((int)(cam.zoomFactor / 10), 0, 3);
			string format = "F" + decimals;
			
			s = "Pos: (" + (-cam.position.X).ToString(format) + ", " + (-cam.position.Y).ToString(format) + "\\" + cam.zoomFactor.ToString() + ")";
			fr.drawText(s, width/2f, (height/2f) - 2f * textSize.Y, textSize, Placement.TopRight, textColor);
			
			s = "Cursor: " + cam.mouseWorldPos.ToString(format);
			fr.drawText(s, width/2f, (height/2f) - 3f * textSize.Y, textSize, Placement.TopRight, textColor);
		}
	}
}