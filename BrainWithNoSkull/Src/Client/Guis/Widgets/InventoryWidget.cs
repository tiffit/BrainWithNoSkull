using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.World.Entities.Player;

namespace BrainWithNoSkull.Client.Guis.Widgets;

public class InventoryWidget: GroupingWidget {
    
    private const int HotbarWidth = 176;
    private const int HotbarHeight = 24;

    private readonly uint _invIconsTexture;
    private readonly PlayerInventory? _inv;
    private readonly bool _hud;

    private readonly int _hotbarX;
    private readonly int _hotbarY;
    private readonly int _invX, _invY, _invYBottom;
    
    public InventoryWidget(float z, int width, int height, Gui parent, PlayerInventory? inv, bool hud) : base(0, 0, z, width, height, parent){
        _hotbarX = width / 2 - HotbarWidth/2;
        _hotbarY = height - HotbarHeight - 2;
        _invIconsTexture = ClientAssetLoader.BlockTextureAtlas.LookupGui("Inv_Icons");
        _hud = hud;
        _inv = inv;
        for (int i = 0; i < 9; i++) {
            int slotX = width / 2 - 9 * 19 / 2 + i*19;
            int slotY = _hotbarY + 3;
            AddWidget(new SlotWidget(slotX - X, slotY - Y, -0.01f, parent, inv, i, _invIconsTexture));
        }

        if (inv != null && !hud) {
            _invX = width / 2 - HotbarWidth/2;
            _invY = height - HotbarHeight - 2 - HotbarHeight;
            _invYBottom = _invY + 39 - HotbarHeight + 5;
            for (int i = 9; i < inv.InventorySize; i++) {
                int slotX = width / 2 - 9 * 19 / 2 + (i % 9)*19;
                int slotY = _invYBottom - (i/9)*19;
                AddWidget(new SlotWidget(slotX - X, slotY - Y, -0.01f, parent, inv, i, _invIconsTexture));
            }
        }
    }

    public override void Render(VertexBuilder guiVb, int mouseX, int mouseY){
        base.Render(guiVb, mouseX, mouseY);
        
        Parent.GuiRenderer.DrawTexture(guiVb, _hotbarX, _hotbarY, Z, 0, 18, HotbarWidth, HotbarHeight, 256, _invIconsTexture);

        if (!_hud && _inv != null) {
            int invRows = (_inv.InventorySize - 9) / 9;
            //bottom
            Parent.GuiRenderer.DrawTexture(guiVb, _invX, _invYBottom - 1, 0.99f, 0, 39, HotbarWidth, 3, 256, _invIconsTexture);
            //middle
            for (int i = 0; i < invRows; i++) {
                int y = _invYBottom - (i + 1) * 19;
                Parent.GuiRenderer.DrawTexture(guiVb, _invX, y, 0.99f, 0, 21, HotbarWidth, 18, 256, _invIconsTexture);
                if (i != 0) {
                    Parent.GuiRenderer.DrawTexture(guiVb, _invX, y + 18, 0.99f, 0, 21, HotbarWidth, 1, 256, _invIconsTexture);
                }
            }
            //top
            Parent.GuiRenderer.DrawTexture(guiVb, _invX, _invYBottom - invRows*19 - 3, 0.99f, 0, 18, HotbarWidth, 3, 256, _invIconsTexture);
        }
    }
}