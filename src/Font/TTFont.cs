using System;
using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using StbImageSharp;
using StbTrueTypeSharp;

record GlyphInfo(Vector2 uv0, Vector2 uv1, Vector2 size, Vector2 bearing, float advance);

record TTFont(Texture2D atlas, Dictionary<char, GlyphInfo> glyphs){
	const string defaultMap = "ABCDEFGHIJKLMNOPQRSTUVWXYZ .,:+-*/\\'\"$()[]^?!%~º1234567890 |□#<>abcdefghijklmnopqrstuvwxyz ;&@`_{}Ññ=€¿¡";
	
	public static TTFont fromAssembly(string name, TextureParams tp, float pixelHeight, int atlasSize, int u = 0, string r = defaultMap){
		try{
			return fromBytes(AssemblyFiles.getBytes(name), tp, pixelHeight, atlasSize, u, r, name);
		}catch(Exception e){
			throw new Exception("TTF loading failed from assembly: " + name, e);
		}
	}
	
	public static TTFont fromFile(string path, TextureParams tp, float pixelHeight, int atlasSize, int u = 0, string r = defaultMap){
		try{
			return fromBytes(File.ReadAllBytes(path), tp, pixelHeight, atlasSize, u, r, path);
		}catch(Exception e){
			throw new Exception("TTF loading failed from file: " + path, e);
		}
	}
	
	public static TTFont fromStream(Stream s, TextureParams tp, float pixelHeight, int atlasSize, int u = 0, string r = defaultMap, string name = null){
		using(var ms = new System.IO.MemoryStream()){
			s.CopyTo(ms);
			return fromBytes(ms.ToArray(), tp, pixelHeight, atlasSize, u, r, name);
		}
	}
	
	public unsafe static TTFont fromBytes(byte[] data, TextureParams tp, float pixelHeight, int atlasSize, int u = 0, string r = defaultMap, string name = null){	
		StbTrueType.stbtt_fontinfo fontInfo = StbTrueType.CreateFont(data, 0);
		if(fontInfo == null){
			throw new Exception("Failed to initialize TTF font.");
		}
		
		byte[] bitmap = new byte[atlasSize * atlasSize];
		StbTrueType.stbtt_pack_context packContext = new StbTrueType.stbtt_pack_context();
		fixed(byte* ptr = bitmap){
			if(StbTrueType.stbtt_PackBegin(packContext, ptr, atlasSize, atlasSize, 0, 1, null) == 0){
				throw new Exception("Failed to begin font packing.");
			}	
		}
		
		
		//collect all chars in BMP
		char[] chars = r.ToCharArray().Distinct().ToArray();
		
		//prepare packed chars
		StbTrueType.stbtt_packedchar[] packedChars = new StbTrueType.stbtt_packedchar[chars.Length];
		StbTrueType.stbtt_pack_range fontRange;
		
		fixed(int* ptr = Array.ConvertAll(chars, c => (int)c))
		fixed(StbTrueType.stbtt_packedchar* ptr2 = packedChars){
			fontRange = new StbTrueType.stbtt_pack_range{
				font_size = pixelHeight,
				num_chars = chars.Length,
				array_of_unicode_codepoints = ptr,
				chardata_for_range = ptr2,
				h_oversample = 2,
				v_oversample = 2
			};
		}
		
		//pack
		fixed(byte* ptr = data){
			if(StbTrueType.stbtt_PackFontRanges(packContext, ptr, 0, &fontRange, 1) == 0){
				throw new Exception("Failed to pack font glyphs.");
			}
		}
		
		StbTrueType.stbtt_PackEnd(packContext);
		
		//bitmap = bitmap.SelectMany(h => new byte[]{h, h, h}).ToArray();
		
		ImageResult img = new ImageResult{
			Width = atlasSize,
			Height = atlasSize,
			Comp = ColorComponents.Grey, // single channel
			Data = bitmap
		};
		
		Texture2D t = new Texture2D(img, tp, u, name);
		
		//Build glyph dictionary
		Dictionary<char, GlyphInfo> glyphs = new Dictionary<char, GlyphInfo>();
		for(int i = 0; i < chars.Length; i++){
			StbTrueType.stbtt_packedchar pc = packedChars[i];
			GlyphInfo g = new GlyphInfo(
				new Vector2(pc.x0 / (float) atlasSize, pc.y0 / (float) atlasSize),
				new Vector2(pc.x1 / (float) atlasSize, pc.y1 / (float) atlasSize),
				new Vector2((pc.x1 - pc.x0) / (float) pixelHeight, (pc.y1 - pc.y0) / (float) pixelHeight),
				new Vector2(pc.xoff / (float) pixelHeight, pc.yoff / (float) pixelHeight),
				pc.xadvance / (float) pixelHeight
			);
			
			glyphs[chars[i]] = g;
		}
		
		Console.WriteLine("Loaded TTF: " + name);
		
		return new TTFont(t, glyphs);
	}
}