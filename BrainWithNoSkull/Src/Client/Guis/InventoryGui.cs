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

        Widgets.Add(new InventoryWidget(0.8f, scaledWidth, scaledHeight, _inventory, false));

    }
    
    public override void Render(VertexBuilder guiVb, int mouseX, int mouseY){
    }
    
}