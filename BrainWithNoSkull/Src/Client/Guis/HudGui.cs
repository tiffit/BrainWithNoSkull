using BrainWithNoSkull.Client.Guis.Widgets;
using BrainWithNoSkull.Client.Render;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Entities.Player;

namespace BrainWithNoSkull.Client.Guis;

public class HudGui : Gui {
    
    private int _fps;
    private int _frameCount;
    private long _lastFrameSecond;

    private uint _invIconsTexture;
    
    public override void Init(IRenderEngine renderEngine, FontRenderer font, int scaledWidth, int scaledHeight){
        base.Init(renderEngine, font, scaledWidth, scaledHeight);
        _invIconsTexture = ClientAssetLoader.BlockTextureAtlas.LookupGui("Inv_Icons");
        PlayerInventory? inv = GameClient.Instance.ServerConnection.PlayerEntity?.Inventory;
        Widgets.Add(new InventoryWidget(0.99f, scaledWidth, scaledHeight, this, inv, true));
    }

    public override void Render(VertexBuilder guiVb, int mouseX, int mouseY){
        RenderFps();
        RenderCrosshair(guiVb);
    }

    private void RenderCrosshair(VertexBuilder vb){
        float crosshairX = (ScaledWidth / 2) - 1f;
        float crosshairY = (ScaledHeight / 2) - 1f;
        GuiRenderer.DrawTexture(vb, crosshairX, crosshairY, 0.99f, 54, 0, 3, 3, 256, _invIconsTexture);
    }
    
    private void RenderFps(){
        const int fpsPos = 6;
        ClientPlayerEntity? player = GameClient.Instance.ServerConnection.PlayerEntity;
        string fps = $"FPS {_fps}";
        if (player != null) {
            int x = (int)player.Position.X;
            int y = (int)player.Position.Y;
            int z = (int)player.Position.Z;
            fps = $"{fps} ({x}, {y}, {z})";
        }
        Font.RenderString(fpsPos - 1, fpsPos, 0.999f, fps, ColorUtil.FromBytes(0, 0, 0));
        Font.RenderString(fpsPos + 1, fpsPos, 0.999f, fps, ColorUtil.FromBytes(0, 0, 0));
        Font.RenderString(fpsPos, fpsPos - 1, 0.999f, fps, ColorUtil.FromBytes(0, 0, 0));
        Font.RenderString(fpsPos, fpsPos + 1, 0.999f, fps, ColorUtil.FromBytes(0, 0, 0));
        Font.RenderString(fpsPos, fpsPos, 0, fps, ColorUtil.FromBytes(255, 255, 255));
        
        _frameCount++;
        long nowTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000;
        if (nowTime != _lastFrameSecond) {
            _fps = _frameCount;
            _frameCount = 0;
            _lastFrameSecond = nowTime;
        }
        
        GameClient client = GameClient.Instance;
        int[] counts = client.LastRenderedChunkCount;
        string chunkCount = $"{counts[0]}/{client.TotalChunkMeshCount}";
        Font.RenderString(fpsPos, fpsPos + 10, 0, chunkCount, ColorUtil.FromBytes(255, 255, 255));
        string shadowCount = $"{counts[1]} {counts[2]} {counts[3]} {counts[4]}";
        Font.RenderString(fpsPos, fpsPos + 20, 0, shadowCount, ColorUtil.FromBytes(255, 255, 255));
    }
}