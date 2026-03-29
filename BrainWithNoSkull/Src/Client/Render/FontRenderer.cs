using BrainWithNoSkull.Client.Atlas;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.Render.Shader;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Render;

public class FontRenderer {

    private readonly IRenderEngine _renderEngine;
    private readonly GameClient _client;
    
    public FontRenderer(GameClient client, IRenderEngine renderEngine){
        _client = client;
        _renderEngine = renderEngine;
    }

    public void RenderString(float x, float y, float z, string text, Vector3D<float> color){
        Glyph[] glyphs = new Glyph[text.Length];
        FontTextureAtlas fontAtlas = ClientAssetLoader.FontTextureAtlas;
        for (var i = 0; i < text.Length; i++) {
            glyphs[i] = fontAtlas.GetGlyph(text[i]);
        }
        RenderGlyphs(x, y, z, glyphs, in color);
    }

    private void RenderGlyphs(float x, float y, float z, Glyph[] glyphs, in Vector3D<float> color){
        VertexBuilder vb = _renderEngine.GetVertexBuilder(ShaderType.GuiText);
        foreach (Glyph glyph in glyphs) {
            RenderGlyph(x * _client.GuiScale, y * _client.GuiScale, z, glyph, vb, in color);
            x += (int)glyph.Width + 1;
        }
    }

    private void RenderGlyph(float x, float y, float z, Glyph glyph, VertexBuilder builder, in Vector3D<float> color){
        Vector2D<int> windowSize = _client.GameWindow.Size;
        float screenX = (x / windowSize.X)*2 - 1;
        float screenY = (y / windowSize.Y)*2 - 1;
        
        float screenGlyphSizeX = (8 / (float)windowSize.X)*2*_client.GuiScale;
        float screenGlyphSizeY = (8 / (float)windowSize.Y)*2*_client.GuiScale;

        Vector2D<float> uvMin = glyph.Uv.Origin;
        Vector2D<float> uvMax = glyph.Uv.Max;
        
        builder.Add(new Vertex{
            Pos = new Vector3D<float>(screenX, screenY, z),
            Uv = uvMin,
            Color = color
        });
        builder.Add(new Vertex{
            Pos = new Vector3D<float>(screenX + screenGlyphSizeX, screenY, z),
            Uv = new Vector2D<float>(uvMax.X, uvMin.Y),
            Color = color
        });
        builder.Add(new Vertex{
            Pos = new Vector3D<float>(screenX + screenGlyphSizeX, screenY + screenGlyphSizeY, z),
            Uv = uvMax,
            Color = color
        });
        
        builder.Add(new Vertex{
            Pos = new Vector3D<float>(screenX, screenY + screenGlyphSizeY, z),
            Uv = new Vector2D<float>(uvMin.X, uvMax.Y),
            Color = color
        });
        builder.Add(new Vertex{
            Pos = new Vector3D<float>(screenX, screenY, z),
            Uv = uvMin,
            Color = color
        });
        builder.Add(new Vertex{
            Pos = new Vector3D<float>(screenX + screenGlyphSizeX, screenY + screenGlyphSizeY, z),
            Uv = uvMax,
            Color = color
        });
    }

}