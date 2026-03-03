using System.Diagnostics;
using AshLib;
using AshLib.AshFiles;

using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

class Screens{
	public UiScreen mainMenu;
	public UiScreen infoMenu;
	public UiScreen helpMenu;
	
	public UiScreen[] optionsPages;
	public UiScreen[] controlsPages;
	
	public UiScreen pauseMenu;
	
	public Screens(GenericGame genGame, Renderer ren){
		AshFile config = genGame.config;
		
		mainMenu = new UiScreen(
			new UiImage(Placement.TopCenter, 0, 20, 110, 110, "icon").setColor(Color3.White),
			new UiButton(Placement.Center, 0, -1f * Renderer.separation, 300f, "Play").setAction(genGame.setNewScene),
			new UiButton(Placement.Center, 0, 0, 300f, "Options").setAction(() => ren.setScreen(optionsPages[0])),
			new UiButton(Placement.Center, 0, 2f * Renderer.separation, 300f, "Exit").setColor(Renderer.greenButtonColor).setAction(genGame.Close),
			new UiImageButton(Placement.BottomRight, 0, 0, 35f, 35f, "info").setAction(() => ren.setScreen(infoMenu)).setDescription("Info"),
			new UiImageButton(Placement.BottomLeft, 0, 0, 35f, 35f, "help").setAction(() => ren.setScreen(helpMenu)).setDescription("Help")
		);
		
		infoMenu = new UiScreen(
			new UiText(Placement.TopCenter, 0f, 20f, "Info").setColor(Renderer.titleTextColor),
			new UiText(Placement.TopCenter, 0f, 3f * Renderer.textSize.Y, "Generic Game, created by " + BuildInfo.Author).setColor(Renderer.selectedTextColor),
			new UiText(Placement.TopCenter, 0f, 4f * Renderer.textSize.Y, "Version v" + BuildInfo.Version).setColor(Renderer.selectedTextColor),
			new UiText(Placement.TopCenter, 0f, 5f * Renderer.textSize.Y, "Version built on " + BuildInfo.BuildDate),
			new UiText(Placement.TopCenter, 0f, 7f * Renderer.textSize.Y, "Based on Generic game template by siljamdev").setColor(Renderer.selectedTextColor),
			new UiButton(Placement.BottomCenter, 0f, 2f * Renderer.separation, 300f, "GitHub").setAction(() => openUrl("https://github.com/siljamdev/GenericGame")),
			new UiButton(Placement.BottomCenter, 0f, 1f * Renderer.separation, 300f, "Close").setColor(Renderer.redButtonColor).setAction(ren.closeScreen)
		).addScrollingLog(new UiLog(20f, 20f, 9f * Renderer.textSize.Y,
					"### Changelog ###",
					"- Removed Herobrine",
					"- Nerfed miner"
		));
		
		helpMenu = new UiScreen(
			new UiText(Placement.TopCenter, 0f, 20f, "Help").setColor(Renderer.titleTextColor),
			new UiButton(Placement.BottomCenter, 0f, 1f * Renderer.separation, 300f, "Close").setColor(Renderer.redButtonColor).setAction(ren.closeScreen)
		).addScrollingLog(new UiLog(20f, 20f, 6f * Renderer.textSize.Y,
					"To get help with the program, yell 'HELP' very loud",
					"We will come and help you"
		));
		
		UiCheck vsync = new UiCheck(Placement.Center, 0f, -1f * Renderer.separation, Renderer.textSize.Y + 10f, Renderer.textSize.Y + 10f, "Vsync:", config.GetValue<bool>("vsync"));
		UiField maxFpsF = new UiField(Placement.Center, 0f, 0f, 30f, "Max FPS:", genGame.UpdateFrequency.ToString(), 6, WritingType.FloatPositive);
		UiCheck sound = new UiCheck(Placement.Center, 0f, 1f * Renderer.separation, Renderer.textSize.Y + 10f, Renderer.textSize.Y + 10f, "Sound:", config.GetValue<bool>("sound"));
		UiCheck particles = new UiCheck(Placement.Center, 0f, 2f * Renderer.separation, Renderer.textSize.Y + 10f, Renderer.textSize.Y + 10f, "Particles:", config.GetValue<bool>("particles"));
		
		(string key, object value, string description)[] options = genGame.getConfigurableOptions();
		
		const int optionsPerPage = 6;
		optionsPages = new UiScreen[(int) Math.Ceiling(options.Length / (float) optionsPerPage)];
		
		UiText optionsErrorText = new UiText(Placement.BottomCenter, 0f, 180f, "").setColor(Renderer.redTextColor);
		
		int j = 0;
		foreach(var ka in options.Chunk(optionsPerPage)){
			List<UiElement> optionsElements = new(6);
			optionsElements.Add(new UiText(Placement.TopCenter, 0f, 20f, "Options - " + (j + 1)).setColor(Renderer.titleTextColor));
			
			int i = -4;
			foreach((string key, object value, string description) in ka){
				if(value is bool){
					optionsElements.Add(new UiCheck(Placement.Center, 0f, i * Renderer.separation, Renderer.textSize.Y + 10f, Renderer.textSize.Y + 10f, description + ":", config.GetValue<bool>(key)));
				}else if(value is string){
					optionsElements.Add(new UiField(Placement.Center, 0f, i * Renderer.separation, 100f, description + ":", config.GetValue<string>(key), 16, WritingType.String));
				}else if(value is float){
					optionsElements.Add(new UiField(Placement.Center, 0f, i * Renderer.separation, 30f, description + ":", config.GetValue<float>(key).ToString(), 6, WritingType.FloatPositive));
				}else if(value is int){
					optionsElements.Add(new UiField(Placement.Center, 0f, i * Renderer.separation, 30f, description + ":", config.GetValue<int>(key).ToString(), 4, WritingType.Int));
				}else if(value is Color3){
					optionsElements.Add(new UiField(Placement.Center, 0f, i * Renderer.separation, 60f, description + ":", config.GetValue<Color3>(key).ToString(), 7, WritingType.Hex));
				}else{
					continue;
				}
				
				i++;
			}
			
			int s = j;
			
			optionsElements.Add(new UiImageButton(Placement.BottomLeft, 0f, 0f, 35f, 35f, "file").setAction(() => {
				openFolder(genGame.dep.path);
			}).setDescription("Open data directory"));
			
			optionsElements.Add(new UiButton(Placement.BottomCenter, 0f, 3f * Renderer.separation, 300f, "Controls").setAction(() => ren.setScreen(controlsPages[0])));
			optionsElements.Add(new UiButton(Placement.BottomCenter, -79f, 2f * Renderer.separation, 142f, "Save").setColor(Renderer.greenButtonColor).setAction(() => {
				bool hasError = false;
				
				int i = 1;
				foreach((string key, object value, string description) in ka){
					if(value is bool){
						config.Set(key, ((UiCheck) optionsElements[i]).isChecked);
					}else if(value is string){
						config.Set(key, ((UiField) optionsElements[i]).text);
					}else if(value is float){
						if(float.TryParse(((UiField) optionsElements[i]).text, out float f)){
							config.Set(key, f);
						}else{
							optionsPages[s].showError("Invalid float: " + description);
							hasError = true;
						}
					}else if(value is int){
						if(int.TryParse(((UiField) optionsElements[i]).text, out int f)){
							config.Set(key, f);
						}else{
							optionsPages[s].showError("Invalid int: " + description);
							hasError = true;
						}
					}else if(value is Color3){
						if(Color3.TryParse(((UiField) optionsElements[i]).text, out Color3 f)){
							config.Set(key, f);
						}else{
							optionsPages[s].showError("Invalid color: " + description);
							hasError = true;
						}
					}else{
						continue;
					}
					
					i++;
				}
				
				if(!hasError){
					optionsPages[s].showError("");
					config.Save();
					genGame.loadConfig();
					
					ren.setCornerInfo("Config saved", Renderer.selectedTextColor);
				}
			}));
			optionsElements.Add(new UiButton(Placement.BottomCenter, 79f, 2f * Renderer.separation, 142f, "Reset").setAction(genGame.resetConfig));
			optionsElements.Add(new UiButton(Placement.BottomCenter, 0f, 1f * Renderer.separation, 300f, "Close").setColor(Renderer.redButtonColor).setAction(ren.closeScreen));
			
			if(j > 0){
				optionsElements.Add(new UiImageButton(Placement.CenterLeft, 10f, 0f, 45f, 45f, "previous").setAction(() => {
					ren.closeScreen();
					ren.setScreen(optionsPages[s - 1]);
				}));
			}
			
			if(j < optionsPages.Length - 1){
				optionsElements.Add(new UiImageButton(Placement.CenterRight, 10f, 0f, 45f, 45f, "next").setAction(() => {
					ren.closeScreen();
					ren.setScreen(optionsPages[s + 1]);
				}));
			}
			
			optionsPages[j] = new UiScreen(
				optionsElements.ToArray()
			).addErrorText(optionsErrorText);
			
			j++;
		}
		
		
		const int keybindsPerPage = 7;
		controlsPages = new UiScreen[(int) Math.Ceiling(KeyBind.configurables.Count / (float) keybindsPerPage)];
		
		j = 0;
		foreach(var ka in KeyBind.configurables.Chunk(keybindsPerPage)){
			List<UiElement> controlsElements = new(4);
			controlsElements.Add(new UiText(Placement.TopCenter, 0f, 20f, "Controls - " + (j + 1)).setColor(Renderer.titleTextColor));
			
			int i = -4;
			foreach(KeyBind k in ka){
				controlsElements.Add(new UiKeyField(Placement.Center, 0f, i * Renderer.separation, k.configDescription + ":", k));
				i++;
			}
			
			controlsElements.Add(new UiButton(Placement.BottomCenter, -79f, 2f * Renderer.separation, 142f, "Save").setColor(Renderer.greenButtonColor).setAction(genGame.saveControls));
			controlsElements.Add(new UiButton(Placement.BottomCenter, 79f, 2f * Renderer.separation, 142f, "Reset").setAction(genGame.resetControls));
			controlsElements.Add(new UiButton(Placement.BottomCenter, 0f, 1f * Renderer.separation, 300f, "Close").setColor(Renderer.redButtonColor).setAction(ren.closeScreen));
			
			int s = j;
			if(j > 0){
				controlsElements.Add(new UiImageButton(Placement.CenterLeft, 10f, 0f, 45f, 45f, "previous").setAction(() => {
					ren.closeScreen();
					ren.setScreen(controlsPages[s - 1]);
				}));
			}
			
			if(j < controlsPages.Length - 1){
				controlsElements.Add(new UiImageButton(Placement.CenterRight, 10f, 0f, 45f, 45f, "next").setAction(() => {
					ren.closeScreen();
					ren.setScreen(controlsPages[s + 1]);
				}));
			}
			
			controlsPages[j] = new UiScreen(
				controlsElements.ToArray()
			);
			
			j++;
		}
		
		pauseMenu = new UiScreen(
			new UiText(Placement.TopCenter, 0f, 20f, "Pause").setColor(Renderer.titleTextColor),
			new UiButton(Placement.Center, 0f, -1f * Renderer.separation, 300f, "Close").setColor(Renderer.redButtonColor).setAction(ren.closeScreen),
			new UiButton(Placement.Center, 0f, 0f, 300f, "Options").setAction(() => ren.setScreen(optionsPages[0])),
			new UiButton(Placement.Center, 0f, 2f * Renderer.separation, 300f, "Exit to menu").setColor(Renderer.greenButtonColor).setAction(genGame.closeScene),
			new UiImageButton(Placement.CenterLeft, 0f, 0f, 35f, 35f, "screenshot").setAction(() => {
				ren.closeScreen();
				genGame.takeScreenshotNextFrame = true;
			}).setDescription("Take screenshot")
		);
	}
	
	static void openUrl(string url){
		try{
			if(OperatingSystem.IsWindows()){
				Process.Start(new ProcessStartInfo{
					FileName = url,
					UseShellExecute = true
				});
			}
			else if(OperatingSystem.IsLinux()){
				Process.Start("xdg-open", url);
			}
			else if(OperatingSystem.IsMacOS()){
				Process.Start("open", url);
			}
		}
		catch{}
	}
	
	static void openFolder(string path){
		try{
			if(OperatingSystem.IsWindows()){
				Process.Start(new ProcessStartInfo{
					FileName = path,
					UseShellExecute = true
				});
			}
			else if(OperatingSystem.IsLinux()){
				Process.Start("xdg-open", path);
			}
			else if(OperatingSystem.IsMacOS()){
				Process.Start("open", path);
			}
		}
		catch{}
	}
}