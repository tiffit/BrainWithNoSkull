using BrainWithNoSkull.Client.Guis.Widgets;
using BrainWithNoSkull.Client.Render;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.World.Entities.Player;

namespace BrainWithNoSkull.Client.Guis;

public class InventoryGui: Gui {

    private uint _invIconsTexture;
    private PlayerInventory? _inventory;
    private ClientPlayerEntity? _player;
    
    public override void Init(IRenderEngine renderEngine, FontRenderer font, int scaledWidth, int scaledHeight){
        base.Init(renderEngine, font, scaledWidth, scaledHeight);
        _invIconsTexture = ClientAssetLoader.BlockTextureAtlas.LookupGui("Inv_Icons");
        
        _player = GameClient.Instance.ServerConnection.PlayerEntity;
        _inventory = _player?.Inventory;

        InventoryWidget invWidget;
        Widgets.Add(invWidget = new InventoryWidget(0.8f, scaledWidth, scaledHeight, this, _inventory, false));
        int craftingHeight = invWidget.GetTop() - 20;
        const int craftingWidth = 150;
        Widgets.Add(new InventoryCraftingWidget(ScaledWidth/2 - craftingWidth/2, 10, 0.8f, craftingWidth, craftingHeight, this));
    }
    
    public override void Render(VertexBuilder guiVb, int mouseX, int mouseY){
    }
    
}