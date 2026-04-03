using BrainWithNoSkull.Client.Guis;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.Render.Shader;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Render;

public class GuiRenderer {

    private readonly Gui _gui;
    private readonly IRenderEngine _renderEngine;

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
        //
        // Console.WriteLine(Single.RadiansToDegrees(angle));
        //
        Matrix4X4<float> rotation = Matrix4X4.CreateRotationZ(angle);
        p1 = Vector2D.Transform(p1, rotation);
        p2 = Vector2D.Transform(p2, rotation);
        p3 = Vector2D.Transform(p3, rotation);
        p4 = Vector2D.Transform(p4, rotation);
        //
        // diff = Vector2D.Transform(diff, rotation);
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

}