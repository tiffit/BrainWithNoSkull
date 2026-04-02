using System.ComponentModel;
using BrainWithNoSkull.Client.Guis.Widgets;
using BrainWithNoSkull.Client.Render;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.ItemStacks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Guis;

public abstract class Gui {

    public IRenderEngine RenderEngine = null!;
    protected int ScaledWidth, ScaledHeight;
    public FontRenderer Font = null!;

    protected IItemContainer? HeldStackContainer;
    protected int HeldStackIndex;

    protected List<Widget> Widgets = [];
    
    public virtual void Init(IRenderEngine renderEngine, FontRenderer font, int scaledWidth, int scaledHeight){
        Widgets.Clear();
        RenderEngine = renderEngine;
        ScaledWidth = scaledWidth;
        ScaledHeight = scaledHeight;
        Font = font;
    }

    public void RenderBase(VertexBuilder guiVb){
        foreach (Widget widget in Widgets) {
            widget.Render(guiVb, this);
        }
        
        Render(guiVb);
    }
    
    public abstract void Render(VertexBuilder guiVb);

    public void DrawTexture(VertexBuilder vb, float x, float y, int u, int v, int width, int height, int textureSize, uint textureIndex){
        DrawTexture(vb, x, y, 0, u, v, width, height, textureSize, textureIndex);
    }
    
    public void DrawTexture(VertexBuilder vb, float x, float y, float z, int u, int v, int width, int height, int textureSize, uint textureIndex){
        float u0 = u / (float)textureSize;
        float v0 = v / (float)textureSize;
        float u1 = (u + width) / (float)textureSize;
        float v1 = (v + height) / (float)textureSize;

        Vector2D<float> posMin = GameClient.Instance.GetScreenPos(x, y);
        Vector2D<float> posMax = GameClient.Instance.GetScreenPos(x + width, y + height);
        
        vb.Add(new Vertex{
            Pos = new Vector3D<float>(posMin, z),
            Color = 0xff_ff_ff_ff,
            TextureIndex = textureIndex,
            Uv = new Vector2D<float>(u0, v0)
        });
        
        vb.Add(new Vertex{
            Pos = new Vector3D<float>(posMax.X, posMin.Y, z),
            Color = 0xff_ff_ff_ff,
            TextureIndex = textureIndex,
            Uv = new Vector2D<float>(u1, v0)
        });
        
        vb.Add(new Vertex{
            Pos = new Vector3D<float>(posMax, z),
            Color = 0xff_ff_ff_ff,
            TextureIndex = textureIndex,
            Uv = new Vector2D<float>(u1, v1)
        });
        
        vb.Add(new Vertex{
            Pos = new Vector3D<float>(posMax, z),
            Color = 0xff_ff_ff_ff,
            TextureIndex = textureIndex,
            Uv = new Vector2D<float>(u1, v1)
        });
        
        vb.Add(new Vertex{
            Pos = new Vector3D<float>(posMin.X, posMax.Y, z),
            Color = 0xff_ff_ff_ff,
            TextureIndex = textureIndex,
            Uv = new Vector2D<float>(u0, v1)
        });
        
        vb.Add(new Vertex{
            Pos = new Vector3D<float>(posMin, z),
            Color = 0xff_ff_ff_ff,
            TextureIndex = textureIndex,
            Uv = new Vector2D<float>(u0, v0)
        });
    }

    public virtual bool PausesGame(){
        return false;
    }

}