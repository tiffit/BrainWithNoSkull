using BrainWithNoSkull.Client.Guis;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.Render.Shader;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Render;

public class GuiRenderer {

    private readonly Gui _gui;
    private readonly IRenderEngine _renderEngine;
    public uint Color = 0xff_ff_ff_ff;

    public GuiRenderer(Gui gui, IRenderEngine renderEngine){
        _gui = gui;
        _renderEngine = renderEngine;
    }
    
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
            Color = Color,
            TextureIndex = textureIndex,
            Uv = new Vector2D<float>(u0, v0)
        });

        vb.Add(new Vertex{
            Pos = new Vector3D<float>(posMax.X, posMin.Y, z),
            Color = Color,
            TextureIndex = textureIndex,
            Uv = new Vector2D<float>(u1, v0)
        });

        vb.Add(new Vertex{
            Pos = new Vector3D<float>(posMax, z),
            Color = Color,
            TextureIndex = textureIndex,
            Uv = new Vector2D<float>(u1, v1)
        });

        vb.Add(new Vertex{
            Pos = new Vector3D<float>(posMax, z),
            Color = Color,
            TextureIndex = textureIndex,
            Uv = new Vector2D<float>(u1, v1)
        });

        vb.Add(new Vertex{
            Pos = new Vector3D<float>(posMin.X, posMax.Y, z),
            Color = Color,
            TextureIndex = textureIndex,
            Uv = new Vector2D<float>(u0, v1)
        });

        vb.Add(new Vertex{
            Pos = new Vector3D<float>(posMin, z),
            Color = Color,
            TextureIndex = textureIndex,
            Uv = new Vector2D<float>(u0, v0)
        });
    }
    
    public void DrawLine(float x1, float y1, float x2, float y2, float z, float width, uint color){
        VertexBuilder vb = _renderEngine.GetVertexBuilder(ShaderType.GuiColor);
        Vector2D<float> in1 = GameClient.Instance.GetScreenPos(x1, y1);
        Vector2D<float> in2 = GameClient.Instance.GetScreenPos(x2, y2);
        width = GameClient.Instance.GetScreenSize(width, 1).X;

        Vector2D<float> diff = in2 - in1;

        float distance = diff.Length;
        float heightOffset = width / 2f;

        Vector2D<float> p1 = new Vector2D<float>(0, -heightOffset);
        Vector2D<float> p2 = new Vector2D<float>(distance, -heightOffset);
        Vector2D<float> p3 = new Vector2D<float>(distance, heightOffset);
        Vector2D<float> p4 = new Vector2D<float>(0, heightOffset);

        float angle = MathF.Atan2(diff.Y, diff.X);

        Matrix4X4<float> rotation = Matrix4X4.CreateRotationZ(angle);
        p1 = Vector2D.Transform(p1, rotation);
        p2 = Vector2D.Transform(p2, rotation);
        p3 = Vector2D.Transform(p3, rotation);
        p4 = Vector2D.Transform(p4, rotation);
        
        p1 += in1;
        p2 += in1;
        p3 += in1;
        p4 += in1;
        
        vb.Add(new Vertex{ Pos = new Vector3D<float>(p1, z), Color = color });
        vb.Add(new Vertex{ Pos = new Vector3D<float>(p2, z), Color = color });
        vb.Add(new Vertex{ Pos = new Vector3D<float>(p3, z), Color = color });
        
        vb.Add(new Vertex{ Pos = new Vector3D<float>(p3, z), Color = color });
        vb.Add(new Vertex{ Pos = new Vector3D<float>(p4, z), Color = color });
        vb.Add(new Vertex{ Pos = new Vector3D<float>(p1, z), Color = color });
    }

    public void DrawTexture9Slice(VertexBuilder vb, float x, float y, float z, int u, int v, int uWidth, int vHeight, int width, int height, int left, int right, int top, int bottom, int textureSize, uint textureIndex){
        //top-left
        DrawTexture(vb, x, y, z, u, v, left, top, textureSize, textureIndex);
        
        //top-right
        DrawTexture(vb, x + width - right, y, z, u + uWidth - right, v, right, top, textureSize, textureIndex);
        
        //bottom-left
        DrawTexture(vb, x, y + height - bottom, z, u, v + vHeight - bottom, left, bottom, textureSize, textureIndex);
        
        //bottom-right
        DrawTexture(vb, x + width - right, y + height - bottom, z, u + uWidth - right, v + vHeight - bottom, right, bottom, textureSize, textureIndex);
        
        //top & bottom
        int horizontalPadding = left + right;
        int maxWidth = uWidth - horizontalPadding;
        for (int i = 0; i < (width - horizontalPadding) / maxWidth + 1; i++) {
            int drawX = i * maxWidth;
            int drawWidth = Math.Min(width - horizontalPadding - drawX, maxWidth);
            DrawTexture(vb, x + drawX + left, y, z, u + right, v, drawWidth, top, 256, textureIndex);
            DrawTexture(vb, x + drawX + left, y + height - bottom, z, u + right, v + vHeight - bottom, drawWidth, bottom, 256, textureIndex);
        }
        
        //left & right
        int verticalPadding = top + bottom;
        int maxHeight = vHeight - verticalPadding;
        for (int i = 0; i < (height - verticalPadding) / maxHeight + 1; i++) {
            int drawY = i * maxHeight;
            int drawHeight = Math.Min(height - verticalPadding - drawY, maxHeight);
            DrawTexture(vb, x, y + drawY + top, z, u, v + top, left, drawHeight, 256, textureIndex);
            DrawTexture(vb, x + width - right, y + drawY + top, z, u + uWidth - right, v + top, right, drawHeight, 256, textureIndex);
        }
        
        //middle
        for (int hor = 0; hor < (width - horizontalPadding) / maxWidth + 1; hor++) {
            for (int vert = 0; vert < (height - verticalPadding) / maxHeight + 1; vert++) {
                int drawX = hor * maxWidth;
                int drawY = vert * maxHeight;
                int drawWidth = Math.Min(width - horizontalPadding - drawX, maxWidth);
                int drawHeight = Math.Min(height - verticalPadding - drawY, maxHeight);
                DrawTexture(vb, x + drawX + left, y + drawY + top, z, u + right, v + top, drawWidth, drawHeight, 256, textureIndex);
            }
        }
    }

}