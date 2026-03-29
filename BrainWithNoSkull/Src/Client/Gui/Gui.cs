using BrainWithNoSkull.Client.Render;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Gui;

public abstract class Gui {

    protected IRenderEngine RenderEngine = null!;
    protected int ScaledWidth, ScaledHeight;
    protected FontRenderer Font = null!;
    
    public virtual void Init(IRenderEngine renderEngine, FontRenderer font, int scaledWidth, int scaledHeight){
        RenderEngine = renderEngine;
        ScaledWidth = scaledWidth;
        ScaledHeight = scaledHeight;
        Font = font;
    }

    public abstract void Render(VertexBuilder guiVb);

    protected void DrawTexture(VertexBuilder vb, int x, int y, int u, int v, int width, int height, int textureSize, uint textureIndex){
        DrawTexture(vb, x, y, 0, u, v, width, height, textureSize, textureIndex);
    }
    
    protected void DrawTexture(VertexBuilder vb, int x, int y, float z, int u, int v, int width, int height, int textureSize, uint textureIndex){
        float u0 = u / (float)textureSize;
        float v0 = v / (float)textureSize;
        float u1 = (u + width) / (float)textureSize;
        float v1 = (v + height) / (float)textureSize;

        Vector2D<float> posMin = GameClient.Instance.GetScreenPos(x, y);
        Vector2D<float> posMax = GameClient.Instance.GetScreenPos(x + width, y + height);
        
        vb.Add(new Vertex{
            Pos = new Vector3D<float>(posMin, z),
            Color = Vector3D<float>.One,
            TextureIndex = textureIndex,
            Uv = new Vector2D<float>(u0, v0)
        });
        
        vb.Add(new Vertex{
            Pos = new Vector3D<float>(posMax.X, posMin.Y, z),
            Color = Vector3D<float>.One,
            TextureIndex = textureIndex,
            Uv = new Vector2D<float>(u1, v0)
        });
        
        vb.Add(new Vertex{
            Pos = new Vector3D<float>(posMax, z),
            Color = Vector3D<float>.One,
            TextureIndex = textureIndex,
            Uv = new Vector2D<float>(u1, v1)
        });
        
        vb.Add(new Vertex{
            Pos = new Vector3D<float>(posMax, z),
            Color = Vector3D<float>.One,
            TextureIndex = textureIndex,
            Uv = new Vector2D<float>(u1, v1)
        });
        
        vb.Add(new Vertex{
            Pos = new Vector3D<float>(posMin.X, posMax.Y, z),
            Color = Vector3D<float>.One,
            TextureIndex = textureIndex,
            Uv = new Vector2D<float>(u0, v1)
        });
        
        vb.Add(new Vertex{
            Pos = new Vector3D<float>(posMin, z),
            Color = Vector3D<float>.One,
            TextureIndex = textureIndex,
            Uv = new Vector2D<float>(u0, v0)
        });
    }

}