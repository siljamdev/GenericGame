using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;

class Renderer{
	
	public int width{get; private set;}
	public int height{get; private set;}
	
	Color4 backgroundColor = new Color4(0, 0, 0, 1);
	//public static readonly Vector2 textSize = new Vector2(15f, 18f);
	public static readonly Vector2 textSize = new Vector2(18f);
	
	public const float separation = 40f;
	public const float fieldSeparation = 30f;
	
	public static readonly Color3 textColor = new Color3("EFEFEF");
	public static readonly Color3 selectedTextColor = new Color3("FFFF8F");
	public static readonly Color3 titleTextColor = new Color3("DFDFFF");
	public static readonly Color3 fieldTextColor = new Color3("D6D6D6");
	public static readonly Color3 buttonColor = new Color3("555577");
	public static readonly Color3 greenButtonColor = new Color3("557755");
	public static readonly Color3 redButtonColor = new Color3("775555");
	public static readonly Color3 redTextColor = new Color3("FF8888");
	public static readonly Color3 fieldColor = new Color3("222222");
	public static readonly Color3 fieldSelectedColor = new Color3("505050");
	
	public static readonly Color3 black = Color3.Black;
	
	float ramMB = 0f;
	float ramUpdateCounter = 0f;
	
	public Camera cam{get; private set;}
	public FontRenderer fr{get; private set;}
	public ParticleRenderer uipr{get; private set;} //UI particle renderer
	
	Dictionary<string, Texture2D> uiTexturesBook = new();

	public Mesh uiMesh{get; private set;}
	
	public Shader uiShader{get; private set;}
	public Shader rectShader{get; private set;}
	
	//Example
	public Texture2D iconTex;
	
	GenericGame genGame;
	
	Stopwatch sw;
	string corner;
	Color3 cornerColor;
	
	public Matrix4 projection{get; private set;}
	
	bool advancedMode;
	
	public UiScreen overlayScreen;
	public UiScreen currentScreen;
	
	public Stack<UiScreen> screens{get; private set;}
	
	public Renderer(GenericGame st){
		genGame = st;
		sw = new Stopwatch();
		
		width = 640;
		height = 480;
		
		screens = new Stack<UiScreen>();
		
		//Enable transparency (blending)
		GL.Enable(EnableCap.Blend);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		
		//other utilities
		cam = new Camera(this);
		cam.onViewChange += AABB2D.setView;
		cam.onViewChange += LineStrip.setView;
		
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
		TTFont f = TTFont.fromAssembly("res.AtkinsonHyperlegible.ttf", TextureParams.Smooth, 128, 1024);
		fr = new TruetypeFontRenderer(uiMesh, f, 9);

		uipr = new ParticleRenderer();
		
		//Example texture
		iconTex = Texture2D.fromAssembly("res.icon.png", TextureParams.Default);
		
		//load textures
		addTexture("tick", Texture2D.fromAssembly("res.textures.tick.png", TextureParams.Default));
		addTexture("screenshot", Texture2D.fromAssembly("res.textures.screenshotButton.png", TextureParams.Default));
		addTexture("next", Texture2D.fromAssembly("res.textures.nextButton.png", TextureParams.Default));
		addTexture("previous", Texture2D.fromAssembly("res.textures.previousButton.png", TextureParams.Default));
		addTexture("icon", iconTex);
		addTexture("info", Texture2D.fromAssembly("res.textures.infoIcon.png", TextureParams.Default));
		addTexture("help", Texture2D.fromAssembly("res.textures.helpIcon.png", TextureParams.Default));
		
		overlayScreen = new UiScreen(
			new UiImage(Placement.TopLeft, 0f, 0f, 30, 30, "icon", Color3.White)
		);
		
		overlayScreen.updateProj(this);
	}
	
	public void setScreen(UiScreen s){
		if(currentScreen != null){
			currentScreen.closeAction?.Invoke();
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
			currentScreen = null;
			return;
		}
		
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
	
	public void drawRect(float xp, float xy, float sx, float sy, Color3 c, float a = 1f){
		drawRect(new Vector2(xp, xy), new Vector2(sx, sy), c, a);
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
		GL.ClearColor(backgroundColor);
		GL.Clear(ClearBufferMask.ColorBufferBit);
		
		cam.startFrame();
		
		//Render scene
		if(genGame.sce != null){
			genGame.sce.draw(this);
		}else{
			drawRect(-width/2f, height/2f, width, height, new Color3("2D6054")); //background
		}
		
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
		
		//Render ui
		if(genGame.sce != null){
			if(currentScreen != null){
				overlayScreen.draw(this, false);
				
				drawRect(-width/2f, height/2f, width, height, Color3.Black, 0.6f);
				currentScreen.draw(this, true);
			}else{
				overlayScreen.draw(this, true);
			}
		}else{			
			currentScreen?.draw(this, true);
		}
		
		uipr.draw(this); //Render particles
		
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