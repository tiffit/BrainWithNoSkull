using SkiaSharp;

namespace BrainWithNoSkull.Client.Atlas;

public class FontTextureAtlas: TextureAtlas {

    private readonly Dictionary<char, GlyphData> _glyphs = new Dictionary<char, GlyphData>();
    private uint _nextIndex;
    
    public void Create(){
        SKBitmap bitmap = SKBitmap.Decode($"./Assets/Textures/Font/font.png");
        Textures.Clear();

        SKBitmap grayscale = new SKBitmap(bitmap.Width, bitmap.Height, SKColorType.R8Unorm, SKAlphaType.Premul);
        using (SKCanvas canvas = new SKCanvas(grayscale)) {
            canvas.DrawBitmap(bitmap, 0, 0);
        }
        
        Textures.Add(grayscale);
        
        _glyphs.Clear();
        _nextIndex = 0;
        CreateBatchedGlyphData(['A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'], 5);
        CreateBatchedGlyphData(['1','2','3','4','5','6','7','8','9','0'], 5);
        CreateGlyphData('\u0000', 5);
    }

    private void CreateBatchedGlyphData(char[] chars, uint width){
        foreach (char c in chars) {
            CreateGlyphData(c, width);
        }
    }
    
    private void CreateGlyphData(char c, uint width){
        _glyphs[c] = new GlyphData{Index = _nextIndex++, Width = width};
    }

    public GlyphData GetGlyphData(char c){
        return _glyphs.TryGetValue(c, out GlyphData? data) ? data : _glyphs['\u0000'];
    }
    
}

public class GlyphData {
    public uint Index;
    public uint Width;
}