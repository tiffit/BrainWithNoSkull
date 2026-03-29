using Silk.NET.Maths;
using SkiaSharp;

namespace BrainWithNoSkull.Client.Atlas;

public class FontTextureAtlas: TextureAtlas {

    private readonly Dictionary<char, Glyph> _glyphs = new Dictionary<char, Glyph>();
    private uint _nextIndex;
    public uint AtlasSize;
    
    public void Create(){
        SKBitmap bitmap = SKBitmap.Decode($"./Assets/Textures/Font/font.png");
        Textures.Clear();
        AtlasSize = (uint)bitmap.Width;

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
        CreateBatchedGlyphData(['a','b'], 5);
        CreateBatchedGlyphData(['c'], 4);
        CreateBatchedGlyphData(['d','e'], 5);
        CreateBatchedGlyphData(['f'], 4);
        CreateBatchedGlyphData(['g','h'], 5);
        CreateBatchedGlyphData(['i'], 1);
        CreateBatchedGlyphData(['j'], 3);
        CreateBatchedGlyphData(['k'], 4);
        CreateBatchedGlyphData(['l'], 3);
        CreateBatchedGlyphData(['m'], 5);
        CreateBatchedGlyphData(['n','o','p','q','r','s'], 4);
        CreateBatchedGlyphData(['t'], 3);
        CreateBatchedGlyphData(['u'], 4);
        CreateBatchedGlyphData(['v','w','x'], 5);
        CreateBatchedGlyphData(['y','z'], 4);
        
        CreateInvisibleGlyph(' ', 5);
    }

    private void CreateBatchedGlyphData(char[] chars, uint width){
        foreach (char c in chars) {
            CreateGlyphData(c, width);
        }
    }
    
    private void CreateGlyphData(char c, uint width){
        Glyph glyph = new Glyph{ Index = _nextIndex++, Width = width };
        SetUv(glyph);
        _glyphs[c] = glyph;
    }
    
    private void CreateInvisibleGlyph(char c, uint width){
        _glyphs[c] = new Glyph{ Index = 0, Width = width };
    }

    public Glyph GetGlyph(char c){
        return _glyphs.TryGetValue(c, out Glyph? data) ? data : _glyphs['\u0000'];
    }

    private void SetUv(Glyph glyph){
        uint scaledIndex = glyph.Index * 8;
        uint textureX = scaledIndex % AtlasSize;
        uint textureY = (scaledIndex / AtlasSize)*8;
        float size = 8 / (float)AtlasSize;
        glyph.Uv = new Rectangle<float>(textureX / (float)AtlasSize, textureY / (float)AtlasSize, size, size);
    }
    
}

public class Glyph {
    public uint Index;
    public uint Width;
    public Rectangle<float> Uv;
}