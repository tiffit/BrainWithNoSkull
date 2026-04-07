using BrainWithNoSkull.Client.Guis.Widgets;
using BrainWithNoSkull.Client.Render;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Net;
using BrainWithNoSkull.World.BlockEntities;
using BrainWithNoSkull.World.Entities.Player;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Guis;

public class MachineGui: ContainerGui {

    private readonly MachineContainerSync _sync;
    private MachineBlockEntity? _blockEntity;
    private ClientPlayerEntity _player = null!;
    
    public MachineGui(ContainerSync sync) : base(sync){
        _sync = (sync as MachineContainerSync)!;
        _sync.MachinePos.OnSync += OnMachinePosUpdate;
    }

    private void OnMachinePosUpdate(Vector3D<int> blockPos){
        _blockEntity = GameClient.Instance.World!.GetBlockEntity(blockPos) as MachineBlockEntity;
        if (_blockEntity != null) {
            ReInit();
        }
    }

    public override void Init(IRenderEngine renderEngine, FontRenderer font, int scaledWidth, int scaledHeight){
        base.Init(renderEngine, font, scaledWidth, scaledHeight);
        uint invIconsTexture = ClientAssetLoader.BlockTextureAtlas.LookupGui("Inv_Icons");
        
        _player = GameClient.Instance.ServerConnection.PlayerEntity!;
        PlayerInventory inv = _player.Inventory;
        Widgets.Add(new InventoryWidget(0.8f, scaledWidth, scaledHeight, this, inv, false));
        
        if (_blockEntity != null) {
            const int width = 150;
            const int height = 100;
            PanelWidget panel;
            Widgets.Add(panel = new PanelWidget(scaledWidth/2 - width/2, scaledHeight/2 - 75, 0.8f, width, height, this));

            int inputCount = _blockEntity.GetInputSlotCount();
            int outputCount = _blockEntity.GetOutputSlotCount();

            for (int i = 0; i < inputCount; i++) {
                panel.AddWidget(new SlotWidget(5 + i*20, height/2 - 9, -0.01f, this, _sync.Container, i, invIconsTexture));
            }
            for (int i = 0; i < outputCount; i++) {
                panel.AddWidget(new SlotWidget(width/2 + 5 + i*20, height/2 - 9, -0.01f, this, _sync.Container, inputCount + i, invIconsTexture));
            }
        }
    }
}