using System.ComponentModel;
using BrainWithNoSkull.Client.Render;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.ItemStacks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Guis;

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

    protected void DrawTexture(VertexBuilder vb, float x, float y, int u, int v, int width, int height, int textureSize, uint textureIndex){
        DrawTexture(vb, x, y, 0, u, v, width, height, textureSize, textureIndex);
    }
    
    protected void DrawTexture(VertexBuilder vb, float x, float y, float z, int u, int v, int width, int height, int textureSize, uint textureIndex){
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

    protected void DrawSlot(VertexBuilder vb, IItemContainer? container, int slotIndex, int x, int y, float z, uint invIconsTexture){
        ItemStack? stack = container?.GetItem(slotIndex);
        bool hasStack = false;
        if (!ItemStack.IsNull(stack)) {
            GameClient.Instance.ItemRenderer.RenderItemInGui(vb, stack, x + 9, y + 9, z - 0.02f, 40, 40);
            if (stack.Count > 1) {
                Font.RenderString(x, y, z - 0.03f, "" + stack.Count, ColorUtil.FromFloats(1f, 0.65f, 0f));
            }
            hasStack = true;
        }
        DrawTexture(vb, x, y, z, hasStack ? 0 : 18, 0, 18, 18, 256, invIconsTexture);
    }

    public virtual bool PausesGame(){
        return false;
    }

}