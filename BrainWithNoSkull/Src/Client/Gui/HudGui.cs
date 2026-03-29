using BrainWithNoSkull.Client.Render;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Gui;

public class HudGui : Gui {
    
    private int _fps;
    private int _frameCount;
    private long _lastFrameSecond;

    private uint _invIconsTexture;
    
    public override void Init(IRenderEngine renderEngine, FontRenderer font, int scaledWidth, int scaledHeight){
        base.Init(renderEngine, font, scaledWidth, scaledHeight);
        _invIconsTexture = ClientAssetLoader.BlockTextureAtlas.LookupGui("Inv_Icons");
    }

    public override void Render(VertexBuilder guiVb){
        RenderFps();
        RenderHotbar(guiVb);
    }

    public void RenderHotbar(VertexBuilder vb){
        const int hotbarWidth = 176;
        const int hotbarHeight = 24;
        int hotbarX = ScaledWidth / 2 - hotbarWidth/2;
        int hotbarY = ScaledHeight - hotbarHeight - 2;
        DrawTexture(vb, hotbarX, hotbarY, 0.01f, 0, 18, hotbarWidth, hotbarHeight, 256, _invIconsTexture);

        const int hotbarSlots = 9;
        for (int i = 0; i < hotbarSlots; i++) {
            int slotX = ScaledWidth / 2 - hotbarSlots * 19 / 2 + i*19;
            DrawTexture(vb, slotX, hotbarY + 3, 0, 18, 0, 18, 18, 256, _invIconsTexture);
        }
    }
    
    private void RenderFps(){
        const int fpsPos = 6;
        Font.RenderString(fpsPos - 1, fpsPos, 0.01f, "FPS " + _fps, new Vector3D<float>(0, 0, 0));
        Font.RenderString(fpsPos + 1, fpsPos, 0.01f, "FPS " + _fps, new Vector3D<float>(0, 0, 0));
        Font.RenderString(fpsPos, fpsPos - 1, 0.01f, "FPS " + _fps, new Vector3D<float>(0, 0, 0));
        Font.RenderString(fpsPos, fpsPos + 1, 0.01f, "FPS " + _fps, new Vector3D<float>(0, 0, 0));
        Font.RenderString(fpsPos, fpsPos, 0, "FPS " + _fps, new Vector3D<float>(1, 1, 1));
        
        _frameCount++;
        long nowTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000;
        if (nowTime != _lastFrameSecond) {
            _fps = _frameCount;
            _frameCount = 0;
            _lastFrameSecond = nowTime;
        }
    }
}