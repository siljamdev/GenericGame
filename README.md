# Generic game Template
<img src="res/icon.png" width="200"/>

Template that makes it easy to create 2D games with [OpenTK](https://github.com/opentk/opentk).  
Comes with user interfaces text rendering, sound management, a free camera, basic menus, options and customizable controls.

Everybody is free to use this as a template for their game or in any other way.

## What is included
- **Shader class**, with uniform pre-locating and optimized binding
- **Texture2D class**, with optimized binding and 8 unit support
- **Mesh class**, with support for static data, dynamic data and EBOs, and optimized binding
- **Camera class**, with movement and zoom
- **Sound** and **SoundManager** classes, that abstract on top of OpenAL for ease of use
- **Ui elements** and **screens**, that provide easy access to many useful elements
- **Font rendering**, that renders both bitmap and truetype fonts
- **Particle** and **ParticleRenderer** classes
- **Options**, such as vsync and maximum FPS, easily expandable
- **Controls**, that allow to change keybinds, easily expandable
- **AABBs**

## How to start creating a game
1. [Fork this GitHub repo](https://github.com/siljamdev/GenericGame/fork)
2. Set up a local Git repo and pull from your forked github
3. Start modifying what you wish and build using dotnet (use dotnet 8 or higher)

## Games based on this
- **[InfDun](https://github.com/siljamdev/InfDun)**: turn based dungeon crawler game

## How it works
Mainly, [OpenTK](https://github.com/opentk/opentk) is used for graphics via [OpenGL](https://www.opengl.org/) and sound via [OpenAL](https://www.openal.org/).  
[AshLib](https://github.com/siljamdev/AshLib) is used too, for the Dependencies class, the AshFiles for configs and the Color3 struct for colors across the renderers.  

## Performance
It will achieve high FPS(1000-2000) effortlessly on a capable system (the built template included here, actual games may vary).  
Thanks to its lightweight nature, its memory usage averages ~70 MB when using a bitmap font and ~90 MB when using a TTF (the built template included here, actual games may use more).  
The built binaries average ~75 MB in size (the built template found here, actual games might be bigger).

## Profiler support
When executing in the `Debug` configuration, error messages will be more clear, and profiler support will be active.  
It has been tested on [RenderDoc](https://renderdoc.org/) and works flawlessly.

## Font
The default included font is [Atkinson Hyperlegible](https://fonts.google.com/specimen/Atkinson+Hyperlegible)