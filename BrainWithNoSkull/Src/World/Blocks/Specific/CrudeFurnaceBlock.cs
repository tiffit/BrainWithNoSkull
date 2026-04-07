using BrainWithNoSkull.Net;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Server;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.BlockEntities;
using BrainWithNoSkull.World.Blocks.BlockConfigs;
using BrainWithNoSkull.World.Entities.Player;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Blocks.Specific;

public class CrudeFurnaceBlock: NonFullBlock, IMultiblock, IBlockEntityProvider, IBlockInteractable {
    
    private static readonly AABB CollisionBox = new AABB(1/8f, 0, 1/8f, 7/8f, 1 + 6/8f, 7/8f);
    
    public CrudeFurnaceBlock(string id) : base(id){
        ConfigContainer.With(MultiblockBlockConfigType.Instance);
    }

    public Vector3D<int> GetMultiblockSize(){
        return new Vector3D<int>(1, 2, 1);
    }

    public override AABB GetCollisionBox(){
        return CollisionBox;
    }

    public BlockEntity GetBlockEntity(){
        return BlockEntityRegistry.CrudeFurnace.Create();
    }

    public bool CanInteract(WorldBase world, PlayerEntity player, Vector3D<int> pos){
        return true;
    }

    public void Interact(ServerWorld world, ServerPlayerEntity player, Vector3D<int> pos){
        BlockEntity? blockEntity = world.GetBlockEntity(pos);
        if (blockEntity is MachineBlockEntity machine) {
            ContainerSync sync = new MachineContainerSync(player.Client.GetNextContainerId(), machine, player);
            player.Client.OpenContainer(ContainerSyncRegistry.Machine, sync);
        }
    }
}