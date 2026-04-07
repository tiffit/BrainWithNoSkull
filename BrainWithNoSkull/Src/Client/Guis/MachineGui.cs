using BrainWithNoSkull.Net;
using BrainWithNoSkull.World.BlockEntities;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Guis;

public class MachineGui: ContainerGui {

    private MachineContainerSync _sync;
    private MachineBlockEntity? _blockEntity;
    
    public MachineGui(ContainerSync sync) : base(sync){
        _sync = (sync as MachineContainerSync)!;
        _sync.MachinePos.OnSync += OnMachinePosUpdate;
    }

    private void OnMachinePosUpdate(Vector3D<int> blockPos){
        _blockEntity = GameClient.Instance.World!.GetBlockEntity(blockPos) as MachineBlockEntity;
    }
    
}