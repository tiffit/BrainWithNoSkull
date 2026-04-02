using BrainWithNoSkull.Client.Render;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Entities.Player;
using BrainWithNoSkull.World.ItemStacks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Guis;

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
        RenderCrosshair(guiVb);
    }

    private void RenderCrosshair(VertexBuilder vb){
        float crosshairX = (ScaledWidth / 2) - 1f;
        float crosshairY = (ScaledHeight / 2) - 1f;
        DrawTexture(vb, crosshairX, crosshairY, 0.99f, 54, 0, 3, 3, 256, _invIconsTexture);
    }

    private void RenderHotbar(VertexBuilder vb){
        const int hotbarWidth = 176;
        const int hotbarHeight = 24;
        int hotbarX = ScaledWidth / 2 - hotbarWidth/2;
        int hotbarY = ScaledHeight - hotbarHeight - 2;
        DrawTexture(vb, hotbarX, hotbarY, 0.99f, 0, 18, hotbarWidth, hotbarHeight, 256, _invIconsTexture);

        PlayerInventory? inv = GameClient.Instance.ServerConnection.PlayerEntity?.Inventory;
        
        const int hotbarSlots = 9;
        for (int i = 0; i < hotbarSlots; i++) {
            int slotX = ScaledWidth / 2 - hotbarSlots * 19 / 2 + i*19;
            int slotY = hotbarY + 3;
            ItemStack? stack = inv?.Items[i];
            bool hasStack = false;
            if (!ItemStack.IsNull(stack)) {
                GameClient.Instance.ItemRenderer.RenderItemInGui(vb, stack, slotX + 9, slotY + 9, 0.96f, 40, 40);
                if (stack.Count > 1) {
                    Font.RenderString(slotX, slotY, 0.93f, "" + stack.Count, ColorUtil.FromFloats(1f, 0.65f, 0f));
                }
                hasStack = true;
            }
            DrawTexture(vb, slotX, slotY, 0.98f, hasStack ? 0 : 18, 0, 18, 18, 256, _invIconsTexture);
            if ((inv?.HotbarSlot ?? -1) == i) {
                DrawTexture(vb, slotX, slotY, 0.94f, 36, 0, 18, 18, 256, _invIconsTexture);
            }
        }
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
        Font.RenderString(fpsPos - 1, fpsPos, 0.99f, fps, ColorUtil.FromBytes(0, 0, 0));
        Font.RenderString(fpsPos + 1, fpsPos, 0.99f, fps, ColorUtil.FromBytes(0, 0, 0));
        Font.RenderString(fpsPos, fpsPos - 1, 0.99f, fps, ColorUtil.FromBytes(0, 0, 0));
        Font.RenderString(fpsPos, fpsPos + 1, 0.99f, fps, ColorUtil.FromBytes(0, 0, 0));
        Font.RenderString(fpsPos, fpsPos, 0, fps, ColorUtil.FromBytes(255, 255, 255));
        
        _frameCount++;
        long nowTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000;
        if (nowTime != _lastFrameSecond) {
            _fps = _frameCount;
            _frameCount = 0;
            _lastFrameSecond = nowTime;
        }
    }
}