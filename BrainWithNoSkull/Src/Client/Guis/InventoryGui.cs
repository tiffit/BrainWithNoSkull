using BrainWithNoSkull.Client.Render;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.World.Entities.Player;

namespace BrainWithNoSkull.Client.Guis;

public class InventoryGui: Gui {

    private uint _invIconsTexture;
    
    public override void Init(IRenderEngine renderEngine, FontRenderer font, int scaledWidth, int scaledHeight){
        base.Init(renderEngine, font, scaledWidth, scaledHeight);
        _invIconsTexture = ClientAssetLoader.BlockTextureAtlas.LookupGui("Inv_Icons");
    }
    
    public override void Render(VertexBuilder guiVb){
        ClientPlayerEntity? player = GameClient.Instance.ServerConnection.PlayerEntity;
        PlayerInventory? inv = player?.Inventory;
        if (inv != null) {
            RenderInventory(guiVb, inv, player!);
        }
    }

    private void RenderInventory(VertexBuilder vb, PlayerInventory inv, ClientPlayerEntity player){
        int invRows = (inv.InventorySize - 9) / 9;
        const int hotbarWidth = 176;
        const int hotbarHeight = 24;
        int invX = ScaledWidth / 2 - hotbarWidth/2;
        int invY = ScaledHeight - hotbarHeight - 2 - hotbarHeight;

        int invYBottom = invY + 39 - hotbarHeight + 5;
        //bottom
        DrawTexture(vb, invX, invYBottom - 1, 0.99f, 0, 39, hotbarWidth, 3, 256, _invIconsTexture);
        //middle
        for (int i = 0; i < invRows; i++) {
            int y = invYBottom - (i + 1) * 19;
            DrawTexture(vb, invX, y, 0.99f, 0, 21, hotbarWidth, 18, 256, _invIconsTexture);
            if (i != 0) {
                DrawTexture(vb, invX, y + 18, 0.99f, 0, 21, hotbarWidth, 1, 256, _invIconsTexture);
            }
        }
        //top
        DrawTexture(vb, invX, invYBottom - invRows*19 - 3, 0.99f, 0, 18, hotbarWidth, 3, 256, _invIconsTexture);

        for (int i = 9; i < inv.InventorySize; i++) {
            int slotX = ScaledWidth / 2 - 9 * 19 / 2 + (i % 9)*19;
            int slotY = invYBottom - (i/9)*19;
            DrawSlot(vb, inv, i, slotX, slotY, 0.96f, _invIconsTexture);
        }
    }
    
}