using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common.Input;
using StbImageSharp;
using StbImageWriteSharp;
using AshLib;
using AshLib.Time;
using AshLib.Folders;
using AshLib.AshFiles;

using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;


partial class GenericGame : GameWindow{
	
	public const string version = "1.0.0";
	
	KeyBind fullscreen = new KeyBind(Keys.F11, false);
	KeyBind screenshot = new KeyBind(Keys.F2, false);
	
	KeyBind advancedMode = new KeyBind(Keys.LeftAlt, false);
	
	KeyBind moveUp = new KeyBind(Keys.W, true);
	KeyBind moveDown = new KeyBind(Keys.S, true);
	KeyBind moveLeft = new KeyBind(Keys.A, true);
	KeyBind moveRight = new KeyBind(Keys.D, true);
	
	//These are static
	KeyBind escape = new KeyBind(Keys.Escape, Keys.LeftShift, false);
	KeyBind help = new KeyBind(Keys.F1, false);
	KeyBind logUp = new KeyBind(Keys.Up, true);
	KeyBind logDown = new KeyBind(Keys.Down, true);
	
	public static List<(int, int?)> meshesMarkedForDisposal = new(); //Need to dispose of these on the same thread or else a Fatal error will occur
	
	public Dependencies dep;
	public AshFile config;
	
	bool takeScreenshotNextTick;
	
	Renderer ren;
	
	public Scene sce;
	
	public SoundManager sm;
	
	Sound testSound;
	Sound testSound2;
	
	public static DeltaHelper dh;
	
	bool isFullscreened;
	
	float maxFps = 144f;
	
	static void Main(string[] args){
		if(OperatingSystem.IsWindows()){
			if(GetConsoleWindow() == IntPtr.Zero){
				AttachConsole(ATTACH_PARENT_PROCESS);
			}
		}
		
		using(GenericGame genGame = new GenericGame()){
			genGame.Run();
		}
	}
	
	GenericGame() : base(GameWindowSettings.Default, NativeWindowSettings.Default){
		CenterWindow(new Vector2i(640, 480));
		Title = "Generic Game";
		
		VSync = VSyncMode.On;
	}
	
	void initialize(){
		dh = new DeltaHelper();
		dh.Start();
		
		string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		dep = new Dependencies(appDataPath + "/genGame", true, new string[]{"screenshots"}, null);
		
		setIcon();
		
		initializeConfig();
		
		ren = new Renderer(this);
		sm = new SoundManager();
		
		testSound = Sound.monoFromAssembly("res.sounds.goofy.ogg");
		testSound2 = Sound.monoFromAssembly("res.sounds.gnome.ogg");
		
		Scene.initialize();
		AABB.initialize();
		
		initializeScreens();
		
		ren.setScreen(mainMenu);
	}
	
	void onResize(int x, int y){
		GL.Viewport(0, 0, x, y);
		ren?.updateSize(x, y);
	}
	
	void initializeConfig(){
		int[] k = new List<Keys>{
			fullscreen.key,
			screenshot.key,
			advancedMode.key,
			moveUp.key,
			moveDown.key,
			moveLeft.key,
			moveRight.key
		}.Select(n => (int) n).ToArray();
		
		AshFileModel afm = new AshFileModel(
			new ModelInstance(ModelInstanceOperation.Type, "vsync", true),
			new ModelInstance(ModelInstanceOperation.Type, "maxFps", 144f),
			new ModelInstance(ModelInstanceOperation.Type, "controls", k)
		);
		
		config = dep.config;
		
		afm.deleteNotMentioned = true;
		
		config *= afm;
		
		//Set current version and path. Might be needed by someone (maybe)
		config.Set("version", version);
		try{ //Might not work on linux
			config.Set("path", System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
		}catch{}
		
		config.Save();
		
		setVsync(config.GetValue<bool>("vsync"));
		maxFps = config.GetValue<float>("maxFps");
		
		int[] ka = config.GetValue<int[]>("controls");
		if(ka.Length > 6){
			fullscreen.key = (Keys)ka[0];
			screenshot.key = (Keys)ka[1];
			advancedMode.key = (Keys)ka[2];
			moveUp.key = (Keys)ka[3];
			moveDown.key = (Keys)ka[4];
			moveLeft.key = (Keys)ka[5];
			moveRight.key = (Keys)ka[6];
		}
	}
	
	public void setVsync(bool b){
		if(b){
			VSync = VSyncMode.On;
		}else{
			VSync = VSyncMode.Off;
		}
	}
	
	void handleKeyboardInput(){
		// check to see if the window is focused
		if(!IsFocused){
			return;
		}
		
		switch(escape.isActiveMod(KeyboardState)){
			case 1:
			if(ren.currentScreen != null){
				ren.closeScreen();
			}else{
				ren.setScreen(pauseMenu);
				
				//Delete this please
				Random rnd = new();
				Vector3 c = new Vector3((float)(rnd.NextDouble() * 2.0 - 1.0), (float)(rnd.NextDouble() * 2.0 - 1.0), (float)(rnd.NextDouble() * 2.0 - 1.0));
				c.Normalize();
				sm.play(testSound2, c);
			}
			
			break;
			
			case 2:
			Close();
			break;
		}
		
		if(screenshot.isActive(KeyboardState)){
			captureScreenshot();
			ren.setCornerInfo("Saved screenshot");
		}
		
		if(fullscreen.isActive(KeyboardState)){
			toggleFullscreen();
		}
		
		if(help.isActive(KeyboardState) && ren.currentScreen != helpMenu){
			ren.setScreen(helpMenu);
		}
		
		if(advancedMode.isActive(KeyboardState)){
			ren.toggleAdvancedMode();
			
			//Delete this too
			sm.play(testSound);
		}
		
		if(ren.currentScreen != null){			
			if(logUp.isActive(KeyboardState)){
				ren.currentScreen.scroll(40f * (float) dh.deltaTime);
			}else if(logDown.isActive(KeyboardState)){
				ren.currentScreen.scroll(-40f * (float) dh.deltaTime);
			}
			ren.cam.endFrame();
			return;
		}
		
		if(moveUp.isActive(KeyboardState)){
			ren.cam.moveUp((float) dh.deltaTime);
		}
		
		if(moveDown.isActive(KeyboardState)){
			ren.cam.moveDown((float) dh.deltaTime);
		}
		
		if(moveLeft.isActive(KeyboardState)){
			ren.cam.moveLeft((float) dh.deltaTime);
		}
		
		if(moveRight.isActive(KeyboardState)){
			ren.cam.moveRight((float) dh.deltaTime);
		}
		
		ren.cam.endFrame();
	}
	
	void setNewScene(){
		sce = new Scene(ren);
		ren.setScreen(null);
	}
	
	void closeScene(){
		sce = null;
		ren.setScreen(null);
		ren.setScreen(mainMenu);
		ren.cam.reset();
	}
	
	void toggleFullscreen(){
		VSyncMode t = VSync;
		if(!isFullscreened){
			MonitorInfo mi = Monitors.GetMonitorFromWindow(this);
			WindowState = WindowState.Fullscreen;
			this.CurrentMonitor = mi;
			isFullscreened = true;
			VSync = t;
		}else{
			WindowState = WindowState.Normal;
			isFullscreened = false;
			VSync = t;
		}
	}
	
	public void checkErrors(){
		OpenTK.Graphics.OpenGL.ErrorCode errorCode = GL.GetError();
        while(errorCode != OpenTK.Graphics.OpenGL.ErrorCode.NoError){
            Console.Error.WriteLine("OpenGL Error: " + errorCode);
			if(ren != null){
				ren.setCornerInfo("OpenGL Error: " + errorCode, Renderer.redTextColor);
			}
			
            errorCode = GL.GetError();
        }
		sm.checkErrors();
	}
	
	void disposeOfMeshes(){
		foreach((int VAO, int? VBO) in meshesMarkedForDisposal){		
			GL.DeleteVertexArray(VAO);
			
			if(VBO != null){				
				GL.DeleteBuffer((int) VBO);
			}
		}
		meshesMarkedForDisposal.Clear();
	}
	
	void captureScreenshot(){
		int width = ren.width;
		int height = ren.height;
		
		// Create a byte array to hold the pixel data
		byte[] pixels = new byte[width * height * 3]; // RGBA (4 bytes per pixel)
		
		// Read pixels from OpenGL frame buffer
		GL.ReadBuffer(ReadBufferMode.Front);
		GL.ReadPixels(0, 0, width, height, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, pixels);
		
		byte[] rgbPixels = new byte[width * height * 3]; // RGB (3 bytes per pixel)
		
		// Copy only RGB values
		for(int i = 0; i < height; i++){
			for(int j = 0; j < width; j++){
				rgbPixels[((height - i - 1) * width + j) * 3] = pixels[(i * width + j) *3 + 2];      // Blue
				rgbPixels[((height - i - 1) * width + j) * 3 + 1] = pixels[(i * width + j) * 3 + 1];  // Green
				rgbPixels[((height - i - 1) * width + j) * 3 + 2] = pixels[(i * width + j) * 3];  // Red
			}
		}
		
		// Write PNG using StbImageWriteSharp
		var writer = new StbImageWriteSharp.ImageWriter();
		using (var stream = File.OpenWrite(dep.path + "/screenshots/" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + ".png")){
			writer.WritePng(rgbPixels, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlue, stream);
		}
	}
	
	void setIcon(){
		using Stream s = AssemblyFiles.getStream("res.icon.png");
		
		//Generate the image and put it as icon
		ImageResult image = ImageResult.FromStream(s, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
		if(image == null || image.Data == null){
			return;
		}
		
		OpenTK.Windowing.Common.Input.Image i = new OpenTK.Windowing.Common.Input.Image(image.Width, image.Height, image.Data);
		WindowIcon w = new WindowIcon(i);
		
		this.Icon = w;
	}
	
	protected override void OnKeyDown(KeyboardKeyEventArgs e){
		if(ren.currentScreen != null){
			if(!e.IsRepeat && e.Key != Keys.Escape && e.Key != Keys.Backspace){
				ren.currentScreen.trySetKeybind(e.Key);
			}else if(e.Key == Keys.Backspace){
				ren.currentScreen.tryDelChar();
			}
		}else{
			base.OnKeyDown(e);
		}
	}
	
	protected override void OnTextInput(TextInputEventArgs e){
		if(ren.currentScreen != null){
			string s = e.AsString;
			ren.currentScreen.tryAddStr(s);
		}
		
		base.OnTextInput(e);
	}
	
	protected override void OnLoad(){
		initialize();
		base.OnLoad();
	}
	
	protected override void OnUnload(){
		disposeOfMeshes();
		base.OnUnload();
	}
	
	protected override void OnResize(ResizeEventArgs args){
		onResize(args.Width, args.Height);
		base.OnResize(args);
	}
	
	protected override void OnUpdateFrame(FrameEventArgs args){
		handleKeyboardInput();
		base.OnUpdateFrame(args);
	}
	
	protected override void OnRenderFrame(FrameEventArgs args){
		ren.draw();
		Context.SwapBuffers();
		checkErrors();
		disposeOfMeshes();
		if(takeScreenshotNextTick){
			captureScreenshot();
			ren.setCornerInfo("Saved screenshot");
			takeScreenshotNextTick = false;
		}
		base.OnRenderFrame(args);
		dh.Frame();
		if(VSync != VSyncMode.On){
			dh.Target(maxFps);
		}
	}
	
	protected override void OnMouseWheel(MouseWheelEventArgs args){
		if(ren.currentScreen != null){
			ren.currentScreen.scroll(args.OffsetY);
			base.OnMouseWheel(args);
			return;
		}
		
		ren.cam.scroll(args.OffsetY);
        
		base.OnMouseWheel(args);
    }
	
	protected override void OnMouseMove(MouseMoveEventArgs e){
        ren.cam.mouse(e.X, e.Y);
		base.OnMouseMove(e);
    }
	
	protected override void OnMouseDown(MouseButtonEventArgs e){
        if(e.Button == MouseButton.Left){
			if(sce != null){
				if(ren.currentScreen == null){
					if(!ren.overlayScreen.click(ren, KeyboardState.IsKeyDown(Keys.LeftShift))){
						//Some clicking thing
					}
				}else{
					ren.currentScreen.click(ren, KeyboardState.IsKeyDown(Keys.LeftShift));
				}
			}else{
				ren.currentScreen.click(ren, KeyboardState.IsKeyDown(Keys.LeftShift));
			}
        }
		
		base.OnMouseDown(e);
    }
	
	#region WINDOWS
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
		
		[DllImport("kernel32.dll")]
		static extern bool AttachConsole(int dwProcessId);
		const int ATTACH_PARENT_PROCESS = -1;
		
		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();
	#endregion
}